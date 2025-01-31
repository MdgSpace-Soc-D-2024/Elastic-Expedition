using UnityEngine;
using UnityEngine.U2D;
using System;
using Unity.VisualScripting;
public class TerrainCreator : MonoBehaviour
{
    public SpriteShapeController shape; // Reference to SpriteShapeController
    public int scale = 5000;            // Total width of the terrain
    public int numOfPoints = 100;       // Number of control points for the spline
    public float heightMultiplier = 5f; // Max height of terrain
    public float smoothness = 10f;      // Smoothness factor for Perlin Noise
    public int smoothResolution = 10;  // Number of interpolated points between spline points
    public int colliderDetail = 10;    // Detail level for EdgeCollider2D sampling
    public float seed = 0f;
    private EdgeCollider2D edgeCollider; // EdgeCollider2D reference
    public Sprite edgeSprite; // The sprite to use for the edge
    public Color edgeColor = Color.white;
    public GameObject Fuel;
    public GameObject Grass1,Grass2,Tree1,Stone1,Stone2,Flower,Emptyy,Cloud1,Cloud2;

    private void Start()
    {   
        
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
            if(i>50){
                heightMultiplier+=1f;
                smoothness +=  0.05f;
            }
            float xPos = i * distanceBetweenPoints;
            float yPos = Mathf.PerlinNoise(i / smoothness,seed) * heightMultiplier;
            Vector2 position = new Vector2(xPos, yPos);
            if(i%(numOfPoints/20) == 0)
            {
                Instantiate(Fuel,new Vector3(xPos, yPos+10, 0) , Quaternion.identity);
            }
            shape.spline.InsertPointAt(i + 2, new Vector3(xPos, yPos, 0));
            prevX = xPos;
            prevY = yPos;
        }
        // Set tangent mode for smooth curves
        for (int i = 2; i < numOfPoints + 2; i++)
        {
            shape.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }
        // Smooth the terrain using Catmull-Rom interpolation
        SmoothSpline();

        // Update EdgeCollider2D to follow the smooth terrain
        UpdateEdgeCollider();
        SpawnGrass();
        SpawnClouds();
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

    private void UpdateEdgeCollider()
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
    private void SpawnGrass()
{   
    for (int i = 50; i < shape.spline.GetPointCount() - 1; i++)
    {
        Vector3 point = shape.spline.GetPosition(i);
        Vector3 nextPoint = shape.spline.GetPosition(i + 1);

        // Calculate the slope (tangent vector)
        Vector3 tangent = nextPoint - point;
        float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;

        // Randomize grass placement
        
        int randomNumber = new System.Random().Next(1, 1001);
        if (randomNumber <=100)
        {   
            GameObject grassPrefab ;
            if(randomNumber > 50)
                grassPrefab= Grass1;
            else if(randomNumber > 3)
                grassPrefab= Grass2;
            else
                grassPrefab= Tree1;
            // Instantiate and rotate grass to align with slope
            Instantiate(grassPrefab, point+new Vector3(0,0,0), Quaternion.Euler(0, 0, angle));
        }
        else if(randomNumber <=130)
        {   
            GameObject StonePrefab =null;
            if(randomNumber >100){
            if(randomNumber <108)
                StonePrefab= Stone1;
            else if(randomNumber < 120)
                StonePrefab= Stone2;
            else if(randomNumber < 123)
                StonePrefab= Flower;
            else 
                StonePrefab= Emptyy;
            }
            // Instantiate and rotate grass to align with slope
            Instantiate(StonePrefab, point+new Vector3(0,-0.3f,0), Quaternion.Euler(0, 0, angle));
        }
    }
}
    private void SpawnClouds()
    {
        for (int i = 50; i < shape.spline.GetPointCount() - 1; i++)
        {
            Vector3 point = shape.spline.GetPosition(i);
            Vector3 nextPoint = shape.spline.GetPosition(i + 1);
            GameObject CloudPrefab = null; 
            int randomNumber = new System.Random().Next(1, 1001);
            if(randomNumber <=10)
            {
                CloudPrefab = Cloud1;
                GameObject c=Instantiate(CloudPrefab,point+new Vector3(0,new System.Random().Next(35, 55),0), Quaternion.identity);

            }
            else if(randomNumber <=20)
            {
                CloudPrefab = Cloud2;
                Instantiate(CloudPrefab,point+new Vector3(0,new System.Random().Next(40, 70),0), Quaternion.identity);
            }
        }
    }
}
