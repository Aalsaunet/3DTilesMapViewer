using UnityEngine;

public class BulkHandlingCraneAnimation : CraneAnimation
{
    // This script is intented to be used together with the BulkHandlingCrane Prefab
    public Transform mainCraneBody;
    private float craneAngleActivated = 0f;
    private float craneAngleDeactivated = -90f;

    protected override void Activate() {
        StartCoroutine(LerpLocalRotation(mainCraneBody, Quaternion.Euler(new Vector3(0f, craneAngleActivated, 0f))));
    }

    protected override void Deactivate() {
        StartCoroutine(LerpLocalRotation(mainCraneBody, Quaternion.Euler(new Vector3(0f, craneAngleDeactivated, 0f))));
    }
}
