using CesiumForUnity;
using UnityEngine;

public class WeatherController : MonoBehaviour
{
    public Material skyboxDayClear;
    public Material skyboxDayCloudy;
    public Material skyboxEveningClear;
    public Material skyboxEveningCloudy;
    public Material skyboxNightClear;
    public Material skyboxNightCloudy;
    public Light directionalLight;
    public static bool isDark;

    private Vector3 daySunRotation = new Vector3(60f, 80f, 0f);
    private Vector3 dayEveningRotation = new Vector3(5f, 80f, 0f);
    private Vector3 nightSunRotation = new Vector3(90f, 80f, 0f);
    public BingMapsStyle currentMapStyle;
    private bool isRaining;
    private Color dayAmbientLightColor = Color.white;
    private Color eveningAmbientLightColor = new Color(0.71f, 0.44f, 0f);
    private Color nightAmbientLightColor = new Color(0.302f, 0.482f, 0.573f);
   
    public void SetSkyboxFromMapStyle(BingMapsStyle style) {
        currentMapStyle = style;
        switch (style) {
            case BingMapsStyle.CanvasLight:
                if (!isRaining) {
                    RenderSettings.skybox = skyboxDayClear;
                    RenderSettings.skybox.SetFloat("_Rotation", 90f);
                } else {
                    RenderSettings.skybox = skyboxDayCloudy;
                    RenderSettings.skybox.SetFloat("_Rotation", 120f);
                }
                directionalLight.transform.rotation = Quaternion.Euler(daySunRotation);
                directionalLight.color = dayAmbientLightColor;
                RenderSettings.ambientIntensity = 1.0f;
                RenderSettings.reflectionIntensity = 1.0f;
                EnableLightAndLitWindows(false);
                break;
            case BingMapsStyle.CanvasGray:
                if (!isRaining) {
                    RenderSettings.skybox = skyboxEveningClear;
                    RenderSettings.skybox.SetFloat("_Rotation", 113.5f);
                } else {
                    RenderSettings.skybox = skyboxEveningCloudy;
                    RenderSettings.skybox.SetFloat("_Rotation", 20f);
                }
                directionalLight.transform.rotation = Quaternion.Euler(dayEveningRotation);
                directionalLight.color = eveningAmbientLightColor;
                RenderSettings.ambientIntensity = 0.75f;
                RenderSettings.reflectionIntensity = 0.75f;
                EnableLightAndLitWindows(true);
                break;
            case BingMapsStyle.CanvasDark:
                if (!isRaining) {
                    RenderSettings.skybox = skyboxNightClear;
                    RenderSettings.skybox.SetFloat("_Rotation", 90f);
                } else {
                    RenderSettings.skybox = skyboxNightCloudy;
                    RenderSettings.skybox.SetFloat("_Rotation", 90f);
                }
                directionalLight.transform.rotation = Quaternion.Euler(nightSunRotation);
                directionalLight.color = nightAmbientLightColor;
                RenderSettings.ambientIntensity = 0f;
                RenderSettings.reflectionIntensity = 0.5f;
                EnableLightAndLitWindows(true);
                break;
        } 
    }

    public void EnableLightAndLitWindows(bool enable) {
        isDark = enable;
        foreach (var go in GeoPositionManager.geoIdToGameObject.Values) {
            go.GetComponent<GeoObjectInstance>().EnableLightsAndLitWindows(enable);
        }
    }

    public void SetIsRaining(bool isRaining) {
        this.isRaining = isRaining;
        SetSkyboxFromMapStyle(currentMapStyle);
    }
}
