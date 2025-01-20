using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconRotation : MonoBehaviour
{
    // Rotation speed in degrees per second
    public float rotationSpeed = 45f;

    // Flag to control the rotation
    private bool isRotating = true;

    void Start()
    {
        // Start the rotation coroutine
        StartCoroutine(RotateObject());
    }

    // Coroutine to rotate the object around the Y-axis
    private IEnumerator RotateObject()
    {
        // Infinite loop, you can add a condition to stop it if needed
        while (true)
        {
            // Rotate the object around the Y-axis (using Time.deltaTime for smooth frame-rate independent rotation)
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f);

            // Wait until the next frame
            yield return null;
        }
    }

    // Optional: Method to start and stop rotation
    public void ToggleRotation()
    {
        if (isRotating)
        {
            StopCoroutine(RotateObject());
        }
        else
        {
            StartCoroutine(RotateObject());
        }
        isRotating = !isRotating;
    }
}
