using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SearchController : MonoBehaviour
{
    public TMP_InputField inputField;
    public ScrollRect searchResultScrollRect;
    public GameObject searchResultPrefab;
    private CameraController camCtrler;
    private BrowseGeoObjectsUIController browseController;

    void Awake() {
        camCtrler = GetComponent<CameraController>();
        browseController = GetComponent<BrowseGeoObjectsUIController>();
        MapController.MapLocationChanged += OnMapPostionUpdated;
    }

    public void OnSearchInputChanged(TMP_InputField textField) {
        foreach (Transform c in searchResultScrollRect.content.transform)
            Destroy(c.gameObject);
        searchResultScrollRect.content.sizeDelta = new Vector2(searchResultScrollRect.content.sizeDelta.x, 0f);

        string searchText = textField.text.ToLower().Trim();
        if (searchText.Length == 0) {
            searchResultScrollRect.gameObject.SetActive(false);
            return;
        }
        
        searchResultScrollRect.gameObject.SetActive(true);    

        List<GeoObjectInstance> nameResult = GeoPositionManager.geoObjectNameTrie.GetMatchingPrefixes(searchText);
        List<GeoObjectInstance> idResult = GeoPositionManager.trackingIdTrie.GetMatchingPrefixes(searchText);


        foreach (GeoObjectInstance goi in nameResult.Union(idResult)) {
            GameObject searchResult = Instantiate(searchResultPrefab);
            searchResult.GetComponent<Button>().onClick.AddListener(() => OnSearchResultClicked(goi));
            searchResult.transform.Find("NameField").GetComponent<TextMeshProUGUI>().text = goi.info.DisplayName;
            searchResult.transform.Find("InfoField").GetComponent<TextMeshProUGUI>().text = goi.info.TrackingId;
            float breath = searchResultScrollRect.content.sizeDelta.x;
            float height = searchResultScrollRect.content.sizeDelta.y;
            searchResultScrollRect.content.sizeDelta = new Vector2(breath, height + 24.5f);
            searchResult.transform.SetParent(searchResultScrollRect.content);
            searchResult.SetActive(true);
        }
    }

    public void OnSearchResultClicked(GeoObjectInstance goi) {
        // camCtrler.MoveToGeoObject(goi.gameObject);
        browseController.OpenGeoObjectInfoPanel(goi);
    }

    public void OnMapPostionUpdated() {
        inputField.text = "";
        searchResultScrollRect.gameObject.SetActive(false);
    }

    void OnDestroy() {
        MapController.MapLocationChanged += OnMapPostionUpdated;
    }
}
