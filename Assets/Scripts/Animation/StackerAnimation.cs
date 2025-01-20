using System;
using UnityEngine;

public class StackerAnimation : GeoObjectAnimation
{
    // This script is intented to be used together with the Stacker or KalmarStacker Prefab (default Vector3 values are for Stacker)
    public Transform boomTF;
    public Transform boomExtenderTF;
    public Transform boomHydraulicsTF;
    public Transform HMTF;
    public Transform HMHydraulicsTF;
    public Transform HMExtenderHydraulicsTF;
    public Transform[] wheelTransforms;
    public Vector3 boomLoweredRotation = new Vector3(0f, 0f, 0f);
    public Vector3 boomRaisedRotation = new Vector3(0f, 0f, 30f);
    public Vector3 boomContractedPosition = new Vector3(2.75f, -4.5f, 0f);
    public Vector3 boomExtendedPosition = new Vector3(10f, -4.5f, 0f);
    public Vector3 boomHydraulicsContractedPosition = new Vector3(0f, 0f, 0f);
    public Vector3 boomHydraulicsExtendedPosition = new Vector3(-0.66f, 1.45f, 0f);
    public Vector3 boomHydraulicsContractedScale = new Vector3(1f, 1f, 1f);
    public Vector3 boomHydraulicsExtendedScale = new Vector3(1f, 1f, 1f);
    public Vector3 headMountingLoweredRotation = new Vector3(0f, 0f, 0f);
    public Vector3 headMountingRaisedRotation = new Vector3(0f, 0f, -30f);
    public Vector3 headMountingHydraulicsLoweredRotation = new Vector3(0f, 0f, 0f);
    public Vector3 headMountingHydraulicsRaisedRotation = new Vector3(0f, 0f, -7f);
    public Vector3 headMountingExtenderHydraulicsLoweredPosition = new Vector3(0.628f, -1.49f, -0f);
    public Vector3 headMountingExtenderHydraulicsRaisedPosition = new Vector3(0.747f, -1.782f, -0f);
    public Vector3 wheelForwardDirection = new Vector3(0f, 0f, -1f);
    public GameObject containerModel;

    protected override void InitialzeAnimations() {
        SetupStartAndEndTransforms(transform, new Vector3(0f, 0f, 50f));
        defaultLerpDuration = 10f;
        animationInterval = 0f;
        animationQueue = new Action[6] {
            () => ExtendStackerArm(boomExtenderTF),
            () => DriveToPosition(transform, endPosition, endRotation, wheelForwardDirection),
            () => LowerStackerArm(boomTF, boomHydraulicsTF, HMTF, HMHydraulicsTF, HMExtenderHydraulicsTF),
            () => ContractStackerArm(boomExtenderTF),
            () => DriveToPosition(transform, startPosition, startRotation, -wheelForwardDirection),
            () => RaiseStackerArm(boomTF, boomHydraulicsTF, HMTF, HMHydraulicsTF, HMExtenderHydraulicsTF)
        };
    }

    public void RaiseStackerArm(Transform t1, Transform t2, Transform t3, Transform t4, Transform t5) {
        containerModel.SetActive(true);
        if (t1 != null) StartCoroutine(LerpLocalRotation(t1, Quaternion.Euler(boomRaisedRotation)));
        if (t2 != null) StartCoroutine(LerpLocalTransform(t2, boomHydraulicsExtendedPosition, t2.localRotation, boomHydraulicsExtendedScale));
        if (t3 != null) StartCoroutine(LerpLocalRotation(t3, Quaternion.Euler(headMountingRaisedRotation)));
        if (t4 != null) StartCoroutine(LerpLocalRotation(t4, Quaternion.Euler(headMountingHydraulicsRaisedRotation)));
        if (t5 != null) StartCoroutine(LerpLocalPosition(t5, headMountingExtenderHydraulicsRaisedPosition));
    }

    public void DriveToPosition(Transform t1, Vector3 position, Quaternion rotation, Vector3 wheelsIncrement) {
        StartCoroutine(RotateWheels(wheelTransforms, wheelsIncrement, defaultLerpDuration));
        if (t1 != null) StartCoroutine(LerpLocalTransform(t1, position, rotation, t1.localScale));
    }

    public void ExtendStackerArm(Transform t) {
        if (t != null) StartCoroutine(LerpLocalPosition(t, boomExtendedPosition));
    }

    public void LowerStackerArm(Transform t1, Transform t2, Transform t3, Transform t4, Transform t5) {
        if (t1 != null) StartCoroutine(LerpLocalRotation(t1, Quaternion.Euler(boomLoweredRotation)));
        if (t2 != null) StartCoroutine(LerpLocalTransform(t2, boomHydraulicsContractedPosition, t2.localRotation, boomHydraulicsContractedScale));
        if (t3 != null) StartCoroutine(LerpLocalRotation(t3, Quaternion.Euler(headMountingLoweredRotation)));
        if (t4 != null) StartCoroutine(LerpLocalRotation(t4, Quaternion.Euler(headMountingHydraulicsLoweredRotation)));
        if (t5 != null) StartCoroutine(LerpLocalPosition(t5, headMountingExtenderHydraulicsLoweredPosition));
    }

    public void ContractStackerArm(Transform t) {
        containerModel.SetActive(false);
        if (t != null) StartCoroutine(LerpLocalPosition(t, boomContractedPosition));
    }
}
        