using System.Collections;
using CesiumForUnity;
using UnityEngine;
using UnityEngine.UI;

public class RainController : MonoBehaviour
{
    public WeatherController weatherController;
    public GameObject rainfallObject;
    public Transform rainSplash;
    public Cesium3DTileset bingMapTileset;
    private Vector3 initialPosition;
    private bool rainEffectEnabled;
    
    void Awake() {
        initialPosition = rainSplash.position;
        MapController.MapLocationChanged += OnMapLocationChanged;
    }

    void Start() {
        StartCoroutine(AlignRainSplashToTerrain());
    }

    public void OnToggleRainEffect(Toggle toggle) { 
        rainEffectEnabled = toggle.isOn;
        rainfallObject.SetActive(rainEffectEnabled);
        bingMapTileset.opaqueMaterial.SetInt("_isStormyWeather", rainEffectEnabled ? 1 : 0);
        foreach (var mesh in bingMapTileset.GetComponentsInChildren<MeshRenderer>(true)) {
            mesh.material.SetInt("_isStormyWeather", rainEffectEnabled ? 1 : 0);
        }

        weatherController.SetIsRaining(rainEffectEnabled);
    }

    private IEnumerator AlignRainSplashToTerrain() {
        while (true) {
            yield return new WaitForSeconds(5f);
            bool hitDown = Physics.Raycast(new Vector3(rainSplash.position.x, rainSplash.position.y + 1000f, rainSplash.position.z), Vector3.down, 
                            out RaycastHit downHit, GeoConsts.RAYCAST_LENGTH, LayerMask.GetMask(GeoConsts.RAYCAST_LAYER_TERRAIN));
            if (hitDown)
                rainSplash.position = downHit.point;     
        }
    }

    private void OnMapLocationChanged() {
        rainSplash.position = initialPosition;
    }

    void OnDestroy() {
        MapController.MapLocationChanged -= OnMapLocationChanged;
    }
}
