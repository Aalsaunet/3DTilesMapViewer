using System;
using UnityEngine;

public class RTGCraneAnimation : GeoObjectAnimation
{
     // This script is intented to be used together with the RTGCrane Prefab
    public Transform[] wheelTransforms;

    protected override void InitialzeAnimations() {
        SetupStartAndEndTransforms(transform, new Vector3(0f, 0f, -40f));
        animationQueue = new Action[2] {
            () => MoveRTGCrane(endPosition, new Vector3(-1f, 0f, 0f)),
            () => MoveRTGCrane(startPosition, new Vector3(1f, 0f, 0f))
        };
    }

    public void MoveRTGCrane(Vector3 targetPosition, Vector3 wheelRotationIncrement) {
        StartCoroutine(RotateWheels(wheelTransforms, wheelRotationIncrement, defaultLerpDuration));
        StartCoroutine(LerpGeoObjectWorldPosition(targetPosition));
    }
}
