using UnityEngine;
using CesiumForUnity;
using System.Collections;

public class Cesium3DTileController : MonoBehaviour
{
    public Cesium3DTileset customBuildingTileset;
    public Cesium3DTileset osmBuildingTileset;
    public Transform tileRefreshButton;

    void Start() {
        CesiumConstants.customBuildingTileset.OnTileGameObjectCreated += On3DTileInstantiated;
        CesiumConstants.osmBuildingTileset.OnTileGameObjectCreated += On3DTileInstantiated;
        CesiumConstants.bingMapsTileset.OnTileGameObjectCreated += On3DTileInstantiated;
        CesiumConstants.googleEarthTileset.OnTileGameObjectCreated += On3DTileInstantiated;
    }

    private IEnumerator SpinButton() {   
        float lerpSeconds = 1f;
        float elapsedTime = 0f;
        while (elapsedTime <= lerpSeconds) {
            tileRefreshButton.rotation = Quaternion.Euler(new Vector3(0f, 0f, tileRefreshButton.rotation.eulerAngles.z + 1f));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void On3DTileInstantiated(GameObject tile) {
        StartCoroutine(SpinButton());         
    }

    public void OnRecreateTilesetClicked() {
        CesiumConstants.currentTerrainTileset.RecreateTileset();
        if (CesiumConstants.currentTerrainTilesetType == TerrainTilesetType.bingMaps)
            CesiumConstants.currentBuildingTileset.RecreateTileset();
    }
}
