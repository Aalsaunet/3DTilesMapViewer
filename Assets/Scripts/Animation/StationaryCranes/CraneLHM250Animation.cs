using UnityEngine;

public class CraneLHM250Animation : CraneAnimation
{
    // This script is intented to be used together with the CraneLHMAnimated Prefab
    public Transform mainCraneBody;
    private float craneAngleActivated = 0f;
    private float craneAngleDeactivated = 90f;

    protected override void Activate() {
        StartCoroutine(LerpLocalRotation(mainCraneBody, Quaternion.Euler(new Vector3(90f, 0f, craneAngleActivated))));
    }

    protected override void Deactivate() {
        StartCoroutine(LerpLocalRotation(mainCraneBody, Quaternion.Euler(new Vector3(90f, 0f, craneAngleDeactivated))));
    }


}
