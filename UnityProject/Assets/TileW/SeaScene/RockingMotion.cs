using System;
using System.Numerics;

using UnityEngine;

public class RockingMotion : MonoBehaviour
{
    // Public variables to control the maximum oscillation and frequency
    public float maxOscillationAngle = 10.0f; // in degrees
    public float frequency = 1.0f; // in Hz, range 0.1 to 2 Hz

    // Private variable to store the original rotation
    private UnityEngine.Quaternion originalRotation;

    void Start()
    {
        // Store the original rotation of the object
        originalRotation = transform.rotation;
    }

    void Update()
    {
        // Clamp the frequency between 0.1 and 2 Hz
        frequency = Mathf.Clamp(frequency, 0.1f, 2.0f);

        // Calculate the angle of oscillation for this frame
        float angle = maxOscillationAngle * Mathf.Sin(2 * Mathf.PI * frequency * Time.time);

        // Apply the oscillation around the Z-axis
        transform.rotation = originalRotation * UnityEngine.Quaternion.Euler(0, 0, angle);
    }
}
