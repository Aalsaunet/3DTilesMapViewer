using System;
using UnityEngine;

public class ForkliftAnimation : GeoObjectAnimation
{   
    public Transform forkTransform;
    public Vector3 forkPositionLow;
    public Vector3 forkPositionHigh;
    public Transform[] wheelTransforms;
    public Vector3 wheelForwardDirection = new Vector3(1f, 0f, 0f);
    public GameObject containerModel;

    protected override void InitialzeAnimations() {
        SetupStartAndEndTransforms(transform, new Vector3(0f, 0f, 25f));
        defaultLerpDuration = 10f;
        animationInterval = 0f;
        animationQueue = new Action[4] {
            () => { MoveFork(forkTransform, forkPositionHigh); containerModel.SetActive(true); },
            () => { DriveToPosition(transform, endPosition, endRotation, wheelForwardDirection); },
            () => { MoveFork(forkTransform, forkPositionLow); },
            () => { DriveToPosition(transform, startPosition, startRotation, -wheelForwardDirection); containerModel.SetActive(false); }
        };
    }

    public void MoveFork(Transform tf, Vector3 targetPosition) {
        StartCoroutine(LerpLocalPosition(tf, targetPosition));
    }
    
    public void DriveToPosition(Transform t1, Vector3 position, Quaternion rotation, Vector3 wheelsIncrement) {
        StartCoroutine(RotateWheels(wheelTransforms, wheelsIncrement, defaultLerpDuration));
        if (t1 != null) StartCoroutine(LerpLocalTransform(t1, position, rotation, t1.localScale));
    }
}
