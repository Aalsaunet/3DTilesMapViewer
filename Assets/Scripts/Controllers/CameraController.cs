using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CameraController : MonoBehaviour {
    public const float CAMERA_LERP_DURATION = 1f;
    public static float cameraYLimit = 45f;
    public Camera mainCamera;

    // MOUSE ROTATION SETTINGS //
    // Unity KeyCodes for mouse
    // Mouse0	The Left (or primary) mouse button.
    // Mouse1	Right mouse button (or secondary mouse button).
    // Mouse2	Middle mouse button (or third button).
    // Mouse3	Additional (fourth) mouse button.
    // Mouse4	Additional (fifth) mouse button.
    // Mouse5	Additional (or sixth) mouse button.
    // Mouse6	Additional (or seventh) mouse button.
    private float rotX = 0.0f; // rotation around the right/x axis
    private float rotY = 0.0f; // rotation around the up/y axis
    private float xAccumulator; // Used for smoothing the mouse movement
    private float yAccumulator; // Used for smoothing the mouse movement
    private float mouseSnappiness = 10.0f; // larger values of this cause less filtering, more responsiveness
    private float mouseSensitivity = 10.0f; //1000.0f;
    private float clampAngle = 60.0f;
    
    // WASD MOVEMENT SETTINGS //
    public float speedMultiplier = 10.0f; //100.0f;
    public float currentSpeed = 10.0f;
    
    // VARIABLES ABOUT ENTERING, TRANSITIONING AND EXITING FPVP //
    private GameObject currentHighlightedObject;
    private Vector3 initialCameraPosition;
    private RaycastHit raycastHit;
    [HideInInspector] public bool isAttachedToFPVP;
    private GameObject targetFPVP;
    private Vector3 lastNonFPVPPosition;
    private Quaternion lastNonFPVPRotation;
    public bool isCameraLocked = false;
    private BrowseGeoObjectsUIController browseController;

    void Start() {
        initialCameraPosition = transform.position;
        isAttachedToFPVP = false;
        browseController = GetComponent<BrowseGeoObjectsUIController>();
        SetXYRotation();
    }

    private void SetXYRotation() {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;
    }

    private void ResetXYRotation() {
        rotY = 0f;
        rotX = 0f;
    }

    void LateUpdate() {
        if (isCameraLocked)
            return;

        // Move the camera forward, backward, left and right
        float speed = currentSpeed * speedMultiplier;
        transform.position += transform.forward * Input.GetAxis("Vertical") * speed * Time.deltaTime;
        transform.position += transform.right * Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        transform.position += transform.forward * Input.mouseScrollDelta.y * speed * Time.deltaTime;

        // Move camera up and down by buttons
        if (Input.GetKey(KeyCode.Q))
            transform.position += -transform.up * speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E))
            transform.position += transform.up * speed * Time.deltaTime;
        
        // Speed up
        if (Input.GetKey(KeyCode.LeftShift))
            transform.position += transform.forward * Input.GetAxis("Vertical") * speed * Time.deltaTime;  

        // Prevent going under the ground (a very naive implementation thus far)
        if (transform.position.y < cameraYLimit) 
            transform.position = new Vector3(transform.position.x, cameraYLimit, transform.position.z);

        // Rotate the camera based on the mouse movement
        if (Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.Mouse2))
            transform.rotation = HandleCameraRotation(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y")); 

        // Handle FPVP position and rotation if applicable
        if (isAttachedToFPVP) {
            transform.position = targetFPVP.transform.position;
            if (isAttachedToFPVP) {
                Vector3 targetRot = targetFPVP.transform.rotation.eulerAngles;
                transform.rotation = Quaternion.Euler(targetRot.x + rotX, targetRot.y + rotY, 0.0f);
            }
        }

        // Raycast to highligh objects and go into first person mode
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            RaycastFromScreenPoint(Input.mousePosition);
        }       
        
        // if (Vector3.Distance(transform.position, initialCameraPosition) > GeoConsts.GEO_POSITION_RADIUS) {
        //     RefManager.Get<DemoAssetsController>().DeactivateAll();
        //     var (latitude, longitude) = RefManager.Get<MapController>().UnityPositionToLatLong(transform.position);
        //     RefManager.Get<MapController>().GoToLocation(latitude, longitude, GeoPositionManager.currentAPIKey, GeoPositionManager.currentHubName);
        // }
	}

    public bool RaycastToScreenPoint(Vector2 screenPoint) { 
        EventSystem eventSystem = FindObjectOfType<EventSystem>();
        PointerEventData pointerEventData = new PointerEventData(eventSystem) {
            position = screenPoint
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);
        foreach (RaycastResult result in results) {
            var uiElement = result.gameObject.GetComponent<Selectable>();
            if (uiElement != null) {
                uiElement.Select();
                // uiElement.OnPointerClick(pointerEventData);
                return true;
            }
        }

        return false;
    }

    public void RaycastFromScreenPoint(Vector3 origin) {
        if(IsPointerOverUIElement(GetEventSystemRaycastResults()))
            return; // Prevent any action when the user simply intends to click a UI element

        Ray ray = mainCamera.ScreenPointToRay(origin);

        if (Physics.Raycast(ray, out raycastHit)) {
            var targetGameObject = raycastHit.collider.gameObject;
            if (targetGameObject.TryGetComponent<Button>(out var button)) {
                Debug.Log("Button hit!");
            } else if (targetGameObject.TryGetComponent<GeoObjectInstance>(out var goi)) {
                browseController.OpenGeoObjectInfoPanel(goi);

                if (isAttachedToFPVP) {
                    DisableFPVP();
                    return;
                } else if (targetGameObject == currentHighlightedObject) {
                    EnableFPVP(targetGameObject);
                    return;
                } else if (currentHighlightedObject != null) {
                    Destroy(currentHighlightedObject.GetComponent<Outline>());
                }

                targetGameObject.AddComponent<Outline>();
                currentHighlightedObject = targetGameObject;

            } else {
                if (currentHighlightedObject != null) {
                    Destroy(currentHighlightedObject.GetComponent<Outline>());
                    currentHighlightedObject = null;
                }

                if (isAttachedToFPVP) {
                    DisableFPVP();
                }
            }
        }  else {
            if (currentHighlightedObject != null) {
                Destroy(currentHighlightedObject.GetComponent<Outline>());
                currentHighlightedObject = null;
            }

            if (isAttachedToFPVP)
            {
                DisableFPVP();
            }
        }
    }

    public void EnableFPVP(GameObject targetGameObject) {
        foreach (Transform child in targetGameObject.transform) {
            if (child.name.Equals("FPVP")) {
                // Root object has a First Person Viewport object
                lastNonFPVPPosition = transform.position;
                lastNonFPVPRotation = transform.rotation;
                targetFPVP = child.gameObject;
                StartCoroutine(LerpCamera(child.position, targetGameObject.transform.rotation, CAMERA_LERP_DURATION, true));

                if (currentHighlightedObject != null) {
                    Destroy(currentHighlightedObject.GetComponent<Outline>());
                    currentHighlightedObject = null;
                }

                break;
            }
        } 
    }

    private void DisableFPVP() {  
        StartCoroutine(LerpCamera(lastNonFPVPPosition, lastNonFPVPRotation, CAMERA_LERP_DURATION, false));
    }

    IEnumerator LerpCamera(Vector3 targetPosition, Quaternion targetRotation, float lerpSeconds, bool isAttachingToFPVP) {
        isCameraLocked = true;     
        float elapsedTime = 0;
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        
        while (elapsedTime <= lerpSeconds) {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / lerpSeconds);
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / lerpSeconds);
            elapsedTime += Time.deltaTime;
            yield return null; //new WaitForEndOfFrame();
        }
        transform.position = targetPosition;
        transform.rotation = targetRotation;
        
        if (isAttachingToFPVP)
            ResetXYRotation();
        else
            SetXYRotation();
        
        isAttachedToFPVP = isAttachingToFPVP;
        isCameraLocked = false; 
    }

    public void MoveToGeoObject(GameObject targetGameObject) {
        
        Vector3 newPosition;
        Vector3 offset = new Vector3(0f, 150f, -200f);
        
        // Only move to a new position if the new position is closer to the target GameObject
        if (Vector3.Distance(targetGameObject.transform.position, transform.position) < Vector3.Distance(targetGameObject.transform.position, targetGameObject.transform.position - offset))
            newPosition = transform.position;
        else
            newPosition = targetGameObject.transform.position + offset;
        
        Vector3 relativePos = targetGameObject.transform.position - newPosition;
        Quaternion newRotation = Quaternion.LookRotation(relativePos, Vector3.up);
        StartCoroutine(LerpCamera(newPosition, newRotation, CAMERA_LERP_DURATION, false));

        if (currentHighlightedObject != targetGameObject) {
            if (currentHighlightedObject != null)
                Destroy(currentHighlightedObject.GetComponent<Outline>());
            targetGameObject.AddComponent<Outline>();
            currentHighlightedObject = targetGameObject;
        }  
    }

    public Quaternion HandleCameraRotation(float mouseX, float mouseY) {
        xAccumulator = Mathf.Lerp(xAccumulator, mouseX * mouseSensitivity, mouseSnappiness * Time.deltaTime);
        yAccumulator = Mathf.Lerp(yAccumulator, mouseY * mouseSensitivity, mouseSnappiness * Time.deltaTime);
        rotX += yAccumulator; // rotY += mouseX * mouseSensitivity * Time.deltaTime;
        rotY += xAccumulator; // rotX += mouseY * mouseSensitivity * Time.deltaTime;
        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);
        return Quaternion.Euler(rotX, rotY, 0.0f);
    }

    ///Returns 'true' if we touched or hovering on Unity UI element.
    public static bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaycastResults)  {
        for (int index = 0; index < eventSystemRaycastResults.Count; index ++) {
            RaycastResult curRaysastResult = eventSystemRaycastResults [index];
            if (curRaysastResult.gameObject.layer == LayerMask.NameToLayer("UI"))
                return true;
        }
        return false;
    }

    ///Gets all event systen raycast results of current mouse or touch position.
    static List<RaycastResult> GetEventSystemRaycastResults() {   
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position =  Input.mousePosition;
        List<RaycastResult> raysastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll( eventData, raysastResults );
        return raysastResults;
    }

    public void OnCameraMoveSpeedChanged(Slider slider) {
        currentSpeed = (float)Math.Pow((double)slider.value, 2.0);
    }

    public void UpdateSpeedValueText(TextMeshProUGUI text) {
        text.text = currentSpeed + "x";
    }

    public void ResetCameraPosition(bool excludeHeight = false) {
        transform.position = excludeHeight ? new Vector3(initialCameraPosition.x, transform.position.y, initialCameraPosition.z)
                                           : initialCameraPosition;
    }

    public void SetCameraLocked(bool lockCamera) {
        isCameraLocked = lockCamera;
    }
}
