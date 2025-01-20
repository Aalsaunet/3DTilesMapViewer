using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimationController : MonoBehaviour
{
    public delegate void OnToggleAnimations(bool enable);
    public static event OnToggleAnimations AnimationSettingsChanged;

    public void OnToggleAnimationsEnabled(Toggle toggle) { 
        AnimationSettingsChanged.Invoke(toggle.isOn);
    }
}
