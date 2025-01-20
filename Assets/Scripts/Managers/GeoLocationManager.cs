using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class GeoLocationManager : MonoBehaviour
{
    public const float FENCE_WIDTH = 5f;
    // PREFABS //
    public GameObject locationTextPrefab;
    public GameObject metalFencePrefab;
    public GameObject hedgePrefab;
    public GameObject brickwallPrefab;
    public Transform fenceContainer;

    // OTHER //
    public Slider lineThicknessSlider;
    public Material geoFenceMaterial;

    public static Dictionary<LineRenderer, (string, float)> geoFenceColorValues;

    private Vector4 tmpProTopMargin = new Vector4(0f, 6f, 0f, 0f);
    private Vector4 tmpProBottomMargin = new Vector4(0f, 0f, 0f, 6f);
    
    void Start() {
        geoFenceColorValues = new Dictionary<LineRenderer, (string, float)>();
    }

    public void InstantiateGeoLocationObject(GeoPosition gpos, Vector3 position) {
        GameObject locationObject = Instantiate(locationTextPrefab, position, Quaternion.identity, 
                                                RefManager.Get<GeoPositionManager>().geoObjectContainer);
        GeoObjectInstance goi = locationObject.AddComponent<GeoObjectInstance>();
        goi.info = new GeoObjectInfo(gpos);
        locationObject.name = goi.info.DisplayName;
        TMP_Text textObject = locationObject.GetComponentInChildren<TMP_Text>();
        textObject.text = gpos.DisplayName;
        GeoPositionManager.AddToGeoPositionDataStructures(gpos.Id.ToString(), gpos, locationObject, goi);
        StartCoroutine(FetchGeoFenceIfAny(gpos, goi, textObject));
    }

    private IEnumerator FetchGeoFenceIfAny(GeoPosition gpos, GeoObjectInstance goi, TMP_Text textObject) {
        List<dynamic> geoFences;
        using (UnityWebRequest request = UnityWebRequest.Get(APIConsts.URL_LIST_OBJECT_GEOFENCES + gpos.Id)) {
            request.SetRequestHeader("Api-key", GeoPositionManager.currentAPIKey);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError)
                Debug.Log(request.error);
            geoFences = JsonConvert.DeserializeObject<List<dynamic>>(request.downloadHandler.text);            
        }

        if (geoFences == null) 
            yield break;  

        foreach (var geoFence in geoFences) {
            dynamic geoFenceData;
            using (UnityWebRequest request = UnityWebRequest.Get(APIConsts.URL_GET_GEOFENCE_DEFINITION + geoFence.id)) {
                request.SetRequestHeader("Api-key", GeoPositionManager.currentAPIKey);
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.ConnectionError)
                    Debug.Log(request.error);
                geoFenceData = JsonConvert.DeserializeObject<dynamic>(request.downloadHandler.text);
            }

            if (geoFenceData != null) {
                if (gpos.GeoModelCode == SublocationGeoCode.FENCE_AREA)
                    InstantiateFenceObject(metalFencePrefab, goi, geoFence, geoFenceData, textObject);
                else if (gpos.GeoModelCode == SublocationGeoCode.HEDGE_AREA)
                    InstantiateFenceObject(hedgePrefab, goi, geoFence, geoFenceData, textObject);
                else if (gpos.GeoModelCode == SublocationGeoCode.BRICKWALL_AREA)
                    InstantiateFenceObject(brickwallPrefab, goi, geoFence, geoFenceData, textObject);
                else
                    InstantiateGeoFenceObject(goi, geoFence, geoFenceData, textObject);
            }      
        }
    }

    private void InstantiateFenceObject(GameObject fencePrefab, GeoObjectInstance goi, dynamic fenceDefiniton, dynamic fenceData, TMP_Text textObject) {
        GameObject fenceRootObject = new GameObject("Fence_" + fenceDefiniton.id);
        fenceRootObject.transform.parent = goi.transform;

        List<Vector3> positions = new List<Vector3>();
        foreach (var point in fenceData.geoFencePositions) {
            Vector3 position = RefManager.Get<MapController>().LatLongToUnityPosition((double)point.latitude, (double)point.longitude);
            position.y = goi.transform.position.y + 1f;
            positions.Add(position);
        }

        if (positions.Count < 2)
            return;

        FindOptimalLabelPositionAndRotation(positions, textObject);

        for (int i = 2; i < positions.Count; i++) { // We start at j == 2 to ignore one of the sides of the "geoFence"
            Vector3 startPos = positions[i - 1];
            Vector3 endPos = positions[i];
            
            float distance = Vector3.Distance(startPos, endPos);
            int fenceCount = (int)(distance / FENCE_WIDTH);
            float fenceRemainder = (distance / FENCE_WIDTH) - (float)fenceCount;

            for (int j = 0; j <= fenceCount; j++) {
                Vector3 direction = (endPos - startPos).normalized;
                Vector3 posDiff = new Vector3(j * FENCE_WIDTH * direction.x, 0f, j * FENCE_WIDTH * direction.z);
                var fenceG0 = Instantiate(fencePrefab, startPos + posDiff, Quaternion.LookRotation(direction), fenceContainer);
                fenceG0.AddComponent<GeoObjectInstance>();
                
                if (j == fenceCount) {
                    float scaleFactor = fenceRemainder / FENCE_WIDTH;
                    fenceG0.transform.localScale = new Vector3(1f, 1f, scaleFactor * 5f);
                }
            }
        }
    }

    private void InstantiateGeoFenceObject(GeoObjectInstance goi, dynamic geoFence, dynamic geoFenceData, TMP_Text textObject) {
        GameObject geoFenceRootObject = new GameObject("GeoFence_" + geoFence.id);
        geoFenceRootObject.transform.parent = goi.transform;

        List<Vector3> positions = new List<Vector3>();
        foreach (var point in geoFenceData.geoFencePositions) {
            Vector3 position = RefManager.Get<MapController>().LatLongToUnityPosition((double)point.latitude, (double)point.longitude);
            position.y = goi.transform.position.y + 1f;
            positions.Add(position);
        }

        if (positions.Count < 2)
            return;

        FindOptimalLabelPositionAndRotation(positions, textObject);

        LineRenderer lineRenderer = geoFenceRootObject.AddComponent<LineRenderer>();
        Color c = ResolveColor((string)geoFenceData.color, (float)geoFenceData.opacity);
        lineRenderer.material = new Material(geoFenceMaterial) { //new Material(Shader.Find("Sprites/Default"));
            color = c
        };
        lineRenderer.startColor = lineRenderer.endColor = c;
        lineRenderer.widthMultiplier = lineThicknessSlider.value;
        lineRenderer.useWorldSpace = false;
        // lineRenderer.loop = true;
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
        lineRenderer.numCornerVertices = 5;
        geoFenceColorValues.Add(lineRenderer, ((string)geoFenceData.color, (float)geoFenceData.opacity));
    }

    private void FindOptimalLabelPositionAndRotation(List<Vector3> positions, TMP_Text textObject) {
        if (positions.Count < 2)
            return;

        float maxDistance = Vector3.Distance(positions[0], positions[1]);
        Vector3 maxStart = positions[0]; 
        Vector3 maxEnd = positions[1];

        for (int i = 2; i < positions.Count; i++) {
            if (Vector3.Distance(positions[i - 1], positions[i]) > maxDistance) {
                maxDistance = Vector3.Distance(positions[i - 1], positions[i]);
                maxStart = positions[i - 1];
                maxEnd = positions[i];
            }
        }
        float midPoint = maxDistance / 2f;
        textObject.rectTransform.sizeDelta = new Vector2(maxDistance * 0.8f, 8f); //textObject.rectTransform.sizeDelta.y
        
        if (maxStart.z > maxEnd.z) {
            (maxEnd, maxStart) = (maxStart, maxEnd); // Swap to make the max point the further to the north
            textObject.margin = tmpProTopMargin;
        } else {
            textObject.margin = tmpProBottomMargin;
        }
            

        Vector3 direction = (maxEnd - maxStart).normalized;
        Vector3 lr = Quaternion.LookRotation(direction).eulerAngles;
        textObject.transform.SetPositionAndRotation(maxStart + (midPoint * direction), Quaternion.Euler(new Vector3(lr.x + 90f, lr.y - 90f, lr.z)));
    }

    private Color ResolveColor(string color, float alpha) {
        float r = ((float)Convert.ToInt32(color.Substring(1, 2), 16)) / 255f;
        float g = ((float)Convert.ToInt32(color.Substring(3, 2), 16)) / 255f;
        float b = ((float)Convert.ToInt32(color.Substring(5, 2), 16)) / 255f;
        return new Color(r, g, b, alpha);
    }

    public void OnToggleGeoFencesEnabled(Toggle toggle) { 
        foreach(var geoFenceLineRenderer in geoFenceColorValues.Keys) {
            geoFenceLineRenderer.gameObject.SetActive(toggle.isOn);
        }
    }

    public void OnToggleUseGeoFenceTransparency(Toggle toggle) { 
        foreach(var (geoFenceLineRenderer, colorInfo) in geoFenceColorValues) {
            float alpha = toggle.isOn ? colorInfo.Item2 : 1f;
            geoFenceLineRenderer.material.color = ResolveColor(colorInfo.Item1, alpha);                 
        }
    }

    public void OnLineWidthSettingChanged(Slider slider) {
        foreach(var geoFenceLineRenderer in geoFenceColorValues.Keys) {
            geoFenceLineRenderer.startWidth = slider.value;
            geoFenceLineRenderer.endWidth = slider.value;
            geoFenceLineRenderer.widthMultiplier = slider.value;
        }
    }
}

// Color c1 = Color.yellow;
// Color c2 = Color.red;
// A simple 2 color gradient with a fixed alpha of 1.0f.
// float alpha = 1.0f;
// Gradient gradient = new Gradient();
// gradient.SetKeys(
//     new GradientColorKey[] { new GradientColorKey(c1, 0.0f), new GradientColorKey(c2, 1.0f) },
//     new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
// );
// lineRenderer.colorGradient = gradient;
