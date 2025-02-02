using UnityEngine;
using UnityEngine.U2D;
using System;
using Unity.VisualScripting;
public class Level_1 : MonoBehaviour
{
    public SpriteShapeController shape; // Reference to SpriteShapeController
    public int scale = 5000;            // Total width of the terrain
    public int numOfPoints = 100;       // Number of control points for the spline
    public float heightMultiplier = 100f; // Max height of terrain
    public float smoothness = 5f;      // Smoothness factor for Perlin Noise
    public int smoothResolution = 2;  // Number of interpolated points between spline points
    public int colliderDetail = 2;    // Detail level for EdgeCollider2D sampling
    public float seed = 0f;
    private EdgeCollider2D edgeCollider; // EdgeCollider2D reference
    public Sprite edgeSprite; // The sprite to use for the edge
    public Color edgeColor = Color.white;
    public GameObject Fuel,CoinPrefab;
    public GameObject Grass1,Grass2,Tree1,Stone1,Stone2,Flower,Emptyy,Cloud1,Cloud2,Cloud3,Cloud4,CloudParent,CloudParent2;
    internal static bool isTerrainRendered;

    private void Start()
    {   
        seed=UnityEngine.Random.Range(1, 10000);
        
        edgeCollider = GetComponent<EdgeCollider2D>();
        if (edgeCollider == null)
        {
            edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
        }

        // Get the distance between points based on the scale
        float distanceBetweenPoints = (float)scale / (float)numOfPoints;

        // Get the SpriteShapeController component
        shape = GetComponent<SpriteShapeController>();

        // Extend the base rectangle of the terrain to fit the specified width
        shape.spline.SetPosition(2, shape.spline.GetPosition(2) + Vector3.right * scale);
        shape.spline.SetPosition(3, shape.spline.GetPosition(3) + Vector3.right * scale);
        //Make it strech lower
        shape.spline.SetPosition(1, shape.spline.GetPosition(1) + Vector3.down * 50);
        shape.spline.SetPosition(3, shape.spline.GetPosition(3) + Vector3.down * 50);
        float prevX=0,prevY=0;
        // Generate initial control points using Perlin Noise
        for (int i = 0; i < numOfPoints; i++)
        {   
            if(i>0){
                heightMultiplier+=2f;
                smoothness +=  0.02f;
            }
            float xPos = i * distanceBetweenPoints;
            float yPos = Mathf.PerlinNoise(i / smoothness,seed) * heightMultiplier;
            Vector2 position = new Vector2(xPos, yPos);
            if(i%(numOfPoints/12) == 0)
            {
                Instantiate(Fuel,new Vector3(xPos, yPos+10, 0) , Quaternion.identity);
            }
            shape.spline.InsertPointAt(i + 2, new Vector3(xPos, yPos, 0));
            prevX = xPos;
            prevY = yPos;
        }
        //Set tangent mode for smooth curves
        // for (int i = 2; i < numOfPoints + 2; i++)
        // {
        //     shape.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        // }
        //Smooth the terrain using Catmull-Rom interpolation
        SmoothSpline();

        // Update EdgeCollider2D to follow the smooth terrain
        UpdateEdgeCollider1();
        SpawnProps1();
        SpawnClouds1();
        SpawnCoins1();
    }
    private void SmoothSpline()
    {
        var smoothedPoints = new System.Collections.Generic.List<Vector3>();
        // Iterate through each segment of the spline
        for (int i = 1; i < shape.spline.GetPointCount() - 2; i++)
        {
            // Get positions of control points
            Vector3 p0 = shape.spline.GetPosition(i - 1);
            Vector3 p1 = shape.spline.GetPosition(i);
            Vector3 p2 = shape.spline.GetPosition(i + 1);
            Vector3 p3 = shape.spline.GetPosition(i + 2);

            // Add interpolated points between p1 and p2
            for (int j = 0; j < smoothResolution; j++)
            {
                float t = j / (float)smoothResolution;
                smoothedPoints.Add(GetCatmullRomPosition(t, p0, p1, p2, p3));
            }
        }
        // Add the last control point
        smoothedPoints.Add(shape.spline.GetPosition(shape.spline.GetPointCount() - 2));
        // Clear the existing spline and recreate it using smoothed points
        shape.spline.Clear();
        for (int i = 0; i < smoothedPoints.Count; i++)
        {
            shape.spline.InsertPointAt(i, smoothedPoints[i]);
            shape.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }
    }
    private Vector3 GetCatmullRomPosition(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t3
        );
    }

    private void UpdateEdgeCollider1()
    {
        // Use a List to store sampled points
        var colliderPoints = new System.Collections.Generic.List<Vector2>();

        // Sample the smoothed SpriteShape curve
        for (int i = 0; i < shape.spline.GetPointCount() - 1; i++)
        {
            Vector3 start = shape.spline.GetPosition(i);
            Vector3 end = shape.spline.GetPosition(i + 1);

            // Subdivide each segment using the specified colliderDetail
            for (int j = 0; j <= colliderDetail; j++)
            {
                float t = j / (float)colliderDetail;
                Vector3 point = Vector3.Lerp(start, end, t); // Linear interpolation for collider
                colliderPoints.Add(new Vector2(point.x, point.y));
            }
        }

        // Assign sampled points to the EdgeCollider2D
        edgeCollider.points = colliderPoints.ToArray();
    }
private void SpawnProps1()
{
    int pointCount = shape.spline.GetPointCount();
    Transform propParent = new GameObject("PropsParent").transform; // Group all props

    for (int i = 0; i < pointCount - 1; i++)
    {
        Vector3 point = shape.spline.GetPosition(i);
        Vector3 nextPoint = shape.spline.GetPosition(i + 1);

        // Calculate the tangent and slope angle
        Vector3 tangent = nextPoint - point;
        float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;

        // Number of props per segment (1 to 3 randomly)
        int numProps = UnityEngine.Random.Range(1, 4);

        for (int j = 0; j < numProps; j++)
        {
            // Offset position slightly along the segment
            float t = UnityEngine.Random.Range(0f, 1f);
            Vector3 spawnPoint = Vector3.Lerp(point, nextPoint, t);

            // Add small random variation in height
            spawnPoint.y += UnityEngine.Random.Range(-0.2f, 0f);

            // Randomize prop selection
            int randomNumber = UnityEngine.Random.Range(1, 101);
            GameObject prefab = null;

            if (randomNumber <= 50)
            {
                prefab = (randomNumber > 30) ? Grass1 : (randomNumber > 10) ? Grass2 : Tree1;
            }
            else
            {
                if (randomNumber < 60)
                    prefab = Stone1;
                else if (randomNumber < 70)
                    prefab = Stone2;
                else if (randomNumber < 75)
                    prefab = Flower;
            }

            if (prefab != null)
            {
                // Instantiate with slight random scaling
                GameObject prop = Instantiate(prefab, spawnPoint, Quaternion.Euler(0, 0, angle));
                prop.transform.localScale *= UnityEngine.Random.Range(0.6f, 1.4f); // Scale variation
                prop.transform.SetParent(propParent); // Organize under parent
            }
        }
    }
}


    private void SpawnClouds1()
{
    int pointCount = shape.spline.GetPointCount(); // Store count for efficiency // Create parent for organization

    for (int i = 0; i < pointCount; i++)
    {
        Vector3 point = shape.spline.GetPosition(i);
        int randomNumber = UnityEngine.Random.Range(1, 200); // Use Unity's Random

        GameObject cloudPrefab = null;
        float heightOffset = 0f;

        if (randomNumber <= 10)
        {
            cloudPrefab = Cloud1;
            heightOffset = UnityEngine.Random.Range(20f, 45f);
        }
        else if (randomNumber <= 20)
        {
            cloudPrefab = Cloud2;
            heightOffset = UnityEngine.Random.Range(30f, 55f);
        }
        else if (randomNumber <= 30)
        {
            cloudPrefab = Cloud3;
            heightOffset= UnityEngine.Random.Range(20f, 35f);
        }
        else if (randomNumber <= 40)
        {
            cloudPrefab = Cloud4;
            heightOffset = UnityEngine.Random.Range(35f, 50f);
        }

        if (cloudPrefab != null)
        {
            GameObject cloudInstance = Instantiate(cloudPrefab, point + new Vector3(0, heightOffset, 0), Quaternion.identity);
            if(randomNumber<=20)
                cloudInstance.transform.SetParent(CloudParent.transform); // Assign to parent GameObject
            else
                cloudInstance.transform.SetParent(CloudParent2.transform);
        }
    }
}
private void SpawnCoins1()
{
    int pointCount = shape.spline.GetPointCount(); // Store count for efficiency

    for (int i = 0; i < pointCount - 1; i += UnityEngine.Random.Range(5, 15)) // Random gaps between bundles
    {
        Vector3 point = shape.spline.GetPosition(i);
        Vector3 nextPoint = shape.spline.GetPosition(i + 1);

        // Calculate tangent (slope direction)
        Vector3 tangent = (nextPoint - point).normalized;
        float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;

        // Number of coins in bundle increases as we move forward
        int coinsInBundle = Mathf.Clamp(i / 20, 1, 3); // Min 1, Max 3 coins per bundle
        float coinSpacing = 7f; // Distance between coins in a bundle

        for (int j = 0; j < coinsInBundle; j++)
        {
            // Place coins along the slope by moving in the tangent direction
            Vector3 coinPosition = point + tangent * (j * coinSpacing) + new Vector3(0, 5f, 0); // Slight vertical offset
            Instantiate(CoinPrefab, coinPosition, Quaternion.Euler(0, 0, angle));
        }
    }
}


}
