using UnityEngine;

public class DrawSpringBetweenWheels : MonoBehaviour
{
    public Transform rearWheel; // Reference to the rear wheel
    public Transform frontWheel; // Reference to the front wheel
    private LineRenderer lineRenderer;

    public int springSegments = 20; // Number of segments for the spring
    public float springAmplitude = 0.3f; // Height of the spring wave

    void Start()
    {
        // Get or add LineRenderer component
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        // LineRenderer settings
        lineRenderer.positionCount = springSegments + 2; // Total points for the spring
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Default material
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.blue;
    }

    void FixedUpdate()
    {
        // Update spring shape between wheels
        DrawSpring();
    }

    private void DrawSpring()
    {
        Vector3 start = rearWheel.position; // Start at rear wheel
        Vector3 end = frontWheel.position; // End at front wheel

        // Calculate the direction and distance between wheels
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);

        // Calculate orthogonal direction for spring zigzag
        Vector3 orthogonal = Vector3.Cross(direction, Vector3.forward).normalized;

        // Set positions for the spring
        for (int i = 0; i <= springSegments + 1; i++)
        {
            float t = (float)i / (springSegments + 1); // Normalize [0, 1]
            Vector3 position = Vector3.Lerp(start, end, t); // Linear interpolation

            // Add sine wave for spring effect
            if (i > 0 && i < springSegments + 1) // Skip start and end points
                position += orthogonal * Mathf.Sin(t * Mathf.PI * springSegments) * springAmplitude;           

            lineRenderer.SetPosition(i, position); // Set point position
        }
    }
}
