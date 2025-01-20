using System;
using UnityEngine;

public abstract class CraneAnimation : GeoObjectAnimation
{
    // protected abstract void ActivateCrane();
    // protected abstract void DeactivateCrane();
    
    protected override void InitialzeAnimations() {
        animationQueue = new Action[2] {
            () => Activate(),
            () => Deactivate()
        };
    }

    // void OnTriggerEnter(Collider other) {
    //     if (!animationLoopRunning) {
    //         StopAllCoroutines();
    //         ActivateCrane();
    //     }
            
    // }

    // void OnTriggerExit(Collider other) {
    //     if (!animationLoopRunning) {
    //         StopAllCoroutines();
    //         DeactivateCrane();
    //     }
    // }
}
