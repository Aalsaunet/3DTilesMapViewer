using UnityEngine;

public class HarbourCraneAnimation : CraneAnimation
{
    // This script is intented to be used together with the HarbourCraneExtendable Prefab
    public Transform mainArm;
    public Transform supportArm;
    public AnimationCurve extendLerpProgression;
    public AnimationCurve contractLerpProgression;
    private float mainArmAngleContracted = 73.246f;
    private float mainArmAngleExtended = 0f;
    private float supportArmAngleContracted = 79.895f;
    private float supportArmAngleExtended = 0f;
    private Vector3 supportArmScaleContracted = new Vector3(1f, 1f, 1f);
    private Vector3 supportArmScaleExtended = new Vector3(1.77f, 1.69f, 1f);

    protected override void Activate() {
        StartCoroutine(LerpLocalRotation(mainArm, Quaternion.Euler(new Vector3(0f, 0f, mainArmAngleExtended))));
        StartCoroutine(LerpLocalTransform(supportArm, supportArm.localPosition, Quaternion.Euler(new Vector3(0f, 0f, supportArmAngleExtended)), supportArmScaleExtended, extendLerpProgression));
    }

    protected override void Deactivate() {
        StartCoroutine(LerpLocalRotation(mainArm, Quaternion.Euler(new Vector3(0f, 0f, mainArmAngleContracted))));
        StartCoroutine(LerpLocalTransform(supportArm, supportArm.localPosition, Quaternion.Euler(new Vector3(0f, 0f, supportArmAngleContracted)), supportArmScaleContracted, contractLerpProgression));
    }
}
