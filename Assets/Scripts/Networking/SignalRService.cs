using System.Threading.Tasks;
using UnityEngine;
using System;
using Microsoft.AspNetCore.SignalR.Client;
using FourPro.Web.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Connections;
using System.Threading;

public class SignalRService : MonoBehaviour
{      
    public delegate void OnSignalRMessageReceivedHandler(GeoLocation geoLocation);
    public event OnSignalRMessageReceivedHandler SignalRMessageReceived;
    private AccessTokenUtil accessTokenUtil;
    private AccessTokenUtil.AccessToken accessToken;
    private HubConnection connection = null;
    // private CancellationTokenSource taskController;

    void Awake() {
        // taskController = new CancellationTokenSource();
        MapController.MapLocationChanged += OnMapLocationChanged;
    }

    private async void OnMapLocationChanged() {
        if (connection != null) {
            accessToken = null;
            // taskController.Cancel();
            // await connection.StopAsync(taskController.Token);
            await connection.StopAsync();
            await connection.DisposeAsync();
        }
        
        await InitAsync(GeoPositionManager.currentHubName);
    }

    private async Task InitAsync(string hubName) {
        RealTimeUpdateUIController.rtcInstance.PrintLogMessage("[RT][SR] Connecting to SignalR hub: " + hubName);
        await InitAsync<GeoLocation>(APIConsts.SIGNALR_ENDPOINT_URL + hubName, APIConsts.SIGNALR_USERNAME, APIConsts.SIGNALR_API_KEY, APIConsts.SIGNALR_METHOD_NAME);
        RealTimeUpdateUIController.rtcInstance.PrintLogMessage("[RT][SR] SignalR connection established.");
    }

    public async Task InitAsync<T>(string uri, string userName, string apiKey, string handlerMethod) where T : GeoLocation {
        accessTokenUtil = new AccessTokenUtil(apiKey);
        connection = new HubConnectionBuilder()
            .WithUrl(uri, options => {
                options.AccessTokenProvider = () => GetAccessTokenAsync(uri, userName);
                options.Transports = HttpTransportType.WebSockets;
                options.SkipNegotiation = true;
            })
            .ConfigureLogging(logging => {
                logging.SetMinimumLevel(LogLevel.Information);
                logging.AddConsole();
            })
            .WithAutomaticReconnect().Build();

        connection.ServerTimeout = TimeSpan.FromMinutes(5);
        
        connection.On<GeoLocation>(handlerMethod, geoLocation => {
            SignalRMessageReceived?.Invoke(geoLocation);
        });

        await StartConnectionAsync();
    }

    private async Task StartConnectionAsync()
    {
        try {
            // await connection.StartAsync(taskController.Token);
            await connection.StartAsync();
        }
        catch (Exception ex) {
            UnityEngine.Debug.LogError($"Error {ex.Message}");
        }
    }

    private Task<string> GetAccessTokenAsync(string uri, string userName) {
        var token = accessToken;
        if (token?.IsExpired() ?? true)
            token = accessToken = accessTokenUtil.GenerateAccessToken(uri, userName, TimeSpan.FromMinutes(30));
        return Task.FromResult(token.Token);
    }

    async void OnDestroy() {
        // taskController.Cancel();
        // await connection.StopAsync(taskController.Token);
        await connection.StopAsync();
        await connection.DisposeAsync();
        MapController.MapLocationChanged -= OnMapLocationChanged;
    }
}