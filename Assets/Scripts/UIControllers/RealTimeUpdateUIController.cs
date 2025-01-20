using System;
using System.Collections;
using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.UI;

public class RealTimeUpdateUIController : MonoBehaviour
{
    public TextMeshProUGUI lastUpdateText;
    private DateTime lastUpdateTime; 
    private bool liveUpdateReceived = false;
    public ScrollRect logScrollView;
    public static ScrollRect logScrollViewInstance;
    public GameObject logMessageTemplate;
    public static RealTimeUpdateUIController rtcInstance;

    void Awake() {
        RealTimePositionManager.RealTimePositionReceived += OnRealTimePositionReceived;
        MapController.MapLocationChanged += OnMapLocationChanged;
        logScrollViewInstance = logScrollView;
        rtcInstance = this;
        lastUpdateTime = DateTime.Now;
        StartCoroutine(DisplayLastUpdateTime());
    }

    IEnumerator DisplayLastUpdateTime() {
        while (true) {
            while (!liveUpdateReceived)
            yield return new WaitForSeconds(1f);
        
            while (liveUpdateReceived) {
                lastUpdateText.text = PrintTimeSpan(DateTime.Now - lastUpdateTime);
                yield return new WaitForSeconds(1f);
            }
        } 
    }

    public static string PrintTimeSpan(TimeSpan span) {
        if (span == TimeSpan.Zero) 
            return "None";

        var sb = new StringBuilder();
        if (span.Days > 0)
            sb.AppendFormat(">{0} day{1} ago", span.Days, span.Days > 1 ? "s" : String.Empty);
        else if (span.Hours > 0)
            sb.AppendFormat(">{0} hour{1} ago", span.Hours, span.Hours > 1 ? "s" : String.Empty);
        else if (span.Minutes > 0)
            sb.AppendFormat(">{0} minute{1} ago", span.Minutes, span.Minutes > 1 ? "s" : String.Empty);
        else if (span.Seconds >= 0)
            sb.AppendFormat("{0} second{1} ago", span.Seconds, span.Seconds > 1 ? "s" : String.Empty);
        return sb.ToString();
    }

    public void OnRealTimePositionReceived() {
        lastUpdateTime = DateTime.Now;
        liveUpdateReceived = true;
    }

    public void OnMapLocationChanged() {
        liveUpdateReceived = false;
        lastUpdateText.text = "None";
    }

    void OnDestroy() {
        RealTimePositionManager.RealTimePositionReceived -= OnRealTimePositionReceived;
        MapController.MapLocationChanged -= OnMapLocationChanged;
    }

    public void PrintLogMessage(string message) {
        #if !UNITY_EDITOR
        Debug.Log(message);
        #endif
        GameObject logMessage = Instantiate(logMessageTemplate);
        TextMeshProUGUI tmproText = logMessage.GetComponent<TextMeshProUGUI>();
        tmproText.text = message;
        float breath = logScrollViewInstance.content.sizeDelta.x;
        float height = logScrollViewInstance.content.sizeDelta.y;
        logScrollViewInstance.content.sizeDelta = new Vector2(breath, height + 12.1765f);
        logMessage.transform.SetParent(logScrollViewInstance.content);
        logMessage.SetActive(true);
    }

    public void ClearLog() {
        foreach (Transform t in logScrollViewInstance.content)
            Destroy(t.gameObject);
        logScrollViewInstance.content.sizeDelta = new Vector2(-250f, 12.1765f);
    }
}
