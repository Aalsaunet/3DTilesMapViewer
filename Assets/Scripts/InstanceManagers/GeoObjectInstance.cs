using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class GeoObjectInstance : MonoBehaviour
{
    public GeoInfo info;
    public bool isLerping = false;
    public bool isStackingChart = false;
    public bool isLightMast = false;
    public bool isMaintenanceStatusSet = false;
    public AISTrackerOffsets trackerOffsets;
    public Light[] lights;
    public MaintenanceStatus maintenanceStatus;
    private GeoObjectAnimation goa;
    private GeoLocation previousLocation;
    private Queue<(Vector3, Quaternion, APIConsts.RTSource, float)> geoLocations;
    private AnimationCurve defaultLerpCurve;

    void Awake() {
        RealTimeUpdateUIController.rtcInstance.PrintLogMessage("[NRT] " + gameObject.name + " instantiated on map.");
        geoLocations = new Queue<(Vector3, Quaternion, APIConsts.RTSource, float)>();
        lights = GetComponentsInChildren<Light>(true);
        EnableLightsAndLitWindows(WeatherController.isDark);
        goa = GetComponent<GeoObjectAnimation>();
        defaultLerpCurve = RefManager.Get<RealTimePositionManager>().defaultPositionUpdateLerpCurve;
        maintenanceStatus = MaintenanceStatus.UNKNOWN;
    }

    void Update() {
        if (!isLerping && geoLocations.Count > 0) {
            isLerping = true;
            var (pos, rot, source, lerpSeconds) = geoLocations.Dequeue();
            //Accelerate and deaccelerate if it's the only geoLocation or move linearly if there are more. 
            var lerpCurve = geoLocations.Count == 0 ? defaultLerpCurve : null; 
            lerpSeconds += 1f; // Smooth out the lerp by adding a bit of lerp time
            StartCoroutine(LerpPosition(pos, rot, source, lerpSeconds, lerpCurve));
            
            if (goa != null && goa is StackerAnimation)  {
                StackerAnimation sa = goa as StackerAnimation;
                StartCoroutine(sa.RotateWheels(sa.wheelTransforms, sa.wheelForwardDirection, lerpSeconds));  
            }                    
        }
    }

    public void EnableLightsAndLitWindows(bool enable) {
        if (lights != null)
            foreach (var light in lights)
                light.gameObject.SetActive(enable);
        
        Transform tf = transform.Find("LitWindows");
        if (tf != null)
            tf.gameObject.SetActive(enable);
    }

    public void AddGeoLocation(GeoLocation gl, Vector3 pos, Quaternion rot, APIConsts.RTSource source) {
        if (previousLocation == null) {
            if (trackerOffsets != null) { // Increase targetPosition precision by applying AIS offsets
                Vector3 offsetVector = VesselManager.GetAISOffsetVector(trackerOffsets);
                transform.position = pos + transform.right * offsetVector.x + transform.up * offsetVector.y + transform.forward * offsetVector.z;
            } else
                transform.position = pos;
            
            transform.rotation = rot;
            previousLocation = gl;
            return;
        }

        // Leap from L - 1 to L with timedelta T   
        TimeSpan? result = gl.TimeStamp - previousLocation.TimeStamp;
        float lerpSeconds = Math.Abs((float)(result?.TotalSeconds ?? 0f)) % 3600;          
        geoLocations.Enqueue((pos, rot, source, lerpSeconds));
        previousLocation = gl;
    }

    IEnumerator LerpPosition(Vector3 targetPosition, Quaternion targetRotation, APIConsts.RTSource source, float lerpSeconds, AnimationCurve lerpCurve) {  
        if (trackerOffsets != null) { // Increase targetPosition precision by applying AIS offsets
            Vector3 offsetVector = VesselManager.GetAISOffsetVector(trackerOffsets);
            targetPosition = targetPosition + transform.right * offsetVector.x + transform.up * offsetVector.y + transform.forward * offsetVector.z;
        }

        // Vector3 origin = new Vector3(targetPosition.x, targetPosition.y + 1000f, targetPosition.z);
        // if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, GeoConsts.RAYCAST_LENGTH, LayerMask.GetMask(GeoConsts.RAYCAST_LAYER_TERRAIN)))
        //     targetPosition = hit.point;

        float elapsedTime = 0;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float lerpDistance = Vector3.Distance(transform.position, targetPosition);
        
        RealTimeUpdateUIController.rtcInstance.PrintLogMessage("[RT][" + source.ToString() + "] Moving " + gameObject.name + " " + lerpDistance + "m for " + lerpSeconds + "s.");

        while (elapsedTime <= lerpSeconds) {
            float progress = lerpCurve != null ? lerpCurve.Evaluate(elapsedTime / lerpSeconds) : elapsedTime / lerpSeconds;
            Vector3 newPos = Vector3.Lerp(startPosition, targetPosition, progress);
            float newY;

            if (Physics.Raycast(new Vector3(newPos.x, newPos.y + GeoConsts.RAYCAST_ORIGIN_OFFSET, newPos.z), Vector3.down, out RaycastHit downHit, GeoConsts.RAYCAST_LENGTH, LayerMask.GetMask(GeoConsts.RAYCAST_LAYER_TERRAIN)))
                newY = downHit.point.y;
            else 
                newY = Math.Max(targetPosition.y, startPosition.y);
            
            transform.position = new Vector3(newPos.x, newY, newPos.z);
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isLerping = false;
    }
}
