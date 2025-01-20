using System;
using UnityEngine;

public class GateAnimation : GeoObjectAnimation
{
    public Vector3 localSlideOffset;
    private Transform meshTF;

    void Awake() {
        meshTF = transform.Find("Sketchfab_model");
    }

    protected override void InitialzeAnimations() {
        SetupStartAndEndTransforms(meshTF, localSlideOffset);
        defaultLerpDuration = 6f;
        animationInterval = 10f;
        animationQueue = new Action[2] {
            () => Activate(),
            () => Deactivate()
        };
    }

    protected override void Activate() { // Open gate
        StartCoroutine(LerpLocalPosition(meshTF, localEndPosition));
    }

    protected override void Deactivate() { // close gate
        StartCoroutine(LerpLocalPosition(meshTF, localStartPosition));
    }    
}
