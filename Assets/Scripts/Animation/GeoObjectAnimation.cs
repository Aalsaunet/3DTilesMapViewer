using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GeoObjectAnimation : MonoBehaviour
{ 
    // TRANSFORM PROPERTIES //
    protected Vector3 localStartPosition;
    protected Vector3 localEndPosition;
    protected Vector3 startPosition;
    protected Vector3 endPosition;
    protected Quaternion startRotation;
    protected Quaternion endRotation;

    // DATA STRUCTURES //
    protected Action[] animationQueue;
    protected HashSet<int> geoObjectsInTriggerArea;

    // ANIMATION SETTINGS //
    protected float defaultLerpDuration = 30f;
    protected float animationInterval = 15f;

    // ANIMATION STATE //
    protected bool animationLoopKeyframesInitialized = false;
    protected bool animationLoopRunning = false;
    protected int animationIndex = 0;
    
    // ANIMATION STATE //
    protected abstract void InitialzeAnimations();
    protected virtual void Activate() {}
    protected virtual void Deactivate() {}
    
    void Start() {
        AnimationController.AnimationSettingsChanged += HandleAnimationSettingsChanged;
        geoObjectsInTriggerArea = new HashSet<int>(10);
    }

    protected void HandleAnimationSettingsChanged(bool enableAnimation) {
        if (!animationLoopKeyframesInitialized) {
            InitialzeAnimations();
            animationLoopKeyframesInitialized = true;
        }

        animationLoopRunning = enableAnimation;
        if (enableAnimation) 
            StartCoroutine(LoopAnimation());
    }

    protected IEnumerator LoopAnimation() {
        while (animationLoopRunning) {
            animationIndex = animationIndex >= animationQueue.Length ? 0 : animationIndex;
            animationQueue[animationIndex++]();
            if (animationLoopRunning)
                yield return new WaitForSeconds(defaultLerpDuration + animationInterval);
        }
    }

    protected void SetupStartAndEndTransforms(Transform tf, Vector3 localPositionDelta) {
        localStartPosition = tf.localPosition;
        localEndPosition = localStartPosition + localPositionDelta;
        startPosition = tf.position;
        endPosition = startPosition + tf.TransformDirection(localPositionDelta);
        if (Physics.Raycast(endPosition, Vector3.down, out RaycastHit downHit, GeoConsts.RAYCAST_LENGTH, LayerMask.GetMask(GeoConsts.RAYCAST_LAYER_TERRAIN)))
            endPosition = downHit.point;
        startRotation = tf.rotation;
        endRotation = startRotation;
    }

    protected IEnumerator LerpLocalPosition(Transform tf, Vector3 targetLocalPosition, AnimationCurve lerpCurve = null) {    
        float elapsedTime = 0;
        Vector3 startLocalPosition = tf.localPosition;
        
        while (elapsedTime <= defaultLerpDuration) {
            float progress = lerpCurve != null ? lerpCurve.Evaluate(elapsedTime / defaultLerpDuration) : elapsedTime / defaultLerpDuration;
            tf.localPosition = Vector3.Lerp(startLocalPosition, targetLocalPosition, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        tf.localPosition = targetLocalPosition;
    }

    protected IEnumerator LerpGeoObjectWorldPosition(Vector3 targetPosition) {    
        float elapsedTime = 0;
        Vector3 startPosition = transform.position;
        
        while (elapsedTime <= defaultLerpDuration) {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / defaultLerpDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
    }

    protected IEnumerator LerpLocalRotation(Transform tf, Quaternion targetRotation, AnimationCurve lerpCurve = null) {    
        float elapsedTime = 0;
        Quaternion startLocalRotation = tf.localRotation;
        
        while (elapsedTime <= defaultLerpDuration) {
            float progress = lerpCurve != null ? lerpCurve.Evaluate(elapsedTime / defaultLerpDuration) : elapsedTime / defaultLerpDuration;
            tf.localRotation = Quaternion.Lerp(startLocalRotation, targetRotation, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        tf.localRotation = targetRotation;
    }

    protected IEnumerator LerpLocalScale(Transform tf, Vector3 targetScale, AnimationCurve lerpCurve = null) {    
        float elapsedTime = 0;
        Vector3 startLocalScale = tf.localScale;
        
        while (elapsedTime <= defaultLerpDuration) {
            float progress = lerpCurve != null ? lerpCurve.Evaluate(elapsedTime / defaultLerpDuration) : elapsedTime / defaultLerpDuration;
            tf.localScale = Vector3.Lerp(startLocalScale, targetScale, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        tf.localScale = targetScale;
    }

    protected IEnumerator LerpLocalTransform(Transform tf, Vector3 targetPosition, Quaternion targetRotation, Vector3 targetScale, AnimationCurve lerpCurve = null) {    
        float elapsedTime = 0;
        Vector3 startLocalPosition = tf.localPosition;
        Quaternion startLocalRotation = tf.localRotation;
        Vector3 startLocalScale = tf.localScale;
        
        while (elapsedTime <= defaultLerpDuration) {
            float progress = lerpCurve != null ? lerpCurve.Evaluate(elapsedTime / defaultLerpDuration) : elapsedTime / defaultLerpDuration;
            tf.SetLocalPositionAndRotation(Vector3.Lerp(startLocalPosition, targetPosition, progress), Quaternion.Lerp(startLocalRotation, targetRotation, progress));
            tf.localScale = Vector3.Lerp(startLocalScale, targetScale, elapsedTime / defaultLerpDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        tf.SetLocalPositionAndRotation(targetPosition, targetRotation);
        tf.localScale = targetScale;
    }

    public IEnumerator RotateWheels(Transform[] wheels, Vector3 rotationIncrement, float lerpDuration) {    
        float elapsedTime = 0;  
        while (elapsedTime <= lerpDuration) {
            foreach (var wheel in wheels)
            wheel.Rotate(rotationIncrement, Space.Self);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    protected void OnTriggerEnter(Collider goi) {
        if (!goi.CompareTag(GeoConsts.GEO_OBJECT_TAG) || animationLoopRunning)
            return;
        
        if (geoObjectsInTriggerArea.Contains(goi.GetHashCode()))
            return;

        geoObjectsInTriggerArea.Add(goi.GetHashCode());
        if (geoObjectsInTriggerArea.Count == 1) {
            StopAllCoroutines();
            Activate(); 
        }
        
    }

    protected void OnTriggerExit(Collider goi) {
        if (!goi.CompareTag(GeoConsts.GEO_OBJECT_TAG) || animationLoopRunning)
            return;

        if (!geoObjectsInTriggerArea.Contains(goi.GetHashCode()))
            return;
            
        geoObjectsInTriggerArea.Remove(goi.GetHashCode());
        if (geoObjectsInTriggerArea.Count == 0) {
            StopAllCoroutines();
            Deactivate(); 
        }      
    }

    void OnDestroy() {
        AnimationController.AnimationSettingsChanged -= HandleAnimationSettingsChanged;
    }
}
