using CesiumForUnity;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using Unity.Mathematics;
using System.Collections;

public class MapController : MonoBehaviour
{
    public CesiumBingMapsRasterOverlay cesiumBingMapsOverlay;
    public CameraController cameraController;
    public TMP_InputField latInputField;
    public TMP_InputField longInputField;
    public GameObject loadingPanel;
    public TextMeshProUGUI loadingText;
    public WeatherController weatherController;
    public delegate void OnMapLocationChangedHandler();
    public static event OnMapLocationChangedHandler MapLocationChanged;
    public CesiumGeoreference cesiumGeoRef;
    public Transform[] GameObjectContainers;
    private CesiumWaveNormalsHandling waveHandler;
    private DemoAssetsController demoAssetsController;
    private bool firstLocation = true;
    private bool allGeoObjectInstantiated;
    private readonly string[] terrainLayer = {GeoConsts.RAYCAST_LAYER_TERRAIN};
    // private readonly string[] terrainAndCCULayer = {GeoConsts.RAYCAST_LAYER_TERRAIN, GeoConsts.RAYCAST_LAYER_CCU_STORAGE};

    void Awake() {
        // Initialize building and terrain layer logic
        CesiumConstants.customBuildingTileset = transform.Find("CustomBuildingTileset").GetComponent<Cesium3DTileset>();
        CesiumConstants.osmBuildingTileset = transform.Find("OSMBuildingTileset").GetComponent<Cesium3DTileset>();
        CesiumConstants.bingMapsTileset = transform.Find("BingMapsTerrainTileset").GetComponent<Cesium3DTileset>();
        CesiumConstants.googleEarthTileset = transform.Find("GoogleEarthTerrainTileset").GetComponent<Cesium3DTileset>();
        CesiumConstants.SetBuildingTilesetType(BuildingTilesetType.openStreetMap);
        CesiumConstants.SetTerrainTilesetType(TerrainTilesetType.bingMaps);
        cesiumGeoRef = GetComponent<CesiumGeoreference>();
        demoAssetsController = GetComponent<DemoAssetsController>();
        waveHandler = GetComponentInChildren<CesiumWaveNormalsHandling>();
        GeoPositionManager.GeoPositionsLoaded += OnGeoPositionsLoaded;
    }

    void Start() {
        SetMapStyle(BingMapsStyle.CanvasLight);
    }

    private void OnGeoPositionsLoaded() {
        if (allGeoObjectInstantiated)
            return;

        foreach (Transform container in GameObjectContainers)
            StartCoroutine(RefManager.Get<MapController>().AlignGeoObjectHeightToMapGeometry(container));
        allGeoObjectInstantiated = true;
    }

    private void ClearContainers() {
        foreach (Transform container in GameObjectContainers) {
            StopCoroutine(RefManager.Get<MapController>().AlignGeoObjectHeightToMapGeometry(container));
            foreach(Transform tf in container)
                Destroy(tf.gameObject);
        }
        allGeoObjectInstantiated = false;
    }

    public void GoToLocation(LocationParameters lp) {
        cesiumGeoRef.latitude = lp.latitude;
        cesiumGeoRef.longitude = lp.longitude;
        GeoPositionManager.currentSiteId = lp.siteId;
        GeoPositionManager.currentAPIKey = lp.apiKey;
        GeoPositionManager.currentHubName = lp.signalRHub;
        CameraController.cameraYLimit = lp.cameraYLimit;
        cameraController.ResetCameraPosition(true);
        RealTimeUpdateUIController.rtcInstance.ClearLog();
        RefManager.Get<BrowseGeoObjectsUIController>().Reset();
        demoAssetsController.TryEnableDemoAsset(lp.demoAssetBundle);

        if (firstLocation) {
            firstLocation = false;
            RefManager.Get<GeoPositionManager>().Init();
        } else {
            ClearContainers();
            RefManager.Get<GeoPositionManager>().ClearAllDataStructures();
        }
        
        if (lp.name != null) {
            loadingText.text = "Loading " + lp.name + "..";
            loadingPanel.SetActive(true);
            Waiter.Wait(5, () => {
                loadingPanel.SetActive(false);
            });
        }
        MapLocationChanged.Invoke();    
    } 

    public void OnMapSourceToggled(Toggle bingOSMToggle) { // Only called by the high toggle it's in a toggle group so we don't need to listen for the low toggle
        TerrainTilesetType type = bingOSMToggle.isOn ? TerrainTilesetType.bingMaps : TerrainTilesetType.GoogleEarth;
        CesiumConstants.SetTerrainTilesetType(type);
    }

    public void OnMapStyleChanged(TextMeshProUGUI text) {
        switch (text.text.Trim()) {
            case "Day":
                SetMapStyle(BingMapsStyle.CanvasLight);
                break;
            case "Evening":
                SetMapStyle(BingMapsStyle.CanvasGray);
                break;
            case "Night":
                SetMapStyle(BingMapsStyle.CanvasDark);
                break;
            default:
                SetMapStyle(BingMapsStyle.CanvasGray);
                break;
        }
    }

    private void SetMapStyle(BingMapsStyle style) {
        weatherController.SetSkyboxFromMapStyle(style);
        cesiumBingMapsOverlay.mapStyle = style;
        waveHandler.SetOceanStyle(style);      
    }

    private IEnumerator AlignGeoObjectHeightToMapGeometry(Transform container) {    
        while (true) {
            foreach (Transform geoTransform in container) {        
                RayCastFromGeoObjectToGround(geoTransform, terrainLayer);
                yield return GeoConsts.INTER_OBJECT_TICK_RATE;
            }  
            yield return GeoConsts.ALIGN_TO_TERRAIN_INTERVAL;                          
        }
    }

    private static void RayCastFromGeoObjectToGround(Transform geoTransform, string[] layerNames) {
        // Align Y-position to the ground by raycasting
        Vector3 origin = new Vector3(geoTransform.position.x, geoTransform.position.y + GeoConsts.RAYCAST_ORIGIN_OFFSET, geoTransform.position.z);
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, GeoConsts.RAYCAST_LENGTH, LayerMask.GetMask(layerNames))) {
            geoTransform.position = new Vector3(geoTransform.position.x, hit.point.y, geoTransform.position.z);
        }
    }

    // CESIUM UTILITY FUNCTIONS //
    // public Vector3 FindHeightAdjustedPosition(GeoObjectInstance goi, double latitude, double longitude) {
    //     Vector3 newPosition = LatLongToUnityPosition(latitude, longitude);
    //     if (Physics.Raycast(newPosition, Vector3.down, out RaycastHit downHit, GeoConsts.RAYCAST_LENGTH, LayerMask.GetMask(GeoConsts.RAYCAST_LAYER_TERRAIN)))
    //         newPosition = downHit.point;
    //     else
    //         newPosition.y = goi.transform.position.y; // Use same height as previous Y position
    //     return newPosition;
    // }

    public Vector3 FindHeightAdjustedPosition(GeoLocation gl, GeoObjectInstance goi) {
        Vector3 newPosition = LatLongToUnityPosition((double)gl.Latitude, (double)gl.Longitude);
        Vector3 origin = new Vector3(newPosition.x, newPosition.y + GeoConsts.RAYCAST_ORIGIN_OFFSET, newPosition.z);
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, GeoConsts.RAYCAST_LENGTH, LayerMask.GetMask(GeoConsts.RAYCAST_LAYER_TERRAIN)))
            newPosition = new Vector3(newPosition.x, hit.point.y, newPosition.z);
        else
            newPosition = new Vector3(newPosition.x, goi.transform.position.y, newPosition.z); // Use same height as previous Y position
        return newPosition;
    }

    public (double, double) UnityPositionToLatLong(Vector3 pos) {
        double3 ecef = cesiumGeoRef.TransformUnityPositionToEarthCenteredEarthFixed(new double3((double)pos.x, (double)pos.y, (double)pos.z));
        double3 lonLatHeight = CesiumWgs84Ellipsoid.EarthCenteredEarthFixedToLongitudeLatitudeHeight(ecef);
        return (lonLatHeight.y, lonLatHeight.x);
    }

    public Vector3 LatLongToUnityPosition(double latitude, double longitude) {
        double3 lonLatHeight = new double3(longitude, latitude, 0.0);
        double3 ecef = CesiumWgs84Ellipsoid.LongitudeLatitudeHeightToEarthCenteredEarthFixed(lonLatHeight);
        double3 unityCoordinates = cesiumGeoRef.TransformEarthCenteredEarthFixedPositionToUnity(ecef);
        return new Vector3((float)unityCoordinates.x, (float)unityCoordinates.y, (float)unityCoordinates.z);
    }

    void OnDestroy() {
        GeoPositionManager.GeoPositionsLoaded -= OnGeoPositionsLoaded;
    }
}
