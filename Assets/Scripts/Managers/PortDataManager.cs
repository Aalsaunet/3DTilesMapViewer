using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class PortDataManager : MonoBehaviour
{
    public Transform portObjectContainer;
    ////// PUBLIC PREFABS ///// 
    public GameObject OkIconPrefab;
    public GameObject WarningIconPrefab;
    public GameObject NoIconPrefab;
    public GameObject bollardPrefab;
    public Material dockFrontMaterial;
    private GeoBounds bounds;
    private static CultureInfo cultureInfo;
    
    void Awake() {
        MapController.MapLocationChanged += OnMapLocationChanged;
        GeoPositionManager.GeoPositionsLoaded += OnGeoPositionsLoaded;
        cultureInfo = new CultureInfo("en-US");
    }

    private void OnMapLocationChanged() {
        RealTimeUpdateUIController.rtcInstance.PrintLogMessage("[NRT][KV] Fetching port data from Kartverket");
        bounds = BarentsWatchService.FindTwoCoordinateBounds();
        FetchPortData("all_havnedata_fortoyningsinnredning.xml", APIConsts.KARTVERKET_FORTOYNINGSINNREDNING_URL, ParseBollardPortData);
        FetchPortData("all_havnedata_kaifront.xml", APIConsts.KARTVERKET_KAIFRONT_URL, ParseDockFrontPortData);
    }

    private void OnGeoPositionsLoaded() {
        RealTimeUpdateUIController.rtcInstance.PrintLogMessage("[NRT][KV] Fetching maintenance data");
        if (GeoPositionManager.currentSiteId != null && Guid.TryParse(GeoPositionManager.currentSiteId, out var siteId))
            StartCoroutine(FetchMaintenanceData(siteId));
    } 

    private IEnumerator FetchMaintenanceData(Guid siteId) {
        dynamic response;
        int daysAhead = 7;
        string statuses = "[\"created\", \"accepted\", \"actioncreated\"]";
        string requestBody = "{ \"siteId\": \"" + siteId + "\", \"daysAhead\": " + daysAhead + ", \"statuses\": " + statuses + " }";

        using (UnityWebRequest request = UnityWebRequest.Post(APIConsts.URL_MAINTENANCE, requestBody, "application/json")) {
            request.SetRequestHeader("Api-key", APIConsts.MAINTENANCE_API_KEY);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError)
                Debug.Log(request.error);  
            // Debug.Log(request.downloadHandler.text);  
            response = JsonConvert.DeserializeObject<dynamic>(request.downloadHandler.text);
        }

        if (response == null || response.maintenanceRequests == null) {
            Debug.Log("Empty maintenanceRequests list. Returning.");
            yield break;
        }

        HashSet<Guid?> objectIds = GeoPositionManager.sourceEntityIdToGoi.Keys.ToHashSetPooled();
        foreach(var mr in response.maintenanceRequests) {
            Guid objectId = new((string)mr.objectId);
            if (objectIds.Contains(objectId)) {
                objectIds.Remove(objectId);
                var goi = GeoPositionManager.sourceEntityIdToGoi[objectId];
                if ((string)mr.requestTypeName == "OOS") {
                    goi.maintenanceStatus = MaintenanceStatus.OUT_OF_SERVICE;
                    InstantiateMaintenanceIcon(NoIconPrefab, goi);
                } else {
                    goi.maintenanceStatus = MaintenanceStatus.NEED_SERVICE;
                    InstantiateMaintenanceIcon(WarningIconPrefab, goi);
                }
            } else {
                //InstantiateMaintenanceIcon(OkIconPrefab, goi);
            }
        }

        foreach (var objectId in objectIds) {
            var goi = GeoPositionManager.sourceEntityIdToGoi[objectId];
            goi.maintenanceStatus = MaintenanceStatus.OK;
            InstantiateMaintenanceIcon(OkIconPrefab, goi);
        }

        yield return null;
    }

    private void InstantiateMaintenanceIcon(GameObject iconGO, GeoObjectInstance goi) {
        if (goi.isMaintenanceStatusSet)
            return;
        
        var iconHolder = goi.transform.Find("MaintenanceIcons");
        if (iconHolder == null)
            return;

        Instantiate(iconGO, iconHolder);
        goi.isMaintenanceStatusSet = true;
        
        // var collider = goi.GetComponent<BoxCollider>();
        // if (collider != null) {
        //     Vector3 position = iconInstance.transform.position + new Vector3(0f, collider.bounds.size.y * 1.25f, 0f);
        //     iconInstance.transform.position = position;
        // }
    }

    private void FetchPortData(string filename, string url, Action<string> ParseFunction) {
        if (File.Exists(Path.Combine(Application.streamingAssetsPath, filename))) {
            string text = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, filename));
            if (text != null && text.Length > 0) {
                RealTimeUpdateUIController.rtcInstance.PrintLogMessage($"[NRT][KV] Reading Havnedata from file: {filename}");
                ParseFunction(text);
            } else {
                RealTimeUpdateUIController.rtcInstance.PrintLogMessage($"[NRT][KV] Reading Havnedata from remote: {url}");
                StartCoroutine(FetchRemotePortData(url, ParseFunction));
            }   
        } else {
            RealTimeUpdateUIController.rtcInstance.PrintLogMessage($"[NRT][KV] Reading Havnedata from remote: {url}");
            StartCoroutine(FetchRemotePortData(url, ParseFunction));
        }
    }

    public IEnumerator FetchRemotePortData(string url, Action<string> ParseFunction) {
        using (UnityWebRequest request = UnityWebRequest.Get(url)) {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError)
                Debug.Log(request.error);
            ParseFunction(request.downloadHandler.text);
        }
    }

    private void ParseBollardPortData(string content) {
        XDocument portData = XDocument.Parse(content);
        RealTimeUpdateUIController.rtcInstance.PrintLogMessage("[NRT][KV] Parsed BollardPortData (Havnedata - Fortøyningsinnredning) into XDocument");
        string app = "http://skjema.geonorge.no/SOSI/produktspesifikasjon/Havnedata/2.0";
        string gml = "http://www.opengis.net/gml/3.2";
        string ft = "{" + app + "}fortøyningstype";
        string ln = "{" + app + "}objektLøpenummer";
        string mb = "{" + app + "}maksbelastning";
        string sd = "{" + app + "}sertifiseringsdato";
        string appPos = "{" + app + "}posisjon";
        string point = "{" + gml + "}Point";
        string gmlPos = "{" + gml + "}pos";
        int pullertCount = 0;

        try {
            foreach(var element in portData.Elements().First().Elements()) {    
                if (element.Elements().First().Element(ft).Value == "pullert") {
                    string[] latLongStr = element.Elements().First().Element(appPos).Element(point).Element(gmlPos).Value.Split();
                    var (latitude, longitude) = (double.Parse(latLongStr[0], cultureInfo), double.Parse(latLongStr[1], cultureInfo));
                    if (bounds.Contains(latitude, longitude)) {
                        RealTimeUpdateUIController.rtcInstance.PrintLogMessage("[NRT][KV] Found entry within bounds. Instantiating it");
                        Vector3 position = RefManager.Get<MapController>().LatLongToUnityPosition(latitude, longitude);
                        GameObject geoGameObject = Instantiate(bollardPrefab, position, Quaternion.identity, portObjectContainer);
                        GeoObjectInstance goi = geoGameObject.AddComponent<GeoObjectInstance>();
                        string trackingId = element?.Elements()?.First().Element(ln)?.Value ?? "[NOT FOUND]";
                        string maxLoad = element?.Elements()?.First().Element(mb)?.Value ?? "[NOT FOUND]";
                        string certificationDate = element?.Elements()?.First().Element(sd)?.Value ?? "[NOT FOUND]";
                        goi.info = new PortObjectInfo($"Pullert #{++pullertCount}", trackingId, "Pullert", maxLoad, certificationDate);
                        geoGameObject.name = goi.info.DisplayName; 
                        geoGameObject.tag = GeoConsts.GEO_OBJECT_TAG;
                    }
                }
            }        
        } catch (Exception e) {
            RealTimeUpdateUIController.rtcInstance.PrintLogMessage($"[NRT][KV] An exception occured when parsing BollardPortData (Havnedata - Fortøyningsinnredning): {e.Message}");
        }   
        

        RealTimeUpdateUIController.rtcInstance.PrintLogMessage("[NRT][KV] Successfully parsed BollardPortData (Havnedata - Fortøyningsinnredning)");
    }

    private void ParseDockFrontPortData(string content) {
        XDocument portData = XDocument.Parse(content);
        RealTimeUpdateUIController.rtcInstance.PrintLogMessage("[NRT][KV] Parsed DockFrontPortData (Havnedata - Kaifront) into XDocument");
        string app = "http://skjema.geonorge.no/SOSI/produktspesifikasjon/Havnedata/2.0";
        string gml = "http://www.opengis.net/gml/3.2";
        string appPos = "{" + app + "}senterlinje";
        string point = "{" + gml + "}LineString";
        string gmlPos = "{" + gml + "}posList";
        int kaifrontCount = 0;

        try {
            foreach(var element in portData.Elements().First().Elements()) {    
                string[] latlongArray = element.Elements().First().Element(appPos).Element(point).Element(gmlPos).Value.Split();
                var (latitude, longitude) = (double.Parse(latlongArray[0], cultureInfo), double.Parse(latlongArray[1], cultureInfo));
                if (bounds.Contains(latitude, longitude)) {  
                    RealTimeUpdateUIController.rtcInstance.PrintLogMessage("[NRT][KV] Found entry within bounds. Instantiating it");        
                    GameObject dockFrontGO = new GameObject($"Kaifront {++kaifrontCount}");
                    dockFrontGO.transform.parent = portObjectContainer;

                    List<Vector3> positions = new List<Vector3>();
                    for (int i = 0; i < latlongArray.Length; i += 2) {
                        Vector3 position = RefManager.Get<MapController>().LatLongToUnityPosition(double.Parse(latlongArray[i], cultureInfo), double.Parse(latlongArray[i + 1], cultureInfo));
                        position = new Vector3(position.x, position.y + 0.1f, position.z);
                        positions.Add(position);
                    }

                    LineRenderer lineRenderer = dockFrontGO.AddComponent<LineRenderer>();
                    lineRenderer.material = new Material(dockFrontMaterial);
                    lineRenderer.startColor = lineRenderer.endColor = Color.black;
                    lineRenderer.widthMultiplier = 0.5f;
                    lineRenderer.useWorldSpace = false;
                    lineRenderer.positionCount = positions.Count;
                    lineRenderer.SetPositions(positions.ToArray());
                    lineRenderer.numCornerVertices = 5;
                }
            }
        } catch (Exception e) {
            RealTimeUpdateUIController.rtcInstance.PrintLogMessage($"[NRT][KV] An exception occured when parsing DockFrontPortData (Havnedata - Kaifront): {e.Message}");
        }
        
        RealTimeUpdateUIController.rtcInstance.PrintLogMessage("[NRT][KV] Successfully parsed DockFrontPortData (Havnedata - Kaifront)");
    }

    public void OnToggleDisplayPortDataObjects(Toggle toggle) { 
        portObjectContainer.gameObject.SetActive(toggle.isOn);
    }

    void OnDestroy() {
        MapController.MapLocationChanged -= OnMapLocationChanged;
        GeoPositionManager.GeoPositionsLoaded -= OnGeoPositionsLoaded;
    }
}
