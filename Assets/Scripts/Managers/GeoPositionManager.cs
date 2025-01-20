using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System;
using System.Linq;
using Microsoft.IdentityModel.Tokens;

public class GeoPositionManager : MonoBehaviour
{
    ////// UNITY REFERENCES //////
    public Transform geoObjectContainer;
    public Transform cameraController;
    public GameObject missingObjectPrefab;
    
    ////// DATA STORES //////
    public static Dictionary<string, GameObject> geoIdToGameObject; 
    public static Dictionary<string, GeoObjectInstance> trackingIdToGeoObjectInstance;
    public static Dictionary<string, List<GeoObjectInstance>> categoryCodeToGeoObjects;
    public static Dictionary<string, string> trackingIdToGeoId;
    public static Dictionary<Guid?, GeoObjectInstance> sourceEntityIdToGoi;
    public static Trie geoObjectNameTrie;
    public static Trie trackingIdTrie;

    ////// NETWORKING //////
    public static string currentSiteId; 
    public static string currentAPIKey; 
    public static string currentHubName; 

    ////// EVENTS AND DELEGATES //////
    public delegate void OnGeoPositionsLoadedHandler();
    public static event OnGeoPositionsLoadedHandler GeoPositionsLoaded; // GeoObject from the GeoPositions received and instantiated

    void Awake() {
        // Data stores
        geoIdToGameObject = new Dictionary<string, GameObject>();
        trackingIdToGeoObjectInstance = new Dictionary<string, GeoObjectInstance>();
        categoryCodeToGeoObjects = new Dictionary<string, List<GeoObjectInstance>>();
        trackingIdToGeoId = new Dictionary<string, string>();
        sourceEntityIdToGoi = new Dictionary<Guid?, GeoObjectInstance>();
        geoObjectNameTrie = new Trie();
        trackingIdTrie = new Trie();
    }

    public void Init() {
        StartCoroutine(FetchAndInstantiateGeoObjects());
    }

    public IEnumerator FetchAndInstantiateGeoObjects() {
        while (true) {
            var (latitude, longitude) = RefManager.Get<MapController>().UnityPositionToLatLong(cameraController.position);
            StartCoroutine(FetchPositionsAndInstantiateGeoObjects(latitude, longitude, currentAPIKey));
            if (RealTimeUpdateUIController.rtcInstance != null)
                RealTimeUpdateUIController.rtcInstance.PrintLogMessage("[NRT] Fetching updated GeoPositions.");
            yield return GeoConsts.GEO_UPDATE_INTERVAL;
        }
    }

    public IEnumerator FetchPositionsAndInstantiateGeoObjects(double latitude, double longitude, string apiKey) {
        List<GeoPosition> geoPositions;
        // HashSet<string> geoObjectGUIDsToRemove = geoIdToGameObject.Keys.ToHashSet();
        string latString = latitude.ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
        string longString = longitude.ToString(System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
        string requestBody = "{ \"latitude\": " + latString + ", \"longitude\": " + longString + ", \"radius\": " + GeoConsts.GEO_POSITION_RADIUS + " }";

        using (UnityWebRequest request = UnityWebRequest.Post(APIConsts.URL_POSITIONS, requestBody, "application/json")) {
            request.SetRequestHeader("Api-key", apiKey);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError)
                Debug.Log(request.error);  
            // Debug.Log(request.downloadHandler.text);  
            geoPositions = JsonConvert.DeserializeObject<List<GeoPosition>>(request.downloadHandler.text);
        }

        if (geoPositions.IsNullOrEmpty()) {
            Debug.Log("Empty GeoPosition list. Returning.");
            yield break;
        }
   
        for (int i = 0; i < geoPositions.Count; i++) {
            // if (geoObjectGUIDsToRemove.Contains(geoPositions[i].Id.ToString()))
            //     geoObjectGUIDsToRemove.Remove(geoPositions[i].Id.ToString());

            if (geoIdToGameObject.ContainsKey(geoPositions[i].Id.ToString())) continue;
            if (geoPositions[i].CurrentLatitude == null || geoPositions[i].CurrentLongitude == null) continue;

            Vector3 position = RefManager.Get<MapController>().LatLongToUnityPosition((double)geoPositions[i].CurrentLatitude, (double)geoPositions[i].CurrentLongitude);
            Quaternion rotation = (geoPositions[i].Direction != null) ? Quaternion.Euler(0.0f, (float)geoPositions[i].Direction, 0.0f) : Quaternion.Euler(0f, 0f, 0f);
            geoPositions[i].CategoryCode = geoPositions[i].CategoryCode?.ToUpper().Trim();
            geoPositions[i].GeoModelCode = geoPositions[i].GeoModelCode?.Trim();

            switch (geoPositions[i].CategoryCode) {
                case CategoryCode.CCUSTACKCHARTROW: StartCoroutine(RefManager.Get<CCUManager>().FetchAndInstantiateStackingChart(geoPositions[i], currentAPIKey, position, rotation)); break;             
                // case CategoryCode.CARGO_ITEM_TYPE: RefManager.Get<CCUManager>().RegisterStandaloneCCU(geoPositions[i], position, rotation); break;
                // case CategoryCode.WAREHOUSEENTRY: RefManager.Get<CCUManager>().RegisterStandaloneCCU(geoPositions[i], position, rotation); break;
                case CategoryCode.WAREHOUSEENTRY: RefManager.Get<CCUManager>().InstantiateStandaloneCCU(geoPositions[i], position, rotation); break;
                // case CategoryCode.CCU: RefManager.Get<CCUManager>().RegisterStandaloneCCU(geoPositions[i], position, rotation); break;
                case CategoryCode.TRUCK: RefManager.Get<TruckManager>().InstantiateTruck(geoPositions[i], position, rotation); break;
                case CategoryCode.VESSEL: RefManager.Get<VesselManager>().InstantiateVessel(geoPositions[i], position, rotation, true); break;
                case CategoryCode.FORKLIFT: RefManager.Get<PortVehicleManager>().InstantiatePortObject(geoPositions[i], position, rotation); break;
                case CategoryCode.CRANE: RefManager.Get<PortVehicleManager>().InstantiatePortObject(geoPositions[i], position, rotation); break;
                case CategoryCode.STATIC: RefManager.Get<StaticObjectManager>().InstantiateStaticObject(geoPositions[i], position, rotation); break;
                case CategoryCode.LOCATION: RefManager.Get<GeoLocationManager>().InstantiateGeoLocationObject(geoPositions[i], position); break;
                case CategoryCode.SUBLOCATION: RefManager.Get<GeoLocationManager>().InstantiateGeoLocationObject(geoPositions[i], position); break;
                case CategoryCode.QUAY: RefManager.Get<GeoLocationManager>().InstantiateGeoLocationObject(geoPositions[i], position); break;
                default: break; //var missing = Instantiate(missingObjectPrefab, position, rotation, geoObjectContainer); missing.name = geoPositions[i].DisplayName; break;
            }
            yield return GeoConsts.INTER_OBJECT_TICK_RATE;
        }

        // Remove GeoObject that weren't "mentioned" by the GeoPositions API result and thus are no longer within the search parameters
        // foreach (string id in geoObjectGUIDsToRemove)
        //     RemoveGeoObject(id);
        
        GeoPositionsLoaded.Invoke();
    }

    public static void AddToGeoPositionDataStructures(string id, GeoPosition gpos, GameObject ccuObject, GeoObjectInstance goi) {
        geoIdToGameObject.Add(id, ccuObject);
        geoObjectNameTrie.Insert(goi.info.DisplayName, goi);
        BrowseGeoObjectsUIController.AddEntry(gpos.CategoryCode, goi);

        if (goi.info.TrackingId != null && !trackingIdToGeoId.ContainsKey(goi.info.TrackingId)) {
            trackingIdToGeoId.Add(goi.info.TrackingId, id);
            trackingIdToGeoObjectInstance.Add(goi.info.TrackingId, goi);
            trackingIdTrie.Insert(goi.info.TrackingId, goi);
        }

        if (gpos.SourceEntityId != null && !sourceEntityIdToGoi.ContainsKey(gpos.SourceEntityId) && gpos.CategoryCode == CategoryCode.CRANE) {
            sourceEntityIdToGoi.Add(gpos.SourceEntityId, goi);
        }
    }

    public static void RemoveGeoObject(string id) {
        GameObject goToRemove = geoIdToGameObject[id];
        GeoObjectInstance goi = goToRemove.GetComponent<GeoObjectInstance>();
        if (!goi.info.IsFromGeoPositionAPI)
            return;

        geoIdToGameObject.Remove(id);
        geoObjectNameTrie.Delete(goi.info.DisplayName);

        if (goi.info.TrackingId != null)  {
            trackingIdToGeoId.Remove(goi.info.TrackingId);
            trackingIdTrie.Delete(goi.info.TrackingId);
            trackingIdToGeoObjectInstance.Remove(goi.info.TrackingId);
        }
        
        Destroy(goToRemove);
    }

    public void ClearAllDataStructures() {
        StopCoroutine(FetchAndInstantiateGeoObjects());
        geoIdToGameObject.Clear();
        categoryCodeToGeoObjects.Clear();
        trackingIdToGeoObjectInstance.Clear();
        trackingIdToGeoId.Clear();
        sourceEntityIdToGoi.Clear();
        GeoLocationManager.geoFenceColorValues.Clear();
        geoObjectNameTrie = new Trie();
        trackingIdTrie = new Trie();
        StartCoroutine(FetchAndInstantiateGeoObjects());
    }
}
