using UnityEngine;

public class CompassController : MonoBehaviour
{
    public Transform theCamera;
    private Vector3 vector;

    // Update is called once per frame
    void Update() {
        vector.z = theCamera.eulerAngles.y;
        transform.localEulerAngles = vector;
    }
}
