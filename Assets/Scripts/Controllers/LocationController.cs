using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class LocationController : MonoBehaviour
{
    // public Destination currentDestination;
    public ScrollRect smallLocationScrollView;
    public ScrollRect bigLocationScrollView;
    public RectTransform errorDiaglogPanel;
    public GameObject smallLocationButtonPrefab;
    public GameObject BigLocationButtonPrefab;
    private DemoAssetsController demoAssetsController;
    private MapController mapController;
    private Dictionary<string, LocationParameters> destinationConfigs; 
    
    void Start() {
        mapController = GetComponent<MapController>();
        demoAssetsController = GetComponent<DemoAssetsController>();
        destinationConfigs = new Dictionary<string, LocationParameters>();
        LocationParameters startLocation = null;
        
        string text = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "config.json"));
        if (text != null)
            startLocation = ReadLocationsFromConfig(text);
              
        if (destinationConfigs.Count == 0) {
            ShowErrorMessage();
            return;
        }
        
        PopulateLocationUIPanel(smallLocationScrollView, smallLocationButtonPrefab, 20f);
        PopulateLocationUIPanel(bigLocationScrollView, BigLocationButtonPrefab, 45f);
        mapController.GoToLocation(startLocation);
    }

    private LocationParameters ImportLocations(string configPath) {
        using (StreamReader r = new StreamReader(configPath)) {
            string jsonContent = r.ReadToEnd();
            return ReadLocationsFromConfig(jsonContent);
        }
    }

    private LocationParameters ReadLocationsFromConfig(string configContent) {
        LocationParameters firstLocation = null;
        var config = JsonConvert.DeserializeObject<dynamic>(configContent);
        foreach (var loc in config.locations) {
            Texture2D icon = ImportLocationIcon(loc);
            if (loc?.demoAssetBundle != null && loc?.demoAssetBundle != "")
                demoAssetsController.TryEnableDemoAsset((string)loc.demoAssetBundle);

            float cameraYLimit = loc?.cameraYLimit != null ? loc.cameraYLimit : CameraController.cameraYLimit;
            var dc = new LocationParameters((string)loc.name, (double)loc.latitude, (double)loc.longitude, (string)loc?.siteId ?? "",
                                            (string)loc.api_key, (string)loc.signalr_hub, (string)loc?.demoAssetBundle ?? "", cameraYLimit, icon);
            destinationConfigs.Add((string)loc.name, dc);
            firstLocation ??= dc;

            if (loc?.geoObject_properties != null) {
                foreach (var entry in loc?.geoObject_properties) {
                    if (entry?.trackingId == null && entry?.trackingId == "") {
                        continue;
                    } else if (entry?.video_file != null && entry?.video_file != "") {
                        BrowseGeoObjectsUIController.trackingIdToYoutubeVideoFile.TryAdd((string)entry.trackingId, (string)entry.video_file);
                    } else if (entry?.video_url != null && entry?.video_url != "") {
                        string videoUrlIdPattern = @"v=([^&=]+)"; // e.g https://www.youtube.com/watch?v=qvdqU-LU_ps&ab_channel=MarcoLevermann
                        Regex r = new(videoUrlIdPattern, RegexOptions.IgnorePatternWhitespace);
                        Match m = r.Match((string)entry.video_url);
                        if (!m.Success || m.Groups.Count < 1)
                            continue;
                        BrowseGeoObjectsUIController.trackingIdToYoutubeVideoId.TryAdd((string)entry.trackingId, m.Groups[1].Value);
                    }
                }
            }
        }
        return firstLocation;
    }

    private static Texture2D ImportLocationIcon(dynamic loc) {
        Texture2D icon = null;
        if (loc?.icon_base64 != null && loc?.icon_base64 != "") {
            try {
                byte[] asBytes = Convert.FromBase64String((string)loc.icon_base64);
                icon = new Texture2D(30, 30);
                bool success = icon.LoadImage(asBytes);
                if (!success)
                    icon = null;
            }
            catch {
                icon = null;
            }
        }
        return icon;
    }

    private void ShowErrorMessage() {
        errorDiaglogPanel.gameObject.SetActive(true);
    }

    public void OnClickingApplicationQuitButton() {
        Application.Quit();
    }

    private void PopulateLocationUIPanel(ScrollRect scrollView, GameObject buttonPrefab, float buttonHeight) {
        foreach (Transform c in scrollView.content.transform)
            Destroy(c.gameObject);
        scrollView.content.sizeDelta = new Vector2(scrollView.content.sizeDelta.x, 0f);

        foreach (var location in destinationConfigs.Values) {
            GameObject locationButton = Instantiate(buttonPrefab);
            
            locationButton.transform.Find("LocationName").GetComponent<TextMeshProUGUI>().text = location.name;
            locationButton.transform.Find("LatitudeValue").GetComponent<TextMeshProUGUI>().text = $"{location.latitude}";
            locationButton.transform.Find("LongitudeValue").GetComponent<TextMeshProUGUI>().text = $"{location.longitude}";
            
            if (location.icon != null) {
                locationButton.transform.Find("LocationImage").GetComponent<RawImage>().texture = location.icon;
                locationButton.transform.Find("LocationImage").GetComponent<RawImage>().color = Color.white;
            }
                
            
            float breath = scrollView.content.sizeDelta.x;
            float height = scrollView.content.sizeDelta.y;
            scrollView.content.sizeDelta = new Vector2(breath, height + buttonHeight);
            locationButton.transform.SetParent(scrollView.content);
            locationButton.SetActive(true);
            locationButton.GetComponent<Button>().onClick.AddListener(() => {
                mapController.GoToLocation(location);
            });
        }
    }
}
    // public void OnClickingLocationButton(LocationParameters location) {
    //     mapController.GoToLocation(location);
    // }

    // public void OnClickingLocationButton(TextMeshProUGUI textField) {
    //     string locationText = textField.text.Trim();
    //     if (destinationConfigs.ContainsKey(locationText)) {
    //         DestinationConfig dstConfig = destinationConfigs[locationText];
    //         currentDestination = (Destination)dstConfig.destinationIndex;
    //         demoAssetsController.SetDemoAssetFromDestinationConfig(dstConfig.destinationIndex);
    //         mapController.GoToLocation(dstConfig.latitude, dstConfig.longitude, dstConfig.apiKey, dstConfig.signalRHub, locationText);
    //     }
    // } 

    // public void OnNextDestinationClick() {
    //     currentDestination = (Destination)((int)currentDestination + 1 % destinationConfigs.Count);
    //     LocationParameters config = destinationConfigs[currentDestination.GetName()];
    //     currentDestination = (Destination)config.destinationIndex;
    //     // demoAssetsController.SetDemoAssetFromDestinationConfig(config.destinationIndex);
    //     mapController.GoToLocation(config.latitude, config.longitude, config.apiKey, config.signalRHub, config.name);
    // }

    // public void OnPreviousDestinationClick() {
    //     int dstIndex = (int)currentDestination;
    //     currentDestination = (Destination)(dstIndex == 0 ? destinationConfigs.Count - 1 : dstIndex - 1);
    //     LocationParameters config = destinationConfigs[currentDestination.GetName()];
    //     currentDestination = (Destination)config.destinationIndex;
    //     // demoAssetsController.SetDemoAssetFromDestinationConfig(config.destinationIndex);
    //     mapController.GoToLocation(config.latitude, config.longitude, config.apiKey, config.signalRHub, config.name);
    // }

    // if (File.Exists(testConfigPathWindows))
        //     startLocation = ImportLocations(testConfigPathWindows);
    
    // private LocationParameters ImportLocations(string configPath) {
    //     LocationParameters firstLocation = null;
    //     using (StreamReader r = new StreamReader(configPath)) {
    //         string jsonContent = r.ReadToEnd();
    //         var config = JsonConvert.DeserializeObject<dynamic>(jsonContent);
    //         foreach (var loc in config.locations) {
    //             Texture2D icon = ImportLocationIcon(loc);
    //             if (loc?.demoAssetBundle != null && loc?.demoAssetBundle != "")
    //                 demoAssetsController.TryEnableDemoAsset((string)loc.demoAssetBundle);

    //             var dc = new LocationParameters((string)loc.name, (double)loc.latitude, (double)loc.longitude,
    //                                             (string)loc.api_key, (string)loc.signalr_hub, (string)loc?.demoAssetBundle ?? "", icon);
    //             destinationConfigs.Add((string)loc.name, dc);
    //             firstLocation ??= dc;

    //         }
    //     }
    //     return firstLocation;
    // }

    // destinationConfigs[Destination.east.GetName()] = new DestinationConfig(Destination.east.GetName(), 59.427028, 10.657558, APIConsts.GEO_API_KEY_DEMO, APIConsts.SIGNALR_HUB_DEMO);
    // destinationConfigs[Destination.west.GetName()] = new DestinationConfig(Destination.west.GetName(), 61.937306, 5.135252, APIConsts.GEO_API_KEY_DEMO, APIConsts.SIGNALR_HUB_DEMO);
    // destinationConfigs[Destination.oslo.GetName()] = new DestinationConfig(Destination.oslo.GetName(), 59.918333916137954, 10.725335647334093, APIConsts.GEO_API_KEY_DEMO, APIConsts.SIGNALR_HUB_DEMO);
    // destinationConfigs[Destination.bergen.GetName()] = new DestinationConfig(Destination.bergen.GetName(), 60.3913, 5.3221, APIConsts.GEO_API_KEY_DEMO, APIConsts.SIGNALR_HUB_DEMO);
    // destinationConfigs[Destination.trondheim.GetName()] = new DestinationConfig(Destination.trondheim.GetName(), 63.44205336455276, 10.405652561099153, APIConsts.GEO_API_KEY_TRONDHEIM, APIConsts.SIGNALR_HUB_TRONDHEIM);
    // destinationConfigs[Destination.tanager.GetName()] = new DestinationConfig(Destination.tanager.GetName(), 58.92412889443265, 5.6005701749992935, APIConsts.GEO_API_KEY_TAN, APIConsts.SIGNALR_HUB_TAN);
    // destinationConfigs[Destination.fredrikstad.GetName()] = new DestinationConfig(Destination.fredrikstad.GetName(), 59.2205, 10.9347, APIConsts.GEO_API_KEY_DEMO, APIConsts.SIGNALR_HUB_DEMO);
    // destinationConfigs[Destination.kristiansund.GetName()] = new DestinationConfig(Destination.kristiansund.GetName(), 63.056534975522588, 7.67202449953841, APIConsts.GEO_API_KEY_KB, APIConsts.SIGNALR_HUB_KB);
    // destinationConfigs[Destination.aberdeen.GetName()] = new DestinationConfig(Destination.aberdeen.GetName(), 57.194397201884804, -2.2159616423246185, APIConsts.GEO_API_KEY_DEMO, APIConsts.SIGNALR_HUB_DEMO);
    // destinationConfigs[Destination.senegal.GetName()] = new DestinationConfig(Destination.senegal.GetName(), 14.674609, -17.42804, APIConsts.GEO_API_KEY_DEMO, APIConsts.SIGNALR_HUB_DEMO);
    // destinationConfigs[Destination.tromso.GetName()] = new DestinationConfig(Destination.tromso.GetName(), 69.679607, 18.987792, APIConsts.GEO_API_KEY_DEMO, APIConsts.SIGNALR_HUB_DEMO);
    // startLoc = destinationConfigs[currentDestination.GetName()];