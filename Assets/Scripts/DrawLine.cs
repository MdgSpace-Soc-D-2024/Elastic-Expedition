using UnityEngine;

public class DrawSpringBetweenWheels : MonoBehaviour
{
    public Transform rearWheel; // Reference to the rear wheel
    public Transform frontWheel; // Reference to the front wheel
    public Transform rearPivot; // Reference to the rear pivot
    public Transform frontPivot; // Reference to the front pivot

    private LineRenderer lineRenderer; // LineRenderer for the spring
    private LineRenderer rearRenderer; // LineRenderer for the rear pivot to wheel
    private LineRenderer frontRenderer; // LineRenderer for the front pivot to wheel

    public int springSegments = 20; // Number of segments for the spring
    public float springAmplitude = 0.3f; // Height of the spring wave

    void Start()
    {
        // Ensure a LineRenderer exists for the spring
        lineRenderer = GetOrCreateLineRenderer(gameObject, 0.3f, Color.red, Color.blue);

        // Ensure LineRenderers exist for rear and front pivots
        rearRenderer = GetOrCreateLineRenderer(rearPivot.gameObject, 0.1f, Color.gray, Color.gray);
        frontRenderer = GetOrCreateLineRenderer(frontPivot.gameObject, 0.1f, Color.gray, Color.gray);

        // Configure spring LineRenderer
        lineRenderer.positionCount = springSegments + 2; // Total points for the spring
    }

    void FixedUpdate()
    {
        // Update spring shape between wheels
        DrawSpring();

        // Draw lines from rear and front pivots to wheels
        DrawLine(rearRenderer, rearWheel, rearPivot, 0.3f, Color.gray);
        DrawLine(frontRenderer, frontWheel, frontPivot, 0.3f, Color.gray);
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
            {
                position += orthogonal * Mathf.Sin(t * Mathf.PI * springSegments) * springAmplitude;
            }

            lineRenderer.SetPosition(i, position); // Set point position
        }
    }

    public void DrawLine(LineRenderer lineRenderer, Transform objectA, Transform objectB, float lineWidth, Color lineColor)
    {
        // Configure the LineRenderer
        lineRenderer.positionCount = 2; // A line has two points
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.startColor = lineColor;
        lineRenderer.endColor = lineColor;

        // Set the positions of the line to the positions of the two objects
        lineRenderer.SetPosition(0, objectA.position); // Start of the line
        lineRenderer.SetPosition(1, objectB.position); // End of the line
    }

    private LineRenderer GetOrCreateLineRenderer(GameObject obj, float lineWidth, Color startColor, Color endColor)
    {
        // Check if a LineRenderer exists, and create one if it doesn�t
        LineRenderer lr = obj.GetComponent<LineRenderer>();
        if (lr == null)
        {
            lr = obj.AddComponent<LineRenderer>();
        }

        // Assign a material with a shader that works with LineRenderer
        if (lr.material == null)
        {
            lr.material = new Material(Shader.Find("Sprites/Default"));
        }

        // Configure the LineRenderer
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.startColor = startColor;
        lr.endColor = endColor;

        return lr;
    }
}
