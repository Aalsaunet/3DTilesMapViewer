using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using YoutubePlayer.Components;

public class BrowseGeoObjectsUIController : MonoBehaviour
{
    public ScrollRect scrollView;
    public GameObject categoryButtonPrefab;
    public GameObject geoObjectButtonPrefab;
    
    public GameObject geoObjectInfoPanel;
    public TextMeshProUGUI geoObjectInfoHeader;
    public TextMeshProUGUI geoObjectPropertiesText;
    public Transform geoObjectCamera;
    public Button goToGeoObjectButton;
    
    public RenderTexture geoObjectRenderTexture;
    public RenderTexture liveStreamRenderTexture;
    public RawImage renderTarget;

    public GameObject liveVideoReceiver;
    public VideoPlayer videoPlayer;

    public RawImage MaintenanceIcon;
    public RawImage MaintenanceIconOverlay;
    public TextMeshProUGUI maintenanceStatusText;
    public Texture2D OkIcon;
    public Texture2D WarningIcon;
    public Texture2D ErrorIcon;

    public static Dictionary<string, string> trackingIdToYoutubeVideoId;
    public static Dictionary<string, string> trackingIdToYoutubeVideoFile;

    private CameraController camCtrler;
    

    void Awake() {
        camCtrler = GetComponent<CameraController>();
        trackingIdToYoutubeVideoId = new Dictionary<string, string>();
        trackingIdToYoutubeVideoFile = new Dictionary<string, string>();
        GeoPositionManager.GeoPositionsLoaded += GenerateGeoObjectButtonsByCategory;
    }

    public static void AddEntry(string categoryName, GeoObjectInstance goi) {
        if (!GeoPositionManager.categoryCodeToGeoObjects.ContainsKey(categoryName))
            GeoPositionManager.categoryCodeToGeoObjects.Add(categoryName, new List<GeoObjectInstance>());
        GeoPositionManager.categoryCodeToGeoObjects[categoryName].Add(goi); 
    }

    public void GenerateGeoObjectButtonsByCategory() {
        foreach (Transform c in scrollView.content.transform)
            Destroy(c.gameObject);
        scrollView.content.sizeDelta = new Vector2(scrollView.content.sizeDelta.x, 0f);

        foreach (KeyValuePair<string, List<GeoObjectInstance>> categoryAndGOs in GeoPositionManager.categoryCodeToGeoObjects) {
            GameObject categoryButton = Instantiate(categoryButtonPrefab);
            
            categoryButton.transform.Find("CategoryName").GetComponent<TextMeshProUGUI>().text = categoryAndGOs.Key;
            float breath = scrollView.content.sizeDelta.x;
            float height = scrollView.content.sizeDelta.y;
            scrollView.content.sizeDelta = new Vector2(breath, height + categoryAndGOs.Value.Count * 25f);
            categoryButton.transform.SetParent(scrollView.content);
            categoryButton.SetActive(true);
            
            List<GameObject> gogo = new List<GameObject>();
            foreach (GeoObjectInstance goi in categoryAndGOs.Value) {
                GameObject geoObjectButton = Instantiate(geoObjectButtonPrefab);
                geoObjectButton.GetComponent<Button>().onClick.AddListener(() => OnGeoObjectButtonClicked(goi));
                geoObjectButton.transform.Find("GeoObjectName").GetComponent<TextMeshProUGUI>().text = goi.info.DisplayName;
                geoObjectButton.transform.SetParent(scrollView.content);
                geoObjectButton.SetActive(false);
                gogo.Add(geoObjectButton);
            }

            categoryButton.GetComponent<Button>().onClick.AddListener(() => OnCategoryButtonClicked(gogo));
        }
    }

    public void OnCategoryButtonClicked(List<GameObject> gogo) {
        foreach (GameObject go in gogo) {
            go.SetActive(!go.activeSelf);
        }
    }

    public void OnGeoObjectButtonClicked(GeoObjectInstance goi) {
        OpenGeoObjectInfoPanel(goi);
    }

    public void OpenGeoObjectInfoPanel(GeoObjectInstance goi) {
        geoObjectInfoPanel.SetActive(true);
        geoObjectInfoHeader.text = goi.info.DisplayName;
        geoObjectPropertiesText.text = goi.info.PrintProperties();
        goToGeoObjectButton.onClick.AddListener(() => camCtrler.MoveToGeoObject(goi.gameObject));
        SetMaintenanceStatus(goi);

        if (trackingIdToYoutubeVideoFile.ContainsKey(goi.info.TrackingId)) {
            // Display live stream instead of geoObject camera
            liveStreamRenderTexture.Release();
            videoPlayer.url = Path.Combine(Application.streamingAssetsPath, trackingIdToYoutubeVideoFile[goi.info.TrackingId]);
            renderTarget.texture = liveStreamRenderTexture;
            liveVideoReceiver.SetActive(true);
            videoPlayer.Play();
        } 
        // else if (trackingIdToYoutubeVideoId.ContainsKey(goi.info.TrackingId)) {
        //     // Display live stream instead of geoObject camera
        //     if (trackingIdToYoutubeVideoId[goi.info.TrackingId] != videoPlayer.VideoId) {
        //         liveStreamRenderTexture.Release();
        //         videoPlayer.VideoId = trackingIdToYoutubeVideoId[goi.info.TrackingId];
        //     }

        //     renderTarget.texture = liveStreamRenderTexture;
        //     liveVideoReceiver.SetActive(true);
        //     _ = videoPlayer.PlayVideoAsync();
        // } 
        else {
            liveVideoReceiver.SetActive(false);
            renderTarget.texture = geoObjectRenderTexture;
            geoObjectCamera.gameObject.SetActive(true);
            geoObjectCamera.parent = goi.transform;

            Transform thirdPersonTF = goi.transform.Find("TPVP");
            if (thirdPersonTF != null) {
                geoObjectCamera.position = thirdPersonTF.position;
                geoObjectCamera.rotation = thirdPersonTF.rotation;
            } else {
                geoObjectCamera.localPosition = new Vector3(-10f, 5f, 20f);
                geoObjectCamera.localRotation = Quaternion.Euler(new Vector3(5f, 152f, 0f));
            }
        } 
    }

    private void SetMaintenanceStatus(GeoObjectInstance goi) {
        switch (goi.maintenanceStatus) {
            case MaintenanceStatus.OK:
                maintenanceStatusText.text = "OK";
                MaintenanceIcon.gameObject.SetActive(true);
                MaintenanceIcon.texture = OkIcon;
                MaintenanceIconOverlay.gameObject.SetActive(true);
                MaintenanceIconOverlay.texture = OkIcon;
                break;
            case MaintenanceStatus.NEED_SERVICE:
                maintenanceStatusText.text = "Need of service";
                MaintenanceIcon.gameObject.SetActive(true);
                MaintenanceIcon.texture = WarningIcon;
                MaintenanceIconOverlay.gameObject.SetActive(true);
                MaintenanceIconOverlay.texture = WarningIcon;
                break;
            case MaintenanceStatus.OUT_OF_SERVICE:
                maintenanceStatusText.text = "Out of service";
                MaintenanceIcon.gameObject.SetActive(true);
                MaintenanceIcon.texture = ErrorIcon;
                MaintenanceIconOverlay.gameObject.SetActive(true);
                MaintenanceIconOverlay.texture = ErrorIcon;
                break;
            default:
                maintenanceStatusText.text = "N/A";
                MaintenanceIcon.gameObject.SetActive(false);
                MaintenanceIcon.texture = null;
                MaintenanceIconOverlay.gameObject.SetActive(false);
                MaintenanceIconOverlay.texture = null;
                break;
        }
    }

    public void Reset() {
        liveVideoReceiver.SetActive(false);
        geoObjectInfoPanel.SetActive(false);
        geoObjectCamera.gameObject.SetActive(false);
        geoObjectCamera.parent = null;
        renderTarget.texture = geoObjectRenderTexture;
        maintenanceStatusText.text = "N/A";
        MaintenanceIcon.gameObject.SetActive(false);
        MaintenanceIconOverlay.gameObject.SetActive(false);
    }

    void OnDestroy() {
        GeoPositionManager.GeoPositionsLoaded -= GenerateGeoObjectButtonsByCategory;
    }
}


// // Display live stream instead of geoObject camera
//             liveVideoReceiver.SetActive(true);
//             videoPlayer.PrepareVideoAsync


//             if (trackingIdToYoutubeVideoId[goi.info.TrackingId] != videoPlayer.VideoId) {
//                 liveStreamRenderTexture.DiscardContents();
//                 // videoPlayer.VideoPlayer.Pause();
//                 videoPlayer.VideoId = trackingIdToYoutubeVideoId[goi.info.TrackingId];
//                 videoPlayer.PlayVideoAsync(); // videoPlayer.PlayVideoAsync();
//             }
//             renderTarget.texture = liveStreamRenderTexture;

//             // if (videoPlayer.VideoId == "") {
//             //     videoPlayer.VideoId = trackingIdToVideoURL[goi.info.TrackingId];
//             //     videoPlayer.PlayVideoAsync();
//             // } else if (trackingIdToVideoURL[goi.info.TrackingId] != videoPlayer.VideoId) {
//             //     videoPlayer.VideoPlayer.Pause();
//             //     videoPlayer.VideoId = trackingIdToVideoURL[goi.info.TrackingId];
//             //     videoPlayer.PlayVideoAsync(); // videoPlayer.PlayVideoAsync();
//             // }