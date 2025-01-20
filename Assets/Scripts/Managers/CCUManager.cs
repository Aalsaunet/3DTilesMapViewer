using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using PimDeWitte.UnityMainThreadDispatcher;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CCUManager : MonoBehaviour
{
    // CCU measurements in meters   
    public const float CCU_LENGTH_20FT = 6.096f; 
    public const float CCU_LENGTH_40FT = 12.192f; 
    public const float CCU_WIDTH = 2.438f; 
    public const float CCU_HEIGHT = 2.591f; 
    public const int DG_ICON_LIMIT = 4; 

    // STACKING CHART LONGSIDE PREFABS (pivot lower left) //    
    public GameObject scLSCCU20FTPrefab;
    public GameObject scLSCCU40FTPrefab;
    public GameObject scLSMissingCCU20FTPrefab;
    public GameObject scLSMissingCCU40FTPrefab;
    public GameObject scLSInvalidCCU20FTPrefab;
    public GameObject scLSInvalidCCU40FTPrefab;

    // STACKING CHART SHORTSIDE PREFABS (pivot lower left) //    
    public GameObject scSSCCU20FTPrefab;
    public GameObject scSSCCU40FTPrefab;
    public GameObject scSSMissingCCU20FTPrefab;
    public GameObject scSSMissingCCU40FTPrefab;
    public GameObject scSSInvalidCCU20FTPrefab;
    public GameObject scSSInvalidCCU40FTPrefab;

    // STACKING CHART TANKS // 
    public GameObject scSSTank20FTPrefab;
    public GameObject scSSTank40FTPrefab;
    public GameObject scLSTank20FTPrefab;
    public GameObject scLSTank40FTPrefab;

    // STANDALONE CCU PREFABS (pivot centre) //
    public GameObject saCCCU20FTPrefab;
    public GameObject saCCCU40FTPrefab;
    public GameObject saCTank20FTPrefab;
    public GameObject saCTank40FTPrefab;
    public GameObject saCTank45FTPrefab;
    public GameObject mudskipPrefab;

    // Industrial Building Materials On Pallets //
    public GameObject palletPrefab1;
    public GameObject palletPrefab2;
    public GameObject palletPrefab3;
    public GameObject palletPrefab4;
    public GameObject palletPrefab5;    
    public GameObject palletPrefab6;
    public GameObject palletPrefab7;
    public GameObject palletPrefab8;
    public GameObject palletPrefab9;
    public GameObject palletPrefab10;
    public GameObject palletPrefab11;
    public GameObject palletPrefab12;
    public GameObject palletPrefab13;
    public GameObject palletPrefab14;
    public GameObject palletPrefab15;    
    public GameObject palletPrefab16;
    public GameObject palletPrefab17;
    public GameObject palletPrefab18;
    public GameObject palletPrefab19;
    public GameObject palletPrefab20;

    // DG MATERIALS //
    public Texture2D customsTex;
    public Texture2D dg1Tex;
    public Texture2D dg2_1Tex;
    public Texture2D dg2_2Tex;
    public Texture2D dg2_3Tex;
    public Texture2D dg3Tex;
    public Texture2D dg4_1Tex;
    public Texture2D dg4_2Tex;
    public Texture2D dg4_3Tex;
    public Texture2D dg5_1Tex;
    public Texture2D dg5_2Tex;
    public Texture2D dg6_1Tex;
    public Texture2D dg6_2Tex;
    public Texture2D dg7Tex;
    public Texture2D dg8Tex;
    public Texture2D dg9Tex;

    public Transform stackingChartContainer;
    public Transform standaloneCCUContainer;

    // OTHER //
    private Dictionary<Guid, GameObject[,]> stackRowIDToCCUObjects;
    private Dictionary<GameObject, int[]> ccuToStackPosition;
    private bool displayStandaloneCCUs;

    enum StackChartSlotType {EMPTY, FILLED, DISABLED, INVALID}
        
    void Start() {
        stackRowIDToCCUObjects = new Dictionary<Guid, GameObject[,]>();
        ccuToStackPosition = new Dictionary<GameObject, int[]>();
        displayStandaloneCCUs = true;
        standaloneCCUContainer.gameObject.SetActive(displayStandaloneCCUs);
        MapController.MapLocationChanged += OnMapLocationChanged;
    }

    private void OnMapLocationChanged() {           
        stackRowIDToCCUObjects.Clear();
        ccuToStackPosition.Clear();
    }

    public IEnumerator FetchAndInstantiateStackingChart(GeoPosition gpos, string apiKey, Vector3 pos, Quaternion rot) {
        if (gpos.ObjectReferenceNo.IsNullOrEmpty() || gpos.SourceEntityId == null)
            yield break;

        DetermineCCUTypeAndOrientation(gpos, out GameObject ccuPrefab, out GameObject ccuMissingPrefab, 
                                        out GameObject ccuInvalidPrefab, out float ccuOffset, out bool isShortSidePivoted);

        // Fetch Stacking chart data from API and parse it
        dynamic diagramContent;
        using (UnityWebRequest request = UnityWebRequest.Get(APIConsts.URL_STACK_DIAGRAM + gpos.ObjectReferenceNo)) {
            request.SetRequestHeader("Api-key", apiKey);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError)
                Debug.Log(request.error);
            var response = JsonConvert.DeserializeObject<dynamic>(request.downloadHandler.text);
            // Debug.Log(request.downloadHandler.text);
            if (response == null) yield break;
            string data = response.data;
            if (data == null) yield break;
            diagramContent = JsonConvert.DeserializeObject<dynamic>(data);
        }

        if (!isShortSidePivoted)
            rot.eulerAngles = new Vector3(0f, rot.eulerAngles.y - 90f, 0f);

        // Set up the stacking chart object that will host the CCUs
        string stackingChartName = diagramContent.SubLocationName + $" - {gpos.SourceEntityId}";
        GameObject stackingChartObject = new GameObject(stackingChartName);
        stackingChartObject.transform.position = pos;
        stackingChartObject.transform.rotation = rot;
        stackingChartObject.transform.parent = stackingChartContainer;
        var goiOuter = stackingChartObject.AddComponent<GeoObjectInstance>();
        goiOuter.info = new GeoObjectInfo(gpos);
        goiOuter.isStackingChart = true;
        GeoPositionManager.geoIdToGameObject.Add(gpos.Id.ToString(), stackingChartObject);

        dynamic stackRowContent;
        string url = APIConsts.URL_STACK_ROW_CONTENT + $"{gpos.ObjectReferenceNo}/{gpos.SourceEntityId}";
        using (UnityWebRequest request = UnityWebRequest.Get(url)) {
            request.SetRequestHeader("Api-key", apiKey);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError)
                Debug.Log(request.error);
            stackRowContent = JsonConvert.DeserializeObject<dynamic>(request.downloadHandler.text);
            // Debug.Log(request.downloadHandler.text);
        }

        int horizontalCells = diagramContent.Columns, verticalCells = diagramContent.Height;
        StackChartSlotType[,] isCCUInCell = new StackChartSlotType[horizontalCells, verticalCells];
        stackRowIDToCCUObjects.Add((Guid)gpos.SourceEntityId, new GameObject[horizontalCells, verticalCells]);

        // Start constructing the stacking charts with individual CCUs
        // The size of the stacking chart is determined by rows (depth), colums (width) and number of containers stacked on top of each ohter (height)
        foreach (var entity in stackRowContent.warehouseEntries) {
            string ccuId = (string)(entity?.id);
            string ccuTrackingId = ((string)(entity?.serialNo)).Trim();
            if (ccuTrackingId == null)
                continue;

            if (GeoPositionManager.trackingIdToGeoObjectInstance.TryGetValue(ccuTrackingId, out GeoObjectInstance goiCheck)) {
                if ((goiCheck.info as CcuInfo).IsStandaloneCCU)
                    GeoPositionManager.RemoveGeoObject(GeoPositionManager.trackingIdToGeoId[ccuTrackingId]);
                else
                    continue;
            }

            string stackLocation = entity.stackLocation.locationName;
            if (stackLocation == null || stackLocation == "")
                continue;

            int x, y, iconIndex = 0;
            try {
                (x, y) = StringCoordinateSplitter(stackLocation);
                isCCUInCell[x, y] = StackChartSlotType.FILLED;
            } catch {
                Debug.Log("Unable to parse CCU stackLocation. StackLocation: " + stackLocation);
                continue;
            }

            // Vector3 spawnPosition = new Vector3(pos.x, pos.y + (y * CCU_HEIGHT), pos.z);
            // GameObject ccuObject = Instantiate(ccuPrefab, Vector3.zero, rot, stackingChartObject.transform);
            GameObject ccuObject = Instantiate(ccuPrefab, stackingChartObject.transform);
            ccuObject.transform.Translate(new Vector3(x * ccuOffset, y * CCU_HEIGHT, 0f));

            var goi = ccuObject.AddComponent<GeoObjectInstance>();
            goi.info = new CcuInfo(entity);
            ccuObject.name = goi.info.DisplayName;
            // goi.info.IsFromGeoPositionAPI = true;

            GeoPositionManager.AddToGeoPositionDataStructures(ccuId, gpos, ccuObject, goi);
            stackRowIDToCCUObjects[(Guid)gpos.SourceEntityId][x, y] = ccuObject;
            ccuToStackPosition.Add(ccuObject, new int[2] { x, y });

            SetCCUIDBB(ccuObject, (string)entity.serialNo);

            bool isCustomsRequired = entity.isCustomsIdentificationNumberRequired == null || (bool)entity.isCustomsIdentificationNumberRequired;
            isCustomsRequired = isCustomsRequired && (entity.customsIdentificationNumber == null || (string)entity.customsIdentificationNumber == "");

            if (isCustomsRequired)
                SetCCUBBIcons(ccuObject, iconIndex++, customsTex);

            if (entity.dgEntries != null) {
                foreach (var dgEntry in entity.dgEntries) {
                    if (iconIndex > DG_ICON_LIMIT)
                        break;
                    string dgClass = dgEntry.dgClassNo;
                    switch (dgClass) {
                        case "1": SetCCUBBIcons(ccuObject, iconIndex++, dg1Tex); break;
                        case "2.1": SetCCUBBIcons(ccuObject, iconIndex++, dg2_1Tex); break;
                        case "2.2": SetCCUBBIcons(ccuObject, iconIndex++, dg2_2Tex); break;
                        case "2.3": SetCCUBBIcons(ccuObject, iconIndex++, dg2_3Tex); break;
                        case "3": SetCCUBBIcons(ccuObject, iconIndex++, dg3Tex); break;
                        case "4.1": SetCCUBBIcons(ccuObject, iconIndex++, dg4_2Tex); break;
                        case "4.2": SetCCUBBIcons(ccuObject, iconIndex++, dg4_2Tex); break;
                        case "4.3": SetCCUBBIcons(ccuObject, iconIndex++, dg4_3Tex); break;
                        case "5.1": SetCCUBBIcons(ccuObject, iconIndex++, dg5_1Tex); break;
                        case "5.2": SetCCUBBIcons(ccuObject, iconIndex++, dg5_2Tex); break;
                        case "6.1": SetCCUBBIcons(ccuObject, iconIndex++, dg6_1Tex); break;
                        case "6.2": SetCCUBBIcons(ccuObject, iconIndex++, dg6_2Tex); break;
                        case "7": SetCCUBBIcons(ccuObject, iconIndex++, dg7Tex); break;
                        case "8": SetCCUBBIcons(ccuObject, iconIndex++, dg8Tex); break;
                        case "9": SetCCUBBIcons(ccuObject, iconIndex++, dg9Tex); break;
                        default: Debug.Log("Unknown DG Class encountered: " + dgEntry.dgClassNo); break;
                    }
                }
            }
        }

        // Excluse the disabled cells
        foreach (var row in diagramContent.Rows) {
            if (row.Id != gpos.SourceEntityId || row.Cells == null)
                continue;
            foreach (var cell in row.Cells) {
                bool isEnabled = cell.IsEnabled;
                if (!isEnabled) {
                    for (int i = cell.Y; i < verticalCells; i++)
                        isCCUInCell[cell.X, i] = StackChartSlotType.DISABLED;
                }
            }
        }

        //Material mat = new Material(Shader.Find("Standard"));
        string color = diagramContent.CcuColor;
        float r = ((float)Convert.ToInt32(color.Substring(1, 2), 16)) / 255f;
        float g = ((float)Convert.ToInt32(color.Substring(3, 2), 16)) / 255f;
        float b = ((float)Convert.ToInt32(color.Substring(5, 2), 16)) / 255f;

        // Fill in the empty slots with container outlines to mark available spaces
        for (int i = 0; i < horizontalCells; i++) {
            for (int j = 0; j < verticalCells; j++) {
                if (isCCUInCell[i, j] == StackChartSlotType.EMPTY) {

                    // Check if a CCU is placed above an empty slot (invalid) and use a different missing prefab to indicate this
                    bool invalidPlacement = false;
                    for (int k = j + 1; k < verticalCells; k++)
                    {
                        if (isCCUInCell[i, k] == StackChartSlotType.FILLED)
                        {
                            invalidPlacement = true;
                            isCCUInCell[i, j] = StackChartSlotType.INVALID;
                            break;
                        }
                    }

                    // Vector3 spawnPosition = new Vector3(pos.x, pos.y + (j * CCU_HEIGHT), pos.z);
                    // GameObject prefab = invalidPlacement ? ccuInvalidPrefab : ccuMissingPrefab;
                    // GameObject ccuObject = Instantiate(prefab, spawnPosition, rot, stackingChartObject.transform);
                    // ccuObject.transform.Translate(new Vector3(i * ccuOffset, 0f, 0f));
                    
                    GameObject prefab = invalidPlacement ? ccuInvalidPrefab : ccuMissingPrefab;
                    GameObject ccuObject = Instantiate(prefab, stackingChartObject.transform);
                    ccuObject.transform.Translate(new Vector3(i * ccuOffset, j * CCU_HEIGHT, 0f));

                    stackRowIDToCCUObjects[(Guid)gpos.SourceEntityId][i, j] = ccuObject;
                    ccuToStackPosition.Add(ccuObject, new int[2] {i, j});
                    ccuObject.name = $"Placeholder_CCU_slot_{i}_{j}";
                    if (!invalidPlacement)
                        ccuObject.GetComponentInChildren<Renderer>().material.color = new Color(r, g, b, 0.3f);
                }
            }
        }
    }

    private void DetermineCCUTypeAndOrientation(GeoPosition gpos, out GameObject ccuPrefab, out GameObject ccuMissingPrefab, 
                                                out GameObject ccuInvalidPrefab, out float ccuOffset, out bool isShortSidePivoted) {
            // Determine whether the stacking chart is organised from the shortside or longside
            // and if the stacking chart should be constructed of 20FT or 40FT (default) CCUs
            switch (gpos.GeoModelCode) {
            case StackingChartGeoCode.LONGSIDE_20FT: 
                ccuPrefab = scLSCCU20FTPrefab;
                ccuMissingPrefab = scLSMissingCCU20FTPrefab;
                ccuInvalidPrefab = scLSInvalidCCU20FTPrefab;
                ccuOffset = CCU_LENGTH_20FT;
                isShortSidePivoted = false;
                break;
            case StackingChartGeoCode.LONGSIDE_40FT: 
                ccuPrefab = scLSCCU40FTPrefab;
                ccuMissingPrefab = scLSMissingCCU40FTPrefab;
                ccuInvalidPrefab = scLSInvalidCCU40FTPrefab;
                ccuOffset = CCU_LENGTH_40FT;
                isShortSidePivoted = false;
                break;
            case StackingChartGeoCode.LONGSIDE_TANKS_20FT: 
                ccuPrefab = scLSTank20FTPrefab;
                ccuMissingPrefab = scLSMissingCCU20FTPrefab;
                ccuInvalidPrefab = scLSInvalidCCU20FTPrefab;
                ccuOffset = CCU_LENGTH_20FT;
                isShortSidePivoted = false;
                break;
            case StackingChartGeoCode.LONGSIDE_TANKS_40FT: 
                ccuPrefab = scLSTank40FTPrefab;
                ccuMissingPrefab = scLSMissingCCU40FTPrefab;
                ccuInvalidPrefab = scLSInvalidCCU40FTPrefab;
                ccuOffset = CCU_LENGTH_40FT;
                isShortSidePivoted = false;
                break;
            case StackingChartGeoCode.SHORTSIDE_20FT: 
                ccuPrefab = scSSCCU20FTPrefab;
                ccuMissingPrefab = scSSMissingCCU20FTPrefab;
                ccuInvalidPrefab = scSSInvalidCCU20FTPrefab;
                ccuOffset = CCU_WIDTH;
                isShortSidePivoted = true;
                break;
            case StackingChartGeoCode.SHORTSIDE_40FT: 
                ccuPrefab = scSSCCU40FTPrefab;
                ccuMissingPrefab = scSSMissingCCU40FTPrefab;
                ccuInvalidPrefab = scSSInvalidCCU40FTPrefab;
                ccuOffset = CCU_WIDTH;
                isShortSidePivoted = true;
                break;
            case StackingChartGeoCode.SHORTSIDE_TANKS_20FT: 
                ccuPrefab = scSSTank20FTPrefab;
                ccuMissingPrefab = scSSMissingCCU20FTPrefab;
                ccuInvalidPrefab = scSSInvalidCCU20FTPrefab;
                ccuOffset = CCU_WIDTH;
                isShortSidePivoted = true;
                break;
            case StackingChartGeoCode.SHORTSIDE_TANKS_40FT: 
                ccuPrefab = scSSTank40FTPrefab;
                ccuMissingPrefab = scSSMissingCCU40FTPrefab;
                ccuInvalidPrefab = scSSInvalidCCU40FTPrefab;
                ccuOffset = CCU_WIDTH;
                isShortSidePivoted = true;
                break;
            default: 
                Debug.Log("Unknown CCU type encountered: " + gpos.GeoModelCode + ". Defaulting to 20FT shortside pivot"); 
                ccuPrefab = scSSCCU20FTPrefab;
                ccuMissingPrefab = scSSMissingCCU20FTPrefab;
                ccuInvalidPrefab = scSSInvalidCCU20FTPrefab;
                ccuOffset = CCU_WIDTH;
                isShortSidePivoted = true;
                break;
        }
    }

    private (int, int) StringCoordinateSplitter(string strCoords) {
        // Splits a stacking chart coordinate (e.g "1-2") into X and Y ints by whatever non-digit separator
        int i = 0, spanLength = 1;
        while(i + spanLength < strCoords.Length && Char.IsDigit(strCoords[i + spanLength]))
            spanLength++;
        int x = int.Parse(strCoords.Substring(i, spanLength)) - 1;    
        
        i = spanLength + 1;
        spanLength = 1;
        while(i + spanLength < strCoords.Length && Char.IsDigit(strCoords[i + spanLength]))
            spanLength++;
        int y = int.Parse(strCoords.Substring(i, spanLength)) - 1;
        return (x, y);
    }

    public void InstantiateStandaloneCCU(GeoPosition gpos, Vector3 position, Quaternion rotation) {
        if (gpos.TrackingId == null || GeoPositionManager.trackingIdToGeoObjectInstance.ContainsKey(gpos.TrackingId.Trim())) {
            return;
        }

        GameObject ccuToInstantiate;
        bool matchingCCUPrefabFound;
        switch (gpos.GeoModelCode) {
            case WarehouseEntryGeoCode.TANK_20FT: ccuToInstantiate = saCTank20FTPrefab; matchingCCUPrefabFound = true; break;
            case WarehouseEntryGeoCode.CCU_20FT: ccuToInstantiate = saCTank20FTPrefab; matchingCCUPrefabFound = true; break;
            case WarehouseEntryGeoCode.TANK_40FT: ccuToInstantiate = saCTank20FTPrefab; matchingCCUPrefabFound = true; break;
            case WarehouseEntryGeoCode.CCU_40FT: ccuToInstantiate = saCTank20FTPrefab; matchingCCUPrefabFound = true; break;
            case WarehouseEntryGeoCode.CCU_45FT: ccuToInstantiate = saCTank20FTPrefab; matchingCCUPrefabFound = true; break;
            case WarehouseEntryGeoCode.MUDSKIP: ccuToInstantiate = mudskipPrefab; matchingCCUPrefabFound = true; break;
            case WarehouseEntryGeoCode.PALLET_1: ccuToInstantiate = palletPrefab1; matchingCCUPrefabFound = false; break;
            case WarehouseEntryGeoCode.PALLET_2: ccuToInstantiate = palletPrefab2; matchingCCUPrefabFound = false; break;
            case WarehouseEntryGeoCode.PALLET_3: ccuToInstantiate = palletPrefab3; matchingCCUPrefabFound = false; break;
            case WarehouseEntryGeoCode.PALLET_4: ccuToInstantiate = palletPrefab4; matchingCCUPrefabFound = false; break;
            case WarehouseEntryGeoCode.PALLET_5: ccuToInstantiate = palletPrefab5; matchingCCUPrefabFound = false; break;
            case WarehouseEntryGeoCode.PALLET_6: ccuToInstantiate = palletPrefab6; matchingCCUPrefabFound = false; break;
            case WarehouseEntryGeoCode.PALLET_7: ccuToInstantiate = palletPrefab7; matchingCCUPrefabFound = false; break;
            case WarehouseEntryGeoCode.PALLET_8: ccuToInstantiate = palletPrefab8; matchingCCUPrefabFound = false; break;
            case WarehouseEntryGeoCode.PALLET_9: ccuToInstantiate = palletPrefab9; matchingCCUPrefabFound = false; break;
            case WarehouseEntryGeoCode.PALLET_10: ccuToInstantiate = palletPrefab10; matchingCCUPrefabFound = false; break;
            case WarehouseEntryGeoCode.PALLET_11: ccuToInstantiate = palletPrefab11; matchingCCUPrefabFound = false; break;
            case WarehouseEntryGeoCode.PALLET_12: ccuToInstantiate = palletPrefab12; matchingCCUPrefabFound = false; break;
            case WarehouseEntryGeoCode.PALLET_13: ccuToInstantiate = palletPrefab13; matchingCCUPrefabFound = false; break;
            case WarehouseEntryGeoCode.PALLET_14: ccuToInstantiate = palletPrefab14; matchingCCUPrefabFound = false; break;
            case WarehouseEntryGeoCode.PALLET_15: ccuToInstantiate = palletPrefab15; matchingCCUPrefabFound = false; break;
            case WarehouseEntryGeoCode.PALLET_16: ccuToInstantiate = palletPrefab16; matchingCCUPrefabFound = false; break;
            case WarehouseEntryGeoCode.PALLET_17: ccuToInstantiate = palletPrefab17; matchingCCUPrefabFound = false; break;
            case WarehouseEntryGeoCode.PALLET_18: ccuToInstantiate = palletPrefab18; matchingCCUPrefabFound = false; break;
            case WarehouseEntryGeoCode.PALLET_19: ccuToInstantiate = palletPrefab19; matchingCCUPrefabFound = false; break;
            case WarehouseEntryGeoCode.PALLET_20: ccuToInstantiate = palletPrefab20; matchingCCUPrefabFound = false; break;
            default: ccuToInstantiate = RefManager.Get<GeoPositionManager>().missingObjectPrefab; matchingCCUPrefabFound = false; break;   
        }

        GameObject geoGameObject = Instantiate(ccuToInstantiate, position, rotation, standaloneCCUContainer);
        GeoObjectInstance goi = geoGameObject.AddComponent<GeoObjectInstance>();
        goi.info = new CcuInfo(gpos);
        geoGameObject.name = goi.info.DisplayName;
        goi.info.IsFromGeoPositionAPI = true;
        GeoPositionManager.AddToGeoPositionDataStructures(gpos.Id.ToString(), gpos, geoGameObject, goi);
        if (matchingCCUPrefabFound)
            SetCCUIDBB(geoGameObject, goi.info.TrackingId);          
    }

    public void OnToggleDisplayStandAloneCCUs(Toggle toggle) { 
        displayStandaloneCCUs = toggle.isOn;
        standaloneCCUContainer.gameObject.SetActive(toggle.isOn);
    }

    private void SetCCUIDBB(GameObject ccuObject, string idText) {
        ccuObject.transform.Find("BBFront/CCUIDSign/CCUIDValue").GetComponent<TMP_Text>().text = idText;
        ccuObject.transform.Find("BBBack/CCUIDSign/CCUIDValue").GetComponent<TMP_Text>().text = idText;
        ccuObject.transform.Find("BBLeft/CCUIDSign/CCUIDValue").GetComponent<TMP_Text>().text = idText;
        ccuObject.transform.Find("BBRight/CCUIDSign/CCUIDValue").GetComponent<TMP_Text>().text = idText;
        ccuObject.transform.Find("BBTop/CCUIDSign/CCUIDValue").GetComponent<TMP_Text>().text = idText;
    }

    private void SetCCUBBIcons(GameObject ccuObject, int index, Texture2D tex) {
        List<Transform> bblist = new List<Transform> {
            ccuObject.transform.Find($"BBFront/BBIcon{index}"),
            ccuObject.transform.Find($"BBBack/BBIcon{index}"),
            ccuObject.transform.Find($"BBLeft/BBIcon{index}"),
            ccuObject.transform.Find($"BBRight/BBIcon{index}"),
            ccuObject.transform.Find($"BBTop/BBIcon{index}")
        };

        foreach (var bb in bblist) {
            bb.GetComponent<MeshRenderer>().material.mainTexture = tex;
            bb.gameObject.SetActive(true);
        }
    }

    private static bool IsStringInArray(string input, string[] candidates) {
        foreach (string needle in candidates) {
            if (input.Contains(needle))
                return true;
        }
        return false;
    }

    public void MoveStackedCCU(GeoLocation gl) {
        Debug.Log("SIGNALR EVENT RECEIVED for " + gl.PrintProperties());
        if (GeoPositionManager.geoIdToGameObject.ContainsKey(gl.SourceId.ToString())) {
            if (!gl.EventRef.IsNullOrEmpty()) {

                // EventRef should have the format <Diagram id>:<Stack row id>@col:row
                char[] delimiterChars = { ':', '@'};
                string[] parts = gl.EventRef.Split(delimiterChars);
                // return stackRowIDToCCUObjects[Guid.Parse(parts[1])][int.Parse(parts[2]), int.Parse(parts[3])];

                // CCU moved within the stack
                GameObject targetCCU = GeoPositionManager.geoIdToGameObject[gl.SourceId.ToString()];
                GameObject otherCCU = stackRowIDToCCUObjects[Guid.Parse(parts[1])][int.Parse(parts[2]), int.Parse(parts[3])];   

                if (otherCCU == null) {
                    Debug.Log("Couldn't find the other CCU");
                    return;
                }

                int[] targetCCUPos = ccuToStackPosition[targetCCU];
                int[] otherCCUPos = ccuToStackPosition[otherCCU];

                stackRowIDToCCUObjects[Guid.Parse(parts[1])][targetCCUPos[0], targetCCUPos[1]] = otherCCU;
                stackRowIDToCCUObjects[Guid.Parse(parts[1])][otherCCUPos[0], otherCCUPos[1]] = targetCCU;
                ccuToStackPosition[targetCCU] = otherCCUPos;
                ccuToStackPosition[otherCCU] = targetCCUPos;
                UnityMainThreadDispatcher.Instance().Enqueue(() => { 
                    targetCCU.SetActive(true);
                    (targetCCU.transform.position, otherCCU.transform.position) = (otherCCU.transform.position, targetCCU.transform.position);
                });
            } else {
                // CCU moved outside of the stack
                GameObject targetCCU = GeoPositionManager.geoIdToGameObject[gl.SourceId.ToString()];
                UnityMainThreadDispatcher.Instance().Enqueue(() => { 
                    targetCCU.SetActive(false); //TODO add placeholder CCU gameObject
                });
            }
        }
    }

    void OnDestroy() {
        MapController.MapLocationChanged -= OnMapLocationChanged;
    }    
}

// string ccuId = (string)(entity?.id); 
// string ccuTrackingId = ((string)(entity?.serialNo)).Trim(); 
// if (ccuTrackingId == null)
//     continue;

// if (standaloneCCUTrackingIds.Contains(ccuTrackingId)) {
//     standaloneCCUTrackingIds.Remove(ccuTrackingId);
//     GeoPositionManager.RemoveGeoObject(ccuTrackingId);
// } else if (GeoPositionManager.geoIdToGameObject.ContainsKey(ccuTrackingId)) {
//     continue;
// }

// private string PrintProperties(dynamic entity) {
//     StringBuilder sb = new StringBuilder();
//     sb.AppendFormat("{0}: {1}\n", "Serial Number", entity.serialNo);
//     sb.AppendFormat("{0}: {1}\n", "Name", entity.name);
//     sb.AppendFormat("{0}: {1}\n", "Description", entity.description);
//     sb.AppendFormat("{0}: {1}kg\n", "Gross weight", entity.grossWeight);
//     sb.AppendFormat("{0}: {1}\n", "Last modified", entity.dateTimeLastModified);
//     sb.AppendFormat("{0}: {1}\n", "Modified by", entity.lastModifiedByUser);
//     return sb.ToString();
// }

// case CategoryCode.CCU:
//     if (IsStringInArray(gpos.DisplayName, CategoryCode.TANK_20FT_DEF))
//         ccuToInstantiate = saCTank20FTPrefab;
//     else if (IsStringInArray(gpos.DisplayName, CategoryCode.TANK_40FT_DEF))
//         ccuToInstantiate = saCTank40FTPrefab;
//     else if (IsStringInArray(gpos.DisplayName, CategoryCode.CONTAINER_20FT_DEF))
//         ccuToInstantiate = saCCCU20FTPrefab;
//     else
//         ccuToInstantiate = saCCCU40FTPrefab;
//     break;
// default:
//     continue;

    // public void RegisterStandaloneCCU(GeoPosition gpos, Vector3 pos, Quaternion rot) {
    //     if (gpos.TrackingId == null) {
    //         return;
    //     }
    //     if (!CCUsFromGeoPosition.ContainsKey(gpos.TrackingId)) {
    //         // Debug.Log("Adding " + gpos.PrintProperties());
    //         CCUsFromGeoPosition.Add(gpos.TrackingId, (gpos, pos, rot));
    //     } 
    // }

    // private void CheckAndRemoveDuplicateCCU(string trackingId) {
    //     if (CCUsFromGeoPosition.ContainsKey(trackingId))
    //         CCUsFromGeoPosition.Remove(trackingId);
    // }

    // private void InstantiateRemainingCCUs() {
    //     standaloneCCUContainer.gameObject.SetActive(displayStandaloneCCUs);
    //     Debug.Log("InstantiateRemainingCCUs: " + CCUsFromGeoPosition.Values.Count);
    //     foreach (var (gpos, pos, rot) in CCUsFromGeoPosition.Values) {
    //         GameObject ccuToInstantiate;
    //         // Debug.Log(gpos.GeoModelCode);
    //         // Debug.Log(gpos.PrintProperties());
    //         switch (gpos.GeoModelCode) {
    //             case CargoItemTypeGeoCode.TANK_20FT: ccuToInstantiate = saCTank20FTPrefab; break;
    //             case CargoItemTypeGeoCode.CCU_20FT: ccuToInstantiate = saCTank20FTPrefab; break;
    //             case CargoItemTypeGeoCode.TANK_40FT: ccuToInstantiate = saCTank20FTPrefab; break;
    //             case CargoItemTypeGeoCode.CCU_40FT: ccuToInstantiate = saCTank20FTPrefab; break;
    //             case CargoItemTypeGeoCode.CCU_45FT: ccuToInstantiate = saCTank20FTPrefab; break;
    //             default: ccuToInstantiate = RefManager.Get<GeoPositionManager>().missingObjectPrefab; break;   
    //         }

    //         GameObject geoGameObject = Instantiate(ccuToInstantiate, pos, rot, standaloneCCUContainer);
    //         GeoObjectInstance goi = geoGameObject.AddComponent<GeoObjectInstance>();
    //         goi.info = new GeoObjectInfo(gpos);
    //         geoGameObject.name = goi.info.DisplayName;
            
    //         GeoPositionManager.geoIdToGameObject.Add(gpos.Id, geoGameObject);
    //         GeoPositionManager.geoObjectNameTrie.Insert(goi.info.DisplayName, goi);
    //         GeoPositionManager.trackingIdTrie.Insert(goi.info.TrackingId, goi);
    //         BrowseGeoObjectsUIController.AddEntry(gpos.CategoryCode, goi);
    //         SetCCUIDBB(geoGameObject, goi.info.TrackingId);

    //         if (gpos.TrackingId != null)
    //             GeoPositionManager.trackingIdToGeoObjectInstance.Add(gpos.TrackingId, goi);
    //     }

    //     CCUsFromGeoPosition.Clear();
    // }