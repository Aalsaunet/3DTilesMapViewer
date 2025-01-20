using System;
using System.Collections;
using UnityEngine;
using CesiumForUnity;

public enum BuildingTilesetType {
    openStreetMap,
    custom
}

public enum TerrainTilesetType {
    bingMaps,
    GoogleEarth
}

[Serializable]
public class CesiumConstants {
    public static BuildingTilesetType currentBuildingTilesetType;
    public static Cesium3DTileset osmBuildingTileset;
    public static Cesium3DTileset customBuildingTileset;
    public static Cesium3DTileset currentBuildingTileset;
    
    public static TerrainTilesetType currentTerrainTilesetType;
    public static Cesium3DTileset bingMapsTileset;
    public static Cesium3DTileset googleEarthTileset;
    public static Cesium3DTileset currentTerrainTileset;
    
    

    public static void SetBuildingTilesetType(BuildingTilesetType type) {
        currentBuildingTilesetType = type;
        bool useCustom = type == BuildingTilesetType.custom;
        currentBuildingTileset = useCustom ? customBuildingTileset : osmBuildingTileset;

        // customBuildingTileset.gameObject.SetActive(useCustom);
        foreach (var mesh in customBuildingTileset.GetComponentsInChildren<MeshRenderer>(true)) {
            mesh.enabled = useCustom;
        }

        // osmBuildingTileset.gameObject.SetActive(!useCustom);
        foreach (var mesh in osmBuildingTileset.GetComponentsInChildren<MeshRenderer>(true)) {
            mesh.enabled = !useCustom;
        }     
    }

    public static void SetTerrainTilesetType(TerrainTilesetType type) {
        currentTerrainTilesetType = type;
        bool useBingAndOSM = type == TerrainTilesetType.bingMaps;
        currentTerrainTileset = useBingAndOSM ? bingMapsTileset : googleEarthTileset;

        currentBuildingTileset.gameObject.SetActive(useBingAndOSM);
        bingMapsTileset.gameObject.SetActive(useBingAndOSM);
        googleEarthTileset.gameObject.SetActive(!useBingAndOSM);

        RefManager.Get<CesiumWaveNormalsHandling>().SetAllowWaveEffect(useBingAndOSM);
    }
}
