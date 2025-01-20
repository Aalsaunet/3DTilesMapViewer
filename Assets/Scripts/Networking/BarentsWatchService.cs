using Newtonsoft.Json;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using PimDeWitte.UnityMainThreadDispatcher;

public class BarentsWatchService : MonoBehaviour
{
    ////// BARENTSWATCH STREAM SETTINGS //////
    public APIConsts.BWQueryFiler BarentsWatchFilterMethod;
    public delegate void OnBarentsWatchMessageReceivedHandler(dynamic payload);
    public event OnBarentsWatchMessageReceivedHandler BarentsWatchMessageReceived;
    private BarentsWatchReceiver receiver;
    private Task task;
    private CancellationTokenSource taskController;
    
    void Awake() {
        receiver = new BarentsWatchReceiver();
        taskController = new CancellationTokenSource();
        receiver.NewBarentsWatchMessageReceived += OnBWUpdateReceived;
        receiver.NewDiagnosticMessage += OnBWDiagnosticMessageReceived;
    }
    
    void Start()  {
        RealTimeUpdateUIController.rtcInstance.PrintLogMessage("[RT][BW] Connecting to BarentsWatch"); 
        StartCoroutine(StartBarentsWatchStream()); 
    }

    public IEnumerator StartBarentsWatchStream() {
        while (true) {
            var reqBody = $"client_id={APIConsts.BW_CLIENT_ID}&scope={APIConsts.BW_SCOPE}" +
                          $"&client_secret={APIConsts.BW_CLIENT_SECRET}&grant_type={APIConsts.BW_GRANT_TYPE}";

            using (UnityWebRequest request = UnityWebRequest.Post(APIConsts.BW_TOKEN_URL, reqBody, APIConsts.BW_CONTENT_TYPE)) {
                yield return request.SendWebRequest();
                if (request.result != UnityWebRequest.Result.Success)
                    Debug.Log(request.error);
                
                var response = JsonConvert.DeserializeObject<dynamic>(request.downloadHandler.text);
                string token = response.access_token;
                float tokenExpireInSeconds = (float)response.expires_in;
                DateTime tokenExpire = DateTime.Now.Add(TimeSpan.FromSeconds(tokenExpireInSeconds)); 
                var taskToken = taskController.Token;
                receiver.SetRelevantMMSIGeoBounds(FindTwoCoordinateBounds());
                task = Task.Run(async () => { await receiver.RunGetStreamForAll(token, tokenExpire, taskToken); });    
                RealTimeUpdateUIController.rtcInstance.PrintLogMessage("[RT][BW] BarentsWatch connection established.");
                yield return new WaitForSeconds(tokenExpireInSeconds);
            }
        }
    }

    public void UpdateStreamParameters() {
        receiver.SetRelevantMMSIGeoBounds(FindTwoCoordinateBounds());
    } 

    public static GeoBounds FindTwoCoordinateBounds() {
        float offset = GeoConsts.GEO_POSITION_RADIUS;
        return new GeoBounds(
            new Coordinate(RefManager.Get<MapController>().UnityPositionToLatLong(new Vector3(offset, 0f, offset))),
            new Coordinate(RefManager.Get<MapController>().UnityPositionToLatLong(new Vector3(-offset, 0f, -offset)))
        );
    }

    private void OnBWUpdateReceived(dynamic payload) {
        BarentsWatchMessageReceived.Invoke(payload);
    }

    private void OnBWDiagnosticMessageReceived(string msg) {
        UnityMainThreadDispatcher.Instance().Enqueue(() => { Debug.Log(msg); });
    }

    void OnDestroy() {
        receiver.NewBarentsWatchMessageReceived -= OnBWUpdateReceived;
        receiver.NewDiagnosticMessage -= OnBWDiagnosticMessageReceived;
        StopCoroutine(StartBarentsWatchStream());
        taskController.Cancel();
        task.Wait();
        taskController.Dispose();
        task.Dispose();
    }
}

    // switch (BarentsWatchFilterMethod) {
    //     case APIConsts.BWQueryFiler.MMSI:
    //         ValidateAndSetMMSIs();
    //         break;
    //     case APIConsts.BWQueryFiler.GEOMETRY:
    //         receiver.areaCoordinates = FindFourCoordinateBounds();
    //         break;
    //     case APIConsts.BWQueryFiler.ALL_GET:
    //         receiver.bounds = FindTwoCoordinateBounds();
    //         break;
    //     case APIConsts.BWQueryFiler.ALL_POST:
    //         receiver.bounds = FindTwoCoordinateBounds();
    //         break;
    //     default:
    //         return;
    // }

    // switch (BarentsWatchFilterMethod) {
    //     case APIConsts.BWQueryFiler.MMSI:
    //         Debug.Log($"Running BWStream.MMSI");
    //         ValidateAndSetMMSIs();
    //         task = Task.Run(async () => {
    //             await receiver.RunStreamForMMSIs(token, tokenExpire, taskToken);
    //         }); 
    //         break;
    //     case APIConsts.BWQueryFiler.GEOMETRY: 
    //         receiver.areaCoordinates = FindFourCoordinateBounds();
    //         task = Task.Run(async () => {
    //             await receiver.RunStreamForGeoArea(token, tokenExpire, taskToken);
    //         }); 
    //         break;
    //     case APIConsts.BWQueryFiler.ALL_GET: 
    //         task = Task.Run(async () => {
    //             receiver.SetRelevantMMSIGeoBounds(FindTwoCoordinateBounds());
    //             await receiver.RunGetStreamForAll(token, tokenExpire, taskToken);
    //         });
    //         break;
    //     case APIConsts.BWQueryFiler.ALL_POST: 
    //         Debug.Log($"Running BWStream.ALL_POST with bounds");
    //         receiver.bounds = FindTwoCoordinateBounds();
    //         task = Task.Run(async () => {
    //             await receiver.RunPostStreamForAll(token, tokenExpire, taskToken);
    //         });
    //         break;
    //     default: 
    //         yield break;
    // }

    // private int[] ValidateAndSetMMSIs() {
    //     List<int> mmsis = new();
    //     foreach (var mmsi in GeoPositionManager.mmsiToTrackingId.Keys) {
    //         if (mmsi.Length != APIConsts.MMSI_REQUIRED_DIGIT_COUNT)
    //             continue;
    //         mmsis.Add(int.Parse(mmsi));
    //     }
    //     return mmsis.ToArray();
    // } 

    // private Coordinate[] FindFourCoordinateBounds() {
    //     float offset = GeoConsts.GEO_POSITION_RADIUS;
    //     return new Coordinate[] {
    //         new(RefManager.Get<MapController>().UnityPositionToLatLong(new Vector3(-offset, 0f, offset))),
    //         new(RefManager.Get<MapController>().UnityPositionToLatLong(new Vector3(offset, 0f, offset))),
    //         new(RefManager.Get<MapController>().UnityPositionToLatLong(new Vector3(offset, 0f, -offset))),
    //         new(RefManager.Get<MapController>().UnityPositionToLatLong(new Vector3(-offset, 0f, -offset)))
    //     };
    // }

// Debug.Log("BW Token expires in: " +  (double)token.expires_in);
// Debug.Log("BW Token expiration date: " +  tokenExpirationTime.ToString());
// Debug.Log(DateTime.Now.ToString() + " < " +  tokenExpirationTime.ToString() + " = " + (DateTime.Now < tokenExpirationTime));
