using UnityEngine;
using PimDeWitte.UnityMainThreadDispatcher;
using System;

public class RealTimePositionManager : MonoBehaviour
{
    public AnimationCurve defaultPositionUpdateLerpCurve; //curve that simulates acceleration and deacceleration
    public static BarentsWatchService bwService;
    public static SignalRService signalRService;

    public delegate void OnRealTimePositionReceived();
    public static event OnRealTimePositionReceived RealTimePositionReceived;

    void Start() {
        bwService = GetComponent<BarentsWatchService>();
        signalRService = GetComponent<SignalRService>();
        bwService.BarentsWatchMessageReceived += OnBarentsWatchEventReceived;
        signalRService.SignalRMessageReceived += OnSignalREventReceived;
    } 

    private void OnSignalREventReceived(GeoLocation gl) {
        if (gl.SourceType == "logistics.warehouseentry") {
            RefManager.Get<CCUManager>().MoveStackedCCU(gl);
            return;
        }
            
        if (VesselManager.mmsiToGeoObjectInstance.ContainsKey(gl.TrackingId.Trim())) {
            return; // Ignore RT updates for Vessels since BarentsWatch takes care of that.
        }

        if (gl.TrackingId != null && GeoPositionManager.trackingIdToGeoObjectInstance.TryGetValue(gl.TrackingId, out var goi)) {
            // gl.TimeStamp = DateTime.Now;
            switch (gl.Event) {
                case APIConsts.SIGNALR_EVENT_POSITION_UPDATE: UpdateRealTimePosition(goi, gl, APIConsts.RTSource.SR); break;
                case APIConsts.SIGNALR_EVENT_PICKUP: RealTimeUpdateUIController.rtcInstance.PrintLogMessage("[RT][SR] Pickup event received from SignalR"); break;
                case APIConsts.SIGNALR_EVENT_STACK: RealTimeUpdateUIController.rtcInstance.PrintLogMessage("[RT][SR] Stack event received from SignalR"); break;
                default: UpdateRealTimePosition(goi, gl, APIConsts.RTSource.SR); break;
            } 
        }     
    }

    private void OnBarentsWatchEventReceived(dynamic payload) {
        if (VesselManager.mmsiToGeoObjectInstance.TryGetValue(Convert.ToString(payload.mmsi), out GeoObjectInstance goi)) {
            if (goi.info.IsFromGeoPositionAPI && goi.trackerOffsets == null)
                UnityMainThreadDispatcher.Instance().Enqueue(() => { RefManager.Get<VesselManager>().SetAISOffsetsAndScale(goi, payload); });
            UpdateRealTimePosition(goi, ConvertToGeoLocation(payload), APIConsts.RTSource.BW);
        } else {
            InstantiateBWVessel(payload); 
        }
    }

    private void InstantiateBWVessel(dynamic payload) {
        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            // Debug.Log("Received Barentswatch event on " + payload.name + ", type: " + payload.shipType + ". Instantiating it.");
            Vector3 position = RefManager.Get<MapController>().LatLongToUnityPosition((double)payload.latitude, (double)payload.longitude);
            Quaternion rotation = (payload.trueHeading != null) ? Quaternion.Euler(0.0f, (float)payload.trueHeading, 0.0f)
                                                                : Quaternion.Euler(0f, 0f, 0f);
            Guid guid = Guid.NewGuid();
            GeoPosition gpos = new() {
                Id = guid,
                SourceEntityId = guid,
                DisplayName = payload.name,
                TrackingId = Convert.ToString(payload.mmsi),
                CategoryCode = CategoryCode.VESSEL,
                GeoModelCode = payload.shipType
            };

            RefManager.Get<VesselManager>().InstantiateVessel(gpos, position, rotation, false);
            if (VesselManager.mmsiToGeoObjectInstance.TryGetValue(gpos.TrackingId, out GeoObjectInstance goi)) {
                UnityMainThreadDispatcher.Instance().Enqueue(() => { RefManager.Get<VesselManager>().SetAISOffsetsAndScale(goi, payload); });
                UpdateRealTimePosition(goi, ConvertToGeoLocation(payload), APIConsts.RTSource.BW);
            }
        });
    }

    private GeoLocation ConvertToGeoLocation(dynamic payload) {
        return new GeoLocation {
            TimeStamp = DateTime.Now,
            Latitude = payload.latitude,
            Longitude = payload.longitude,
            TrackingId = Convert.ToString(payload.mmsi),
            Direction = payload.trueHeading
        };
    }

    private void UpdateRealTimePosition(GeoObjectInstance goi, GeoLocation gl, APIConsts.RTSource source) {
        UnityMainThreadDispatcher.Instance().Enqueue(() => 
            RealTimeUpdateUIController.rtcInstance.PrintLogMessage("[RT][" + source.ToString() + "] " 
            + goi.gameObject.name + " received realtime update.")
        );

        UnityMainThreadDispatcher.Instance().Enqueue(() => {
            Vector3 newPosition = RefManager.Get<MapController>().FindHeightAdjustedPosition(gl, goi);
            Quaternion newRotation = (gl.Direction != null) ? Quaternion.Euler(0.0f, (float)gl.Direction, 0.0f) : goi.transform.rotation;
            goi.AddGeoLocation(gl, newPosition, newRotation, source);
        });

        RealTimePositionReceived.Invoke();
    } 

    void OnDestroy() {
        signalRService.SignalRMessageReceived -= OnSignalREventReceived;
        bwService.BarentsWatchMessageReceived -= OnBarentsWatchEventReceived;
    }
}
