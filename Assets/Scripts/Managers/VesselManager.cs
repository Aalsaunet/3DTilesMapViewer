using UnityEngine;
using System.Collections.Generic;

public class VesselManager : MonoBehaviour
{
    // VESSELS PREFABS //
    public GameObject cruiseShipPrefab;
    public GameObject fishingVesselPrefab;
    public GameObject militaryVesselPrefab;
    public GameObject sailingVesselPrefab;
    public GameObject speedBoatPrefab;
    public GameObject tugboatPrefab;
    public GameObject psvRedPrefab;
    public GameObject psvGreenPrefab;
    public GameObject sarRhibPrefab;
    public GameObject ferryVesselPrefab;
    public GameObject lngPrefab; 
    public GameObject cargoPrefab; 
    public GameObject cargo2Prefab; 
    public GameObject cargo3Prefab; 
    public GameObject cargoWithContainersPrefab; 
    public GameObject tankerPrefab; 
    public GameObject tanker2Prefab;
    public GameObject oilRigPrefab;
    public GameObject submarinePrefab;
    
    // OTHER //
    // public GameObject waveTrailerPrefab;
    public Transform vesselContainer;
    public static Dictionary<string, GeoObjectInstance> mmsiToGeoObjectInstance; 
    private BarentsWatchService bwService;

    void Awake() {
        mmsiToGeoObjectInstance = new Dictionary<string, GeoObjectInstance>();
        bwService = GetComponent<BarentsWatchService>();
        MapController.MapLocationChanged += OnMapLocationChanged;
    }

    public void InstantiateVessel(GeoPosition gpos, Vector3 position, Quaternion rotation, bool fromGeoPositionAPI) {
        if (fromGeoPositionAPI && gpos.TrackingId != null && mmsiToGeoObjectInstance.ContainsKey(gpos.TrackingId.Trim())) 
            return; // Vessel has already been instantiated from the Barentswatch stream

        GameObject geoGameObject = Instantiate(FindPrefabForGeoPosition(gpos), position, rotation, vesselContainer);
        GeoObjectInstance goi = geoGameObject.AddComponent<GeoObjectInstance>();
        geoGameObject.tag = GeoConsts.GEO_OBJECT_TAG;
        goi.info = new GeoObjectInfo(gpos);
        // goi.displayName = geoGameObject.name = gpos.DisplayName != null ? gpos.DisplayName.Trim() : gpos.Id.ToString();
        // goi.trackingID = gpos.TrackingId != null ? gpos.TrackingId.Trim() : gpos.Id.ToString();
        geoGameObject.name = goi.info.DisplayName;
        goi.info.IsFromGeoPositionAPI = fromGeoPositionAPI;
        if (IsValidMMSI(goi.info.TrackingId) && !mmsiToGeoObjectInstance.ContainsKey(goi.info.TrackingId)) { 
            mmsiToGeoObjectInstance.Add(goi.info.TrackingId, goi); // Require the TrackingID to be a valid MMSI to be counted as a vessel
        }

        // Instantiate(waveTrailerPrefab, geoGameObject.transform);

        GeoPositionManager.geoIdToGameObject.Add(gpos.Id.ToString(), geoGameObject);
        GeoPositionManager.geoObjectNameTrie.Insert(goi.info.DisplayName, goi);
        GeoPositionManager.trackingIdTrie.Insert(goi.info.TrackingId, goi);
        BrowseGeoObjectsUIController.AddEntry(gpos.CategoryCode, goi);
    }

    private bool IsValidMMSI(string trackingId) {
        return trackingId.Length == APIConsts.MMSI_REQUIRED_DIGIT_COUNT && int.TryParse(trackingId, out _);
    }

    private GameObject FindPrefabForGeoPosition(GeoPosition gpos) {
        return gpos.GeoModelCode switch {
            VesselGeoCode.CRUISE => cruiseShipPrefab,
            VesselGeoCode.FISHING_BOAT => fishingVesselPrefab,
            VesselGeoCode.TOWING_VESSEL1 => tugboatPrefab,
            VesselGeoCode.TOWING_VESSEL2 => tugboatPrefab,
            VesselGeoCode.DREDGE_UNDERWATER_OPTS => psvRedPrefab,
            VesselGeoCode.DIVING_OPTS_VESSEL => fishingVesselPrefab,
            VesselGeoCode.MILITARY_VESSEL => militaryVesselPrefab,
            VesselGeoCode.SAIL_BOAT => sailingVesselPrefab,
            VesselGeoCode.PLEASURE_CRAFT => speedBoatPrefab,
            VesselGeoCode.HIGH_SPEED_CRAFT1 => speedBoatPrefab,
            VesselGeoCode.HIGH_SPEED_CRAFT2 => speedBoatPrefab,
            VesselGeoCode.HIGH_SPEED_CRAFT3 => speedBoatPrefab,
            VesselGeoCode.HIGH_SPEED_CRAFT4 => speedBoatPrefab,
            VesselGeoCode.HIGH_SPEED_CRAFT5 => speedBoatPrefab,
            VesselGeoCode.HIGH_SPEED_CRAFT6 => speedBoatPrefab,
            VesselGeoCode.HIGH_SPEED_CRAFT7 => speedBoatPrefab,
            VesselGeoCode.HIGH_SPEED_CRAFT8 => speedBoatPrefab,
            VesselGeoCode.HIGH_SPEED_CRAFT9 => speedBoatPrefab,
            VesselGeoCode.HIGH_SPEED_CRAFT10 => speedBoatPrefab,
            VesselGeoCode.PSV_RED => psvRedPrefab,
            VesselGeoCode.PSV_GREEN => psvGreenPrefab,
            VesselGeoCode.SEARCH_AND_RESCUE_RHIB => sarRhibPrefab,
            VesselGeoCode.TUGBOAT => tugboatPrefab,
            VesselGeoCode.PORT_TENDER_VESSEL => tugboatPrefab,
            VesselGeoCode.PASSENGER_VESSEL1 => ferryVesselPrefab,
            VesselGeoCode.PASSENGER_VESSEL2 => ferryVesselPrefab,
            VesselGeoCode.PASSENGER_VESSEL3 => ferryVesselPrefab,
            VesselGeoCode.PASSENGER_VESSEL4 => ferryVesselPrefab,
            VesselGeoCode.PASSENGER_VESSEL5 => ferryVesselPrefab,
            VesselGeoCode.PASSENGER_VESSEL6 => ferryVesselPrefab,
            VesselGeoCode.PASSENGER_VESSEL7 => ferryVesselPrefab,
            VesselGeoCode.PASSENGER_VESSEL8 => ferryVesselPrefab,
            VesselGeoCode.PASSENGER_VESSEL9 => ferryVesselPrefab,
            VesselGeoCode.PASSENGER_VESSEL10 => ferryVesselPrefab,
            VesselGeoCode.CARGO_VESSEL1 => cargoPrefab,
            VesselGeoCode.CARGO_VESSEL2 => cargo2Prefab,
            VesselGeoCode.CARGO_VESSEL3 => cargo3Prefab,
            VesselGeoCode.CARGO_VESSEL4 => cargoWithContainersPrefab,
            VesselGeoCode.CARGO_VESSEL5 => cargoWithContainersPrefab,
            VesselGeoCode.CARGO_VESSEL6 => cargoWithContainersPrefab,
            VesselGeoCode.CARGO_VESSEL7 => cargoWithContainersPrefab,
            VesselGeoCode.CARGO_VESSEL8 => cargoWithContainersPrefab,
            VesselGeoCode.CARGO_VESSEL9 => cargoWithContainersPrefab,
            VesselGeoCode.CARGO_VESSEL10 => cargoWithContainersPrefab,
            VesselGeoCode.TANKER_VESSEL1 => lngPrefab,
            VesselGeoCode.TANKER_VESSEL2 => tankerPrefab,
            VesselGeoCode.TANKER_VESSEL3 => tanker2Prefab,
            VesselGeoCode.TANKER_VESSEL4 => lngPrefab,
            VesselGeoCode.TANKER_VESSEL5 => lngPrefab,
            VesselGeoCode.TANKER_VESSEL6 => lngPrefab,
            VesselGeoCode.TANKER_VESSEL7 => lngPrefab,
            VesselGeoCode.TANKER_VESSEL8 => lngPrefab,
            VesselGeoCode.TANKER_VESSEL9 => lngPrefab,
            VesselGeoCode.TANKER_VESSEL10 => lngPrefab,
            VesselGeoCode.SUBMARINE => submarinePrefab,
            VesselGeoCode.OILRIG => oilRigPrefab,
            _ => RefManager.Get<GeoPositionManager>().missingObjectPrefab,
        };
    }

    private void OnMapLocationChanged() {
        bwService.UpdateStreamParameters();
        mmsiToGeoObjectInstance.Clear();
    }

    public void SetAISOffsetsAndScale(GeoObjectInstance goi, dynamic vesselData) {
        AISTrackerOffsets trackerOffsets = new() {
            FrontDistance = (float?)vesselData.dimensionA, // MetersToBow
            BackDistance = (float?)vesselData.dimensionB, // MetersToStern
            LeftDistance = (float?)vesselData.dimensionC, // MetersToPortSide
            RightDistance = (float?)vesselData.dimensionD // MetersToStarboardSide
        };
        
        goi.trackerOffsets = trackerOffsets; 

        // Improve vessel position precision by accounting for AISTracker position on vessel
        goi.transform.Translate(GetAISOffsetVector(goi.trackerOffsets));

        // Scale vessel to be accurate to its real life size
        if (!goi.info.IsScaledToRealWorld)
            ScaleGeoObjectToCorrectSize(goi);
    }

    public static Vector3 GetAISOffsetVector(AISTrackerOffsets offsets) {
        if (offsets == null || offsets.FrontDistance == null || offsets.BackDistance == null || offsets.LeftDistance == null || offsets.RightDistance == null)
            return Vector3.zero;

        float? totalLength = offsets.FrontDistance + offsets.BackDistance;
        float? totalWidth = offsets.LeftDistance + offsets.RightDistance;
        float? initialFrontOffset = totalLength / 2f;
        float? initialRightOffset = totalWidth / 2f;
        float? frontDistanceDiff = offsets.FrontDistance - initialFrontOffset;
        float? rightDistanceDiff = offsets.RightDistance - initialRightOffset;
        
        return new Vector3((float)rightDistanceDiff, 0f, (float)frontDistanceDiff);
    }

    private void ScaleGeoObjectToCorrectSize(GeoObjectInstance goi) {
        AISTrackerOffsets offsets = goi.trackerOffsets;
        if (offsets == null || offsets.FrontDistance == null || offsets.BackDistance == null || offsets.LeftDistance == null || offsets.RightDistance == null)
            return;

        float realLength = (float)offsets.FrontDistance + (float)offsets.BackDistance;
        float realWidth = (float)offsets.LeftDistance + (float)offsets.RightDistance;

        // The scale of the bounds object represent the GeoObjects width, height and length respectively.
        BoxCollider collider = goi.GetComponent<BoxCollider>();
        if (collider == null) {
            Debug.Log("The GeoObject doesn't have any BoxCollider attached and thus can't be scaled to real world size.");
            return;
        }

        foreach (Transform child in goi.transform) {
            if (child.name.Equals("BOUNDS")) {
                // The scale of the bounds object represent the GeoObjects width, height and length respectively.
                float modelLength = realLength/child.localScale.z;
                float modelWidth = realWidth/child.localScale.x;
                float modelHeight = realLength < realWidth ? realLength/child.localScale.y : realWidth/child.localScale.y;
                goi.transform.localScale = new Vector3(modelWidth, modelHeight, modelLength);
                goi.info.IsScaledToRealWorld = true;
                goi.info.Dimensions = "Length: " + realLength + "m, width: " + realWidth + "m";
                break;
            }
        } 
    }

    void OnDestroy() {
        MapController.MapLocationChanged -= OnMapLocationChanged;
    }

    // private void OnGeoPositionsLoaded() {
    //     StartCoroutine(FetchVesselsInformation());
    // }

    // public IEnumerator FetchVesselsInformation() {
    //     List<VesselInfo> vesselInfos = new();
    //     StringBuilder requestBody = new("{\"items\":[");
    //     foreach (var sourceEntityId in GeoPositionManager.sourceEntityIdToTrackingId.Keys) {
    //         requestBody.Append("\"").Append(sourceEntityId).Append("\",");
    //     }
    //     requestBody.Length--; // Remove last comma
    //     requestBody.Append("]}");

    //     using (UnityWebRequest request = UnityWebRequest.Post(APIConsts.URL_VESSELS, requestBody.ToString(), "application/json")) {
    //         request.SetRequestHeader("Api-key", GeoPositionManager.currentAPIKey);
    //         yield return request.SendWebRequest();
    //         if (request.result == UnityWebRequest.Result.ConnectionError)
    //             Debug.Log(request.error);
    //         vesselInfos = JsonConvert.DeserializeObject<VesselInfoWrapper>(request.downloadHandler.text)?.Vessels;    
    //     }   
        
    //     if (vesselInfos == null) {
    //         VesselInformationReceived.Invoke();
    //         yield break;
    //     }
        
    //     foreach (var vessel in vesselInfos) {
    //         var trackingId = GeoPositionManager.sourceEntityIdToTrackingId[vessel.Id];

    //         if (!GeoPositionManager.mmsiToTrackingId.ContainsKey(vessel.MmsiNumber))
    //             GeoPositionManager.mmsiToTrackingId.Add(vessel.MmsiNumber, trackingId);

    //         var goi = GeoPositionManager.trackingIdToGeoObjectInstance[trackingId];
    //         AISTrackerOffsets trackerOffsets = new()
    //         {
    //             FrontDistance = vessel.MetersToBow,
    //             BackDistance = vessel.MetersToStern,
    //             LeftDistance = vessel.MetersToPortSide,
    //             RightDistance = vessel.MetersToStarboardSide
    //         };
    //         goi.trackerOffsets = trackerOffsets; 
    //         goi.isVessel = true;

    //         // Improve vessel position precision by accounting for AISTracker position on vessel
    //         goi.transform.Translate(GetAISOffsetVector(goi.trackerOffsets));

    //         // Scale vessel to be accurate to its real life size
    //         if (!goi.isScaledToRealWorld)
    //             ScaleGeoObjectToCorrectSize(goi);
    //     }
    //     VesselInformationReceived.Invoke();
    // }
}
