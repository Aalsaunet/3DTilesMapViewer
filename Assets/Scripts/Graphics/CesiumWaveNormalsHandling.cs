using UnityEngine;
using UnityEngine.UI;
using CesiumForUnity;
using System;

public class CesiumWaveNormalsHandling : CesiumTextureHandling
{ 
    public Toggle waveEffectToggle;
    public bool waveEffectEnabled;
    public Toggle landEffectToggle;
    public bool landEffectEnabled;

    private readonly float colorRangeLightStyle = 0.2f;
    private readonly float colorRangeGrayStyle = 0.1f;
    private readonly float colorRangeDarkStyle = 0.001f;
    
    // Ocean light
    private readonly string fromOceanColorLightStyle = "#ACC8F2"; 
    private readonly string toOceanColorLightStyle = "#284E6E";
    
    // Ocean grey
    private readonly string fromOceanColorGrayStyle = "#C8C8C8"; 
    private readonly string toOceanColorGrayStyle = "#21405A";
    
    // Ocean dark
    private readonly string fromOceanColorDarkStyle = "#333333";
    private readonly string toOceanColorDarkStyle = "#102230";//"#2A587D";
    
    // Land
    private readonly string fromLandColorLightStyle = "#DAD1C3"; //"#B1B1B5"; 
    private readonly string fromLandColorGrayStyle = "#EAEAEA"; //"#EAEAEA"; 
    private readonly string fromLandColorDarkStyle = "#000000";

    private void SetEffectToMaterial(string shaderVariable, bool enable) {
        // Update material and loop through the existing tiles' mesh instances and update them
        tileset.opaqueMaterial.SetInt(shaderVariable, enable ? 1 : 0);
        foreach (var mesh in tileset.GetComponentsInChildren<MeshRenderer>(true)) {
            mesh.material.SetInt(shaderVariable, enable ? 1 : 0);
        }
    }

    public void OnToggleWaveEffect(Toggle toggle) { 
        waveEffectEnabled = toggle.isOn;
        SetEffectToMaterial("_enableWaveEffect", toggle.isOn);
    }

    public void SetAllowWaveEffect(bool isAllowed) { 
        waveEffectToggle.interactable = isAllowed;
        // SetWaveEffectToMaterial(isAllowed);
    }

    public void OnToggleLandEffect(Toggle toggle) { 
        landEffectEnabled = toggle.isOn;
        SetEffectToMaterial("_enableLandEffect", toggle.isOn);
    }

    public void SetOceanStyle(BingMapsStyle style) {
        string fromHexColor;
        string fromLandHexColor;
        string toHexColor;
        float colorRange;
        
        switch (style) {
            case BingMapsStyle.CanvasLight:
                fromHexColor = fromOceanColorLightStyle;
                fromLandHexColor = fromLandColorLightStyle;
                toHexColor = toOceanColorLightStyle;
                colorRange = colorRangeLightStyle;
                break;
            case BingMapsStyle.CanvasGray:
                fromHexColor = fromOceanColorGrayStyle;
                fromLandHexColor = fromLandColorGrayStyle;
                toHexColor = toOceanColorGrayStyle;
                colorRange = colorRangeGrayStyle;
                break;
            case BingMapsStyle.CanvasDark:
                fromHexColor = fromOceanColorDarkStyle;
                fromLandHexColor = fromLandColorDarkStyle;
                toHexColor = toOceanColorDarkStyle;
                colorRange = colorRangeDarkStyle;
                break;
            default:
                fromHexColor = "#000000";
                fromLandHexColor = "#000000";
                toHexColor = "#000000";
                colorRange = 0f;
                break;
        }

        float r = ((float)Convert.ToInt32(fromHexColor.Substring(1, 2), 16)) / 255f;
        float g = ((float)Convert.ToInt32(fromHexColor.Substring(3, 2), 16)) / 255f;
        float b = ((float)Convert.ToInt32(fromHexColor.Substring(5, 2), 16)) / 255f;
        Color fromColor = new Color(r, g, b);

        float rl = ((float)Convert.ToInt32(fromLandHexColor.Substring(1, 2), 16)) / 255f;
        float gl = ((float)Convert.ToInt32(fromLandHexColor.Substring(3, 2), 16)) / 255f;
        float bl = ((float)Convert.ToInt32(fromLandHexColor.Substring(5, 2), 16)) / 255f;
        Color fromLandColor = new Color(rl, gl, bl);

        r = ((float)Convert.ToInt32(toHexColor.Substring(1, 2), 16)) / 255f;
        g = ((float)Convert.ToInt32(toHexColor.Substring(3, 2), 16)) / 255f;
        b = ((float)Convert.ToInt32(toHexColor.Substring(5, 2), 16)) / 255f;
        Color toColor = new Color(r, g, b);

        tileset.opaqueMaterial.SetColor("_fromWaterColor", fromColor);
        tileset.opaqueMaterial.SetColor("_fromLandColor", fromLandColor);
        tileset.opaqueMaterial.SetColor("_toWaterColor", toColor);
        tileset.opaqueMaterial.SetFloat("_colorMaskRange", colorRange);
        foreach (var mesh in tileset.GetComponentsInChildren<MeshRenderer>(true)) {
            mesh.material.SetColor("_fromWaterColor", fromColor);
            mesh.material.SetColor("_fromLandColor", fromLandColor);
            mesh.material.SetColor("_toWaterColor", toColor);
            mesh.material.SetFloat("_colorMaskRange", colorRange);
        }
    }
}
