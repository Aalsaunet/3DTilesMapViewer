using System;
using UnityEngine;
using TMPro;
 
public class FPSUIController : MonoBehaviour
{
    public TextMeshProUGUI fpsText;
    private int _frameCount;
    private int _totalFPS;
    
    void Update() {
        _frameCount++;
        _totalFPS += (int)Math.Round(1f / Time.unscaledDeltaTime);

        if (_frameCount % 60 == 0) {
            fpsText.text = "FPS : " + _totalFPS / _frameCount;
            _totalFPS = 0;
            _frameCount = 0;
        }
    }
}