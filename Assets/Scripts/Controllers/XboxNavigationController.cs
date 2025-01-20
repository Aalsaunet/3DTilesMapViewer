using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class XboxNavigationController : MonoBehaviour
{
    public GameObject locationMenuPanel;
    public GameObject mapStyleMenuPanel;
    public GameObject cameraSpeedPanel;
    public GameObject[] mapStyleButtons;
    public LocationController locCtrler;
    public MapController mapController;
    public RectTransform controllerCursor;

    private CameraController camCtrler;
    private GameObject[] locationButtons;
    private int currentLocationButtonIndex = 0;
    private int currentMapStyleButtonIndex = 0;
    private bool dpadVerticalBlocked = false;
    private bool cursorModeActive = false;

    enum ActivePanel { None, Location, MapStyle } 
    private ActivePanel currentPanel = ActivePanel.None;
    private TextMeshProUGUI cameraSpeedText;

    void Start() {
        camCtrler = GetComponent<CameraController>();
        cameraSpeedText = cameraSpeedPanel.GetComponentInChildren<TextMeshProUGUI>();

        List<GameObject> locButtonList = new List<GameObject>();
        foreach (Transform tf in locCtrler.bigLocationScrollView.content) {
            locButtonList.Add(tf.gameObject);
        }

        locationButtons = locButtonList.ToArray();
        locationButtons[currentLocationButtonIndex].GetComponent<Button>().interactable = true;
        mapStyleButtons[currentMapStyleButtonIndex].GetComponent<Button>().interactable = true;
    }

    void LateUpdate() {
        if (camCtrler.isCameraLocked)
            return;
        
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            HandleWindowsSpecificInput();
        else if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
            HandleMacOSSpecificInput();

        if (Input.GetButtonDown("XboxButtonA")) {
            if (cursorModeActive) {
                bool gotUIHit = camCtrler.RaycastToScreenPoint(controllerCursor.anchoredPosition);
                if (!gotUIHit)
                    camCtrler.RaycastFromScreenPoint(controllerCursor.position);
            } else if (mapStyleMenuPanel.activeSelf) {
                mapStyleButtons[currentMapStyleButtonIndex].GetComponent<Button>().onClick.Invoke();
                CloseMenuPanel(mapStyleMenuPanel, mapStyleButtons, ref currentMapStyleButtonIndex);
            } else if (locationMenuPanel.activeSelf) {
                locationButtons[currentLocationButtonIndex].GetComponent<Button>().onClick.Invoke();
                CloseMenuPanel(locationMenuPanel, locationButtons, ref currentLocationButtonIndex);
            }
            else {
                ActivateMenuPanel(locationMenuPanel, ActivePanel.Location);
            }
        }

        if (Input.GetButtonDown("XboxButtonY")) {
            if (!mapStyleMenuPanel.activeSelf) {
                if (locationMenuPanel.activeSelf)
                   CloseMenuPanel(locationMenuPanel, locationButtons, ref currentLocationButtonIndex);
                ActivateMenuPanel(mapStyleMenuPanel, ActivePanel.MapStyle); 
            } else {
                CloseMenuPanel(mapStyleMenuPanel, mapStyleButtons, ref currentMapStyleButtonIndex);
            }
        }

        if (Input.GetButtonDown("XboxButtonB")) {
            if (mapStyleMenuPanel.activeSelf) {
                CloseMenuPanel(mapStyleMenuPanel, mapStyleButtons, ref currentMapStyleButtonIndex);
            } else if (locationMenuPanel.activeSelf) {
                CloseMenuPanel(locationMenuPanel, locationButtons, ref currentLocationButtonIndex);
            }
        }

        if (Input.GetButtonDown("XboxButtonX")) {
            cursorModeActive = !cursorModeActive;
            controllerCursor.gameObject.SetActive(cursorModeActive);
        }
            

        if (Input.GetAxis("XboxDpadY") != 0.0f && !dpadVerticalBlocked) {
            dpadVerticalBlocked = true;
            if (Input.GetAxis("XboxDpadY") > 0.0f) {
                if (currentPanel == ActivePanel.Location)
                    SelectPreviousMenuButton(locationButtons, ref currentLocationButtonIndex);
                else if (currentPanel == ActivePanel.MapStyle)
                    SelectPreviousMenuButton(mapStyleButtons, ref currentMapStyleButtonIndex);
            }
            else {
                if (currentPanel == ActivePanel.Location)
                    SelectNextMenuButton(locationButtons, ref currentLocationButtonIndex);
                else if (currentPanel == ActivePanel.MapStyle)
                    SelectNextMenuButton(mapStyleButtons, ref currentMapStyleButtonIndex);
            }
                
        }
        else if (Input.GetAxis("XboxDpadY") == 0.0f)
            dpadVerticalBlocked = false;
    }

    private void HandleWindowsSpecificInput() {
        float speed = camCtrler.currentSpeed * camCtrler.speedMultiplier;

        // Navigation
        if (!cursorModeActive) {
            // Move the camera forward, backward, left and right          
            transform.position += transform.right * Input.GetAxis("XboxLeftStickXWindows") * speed * Time.deltaTime;
            transform.position += transform.forward * Input.GetAxis("XboxLeftStickYWindows") * speed * Time.deltaTime;
        } else {
            float leftDiff = Input.GetAxis("XboxLeftStickXWindows") * Time.deltaTime * 1000f;
            float upDiff = Input.GetAxis("XboxLeftStickYWindows") * Time.deltaTime * 1000f;
            controllerCursor.anchoredPosition += new Vector2(leftDiff, upDiff);
        }
        

        // Move camera up and down by axis
        transform.position += -transform.up * Input.GetAxis("XboxLeftTriggerWindows") * speed * Time.deltaTime;
        transform.position += transform.up * Input.GetAxis("XboxRightTriggerWindows") * speed * Time.deltaTime;

        // Rotate camera horizontally and vertically
        transform.rotation = !camCtrler.isAttachedToFPVP ? camCtrler.HandleCameraRotation(Input.GetAxis("XboxRightStickXWindows"), Input.GetAxis("XboxRightStickYWindows")) : transform.rotation;

        // Menu and location
        if (Input.GetButton("XboxBumperLeftWindows"))
            ModifyCameraSpeed(-1f);
        if (Input.GetButton("XboxBumperRightWindows"))
            ModifyCameraSpeed(1f);
    }

    private void HandleMacOSSpecificInput() {
        // Navigation
        // Move the camera forward, backward, left and right
        float speed = camCtrler.currentSpeed * camCtrler.speedMultiplier;
        transform.position += transform.right * Input.GetAxis("XboxLeftStickXMac") * speed * Time.deltaTime;
        transform.position += transform.forward * Input.GetAxis("XboxLeftStickYMac") * speed * Time.deltaTime;

        // Move camera up and down by axis
        transform.position += -transform.up * Input.GetAxis("XboxLeftTriggerMac") * speed * Time.deltaTime;
        transform.position += transform.up * Input.GetAxis("XboxRightTriggerMac") * speed * Time.deltaTime;

        // Rotate camera horizontally and vertically
        transform.rotation = camCtrler.HandleCameraRotation(Input.GetAxis("XboxRightStickXMac"), Input.GetAxis("XboxRightStickYMac"));

        // Menu and location
        if (Input.GetButton("XboxBumperLeftMac"))
            ModifyCameraSpeed(-1f); // pdlc.OnPreviousDestinationClick();
        if (Input.GetButton("XboxBumperRightMac")) 
            ModifyCameraSpeed(1f); // pdlc.OnNextDestinationClick(); 
    }

    private void ActivateMenuPanel(GameObject panel, ActivePanel panelType) {
        panel.SetActive(true);
        currentPanel = panelType;
    }

    private void CloseMenuPanel(GameObject panel, GameObject[] buttons, ref int index) {
        buttons[index].GetComponent<Button>().interactable = false;
        index = 0;
        buttons[index].GetComponent<Button>().interactable = true;
        panel.SetActive(false);
        currentPanel = ActivePanel.None;
    }

    private void SelectNextMenuButton(GameObject[] buttons, ref int index) {
        buttons[index].GetComponent<Button>().interactable = false;
        index = (index + 1) % buttons.Length;
        buttons[index].GetComponent<Button>().interactable = true;
    }

    private void SelectPreviousMenuButton(GameObject[] buttons, ref int index) {
        buttons[index].GetComponent<Button>().interactable = false;
        index = index == 0 ? buttons.Length - 1 : index - 1;
        buttons[index].GetComponent<Button>().interactable = true;
    }


    public void ModifyCameraSpeed(float value) { // = camCtrler.currentSpeed + 1f; //(float)Math.Pow(camCtrler.currentSpeed + 0.1, 2.0);
        if ((value < 0 && camCtrler.currentSpeed <= 1) || (value > 0 && camCtrler.currentSpeed == float.MaxValue))
            return;
        
        camCtrler.currentSpeed += value; 
        cameraSpeedText.text = "Camera speed: " + camCtrler.currentSpeed + "x";
        
        if (!cameraSpeedPanel.activeSelf) {
            cameraSpeedPanel.SetActive(true);
            Waiter.Wait(1, () => {
                cameraSpeedPanel.SetActive(false);
            });
        }  
    }
}
