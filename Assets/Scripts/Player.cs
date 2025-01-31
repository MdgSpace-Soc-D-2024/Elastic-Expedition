using UnityEngine;

public class CircleMovement : MonoBehaviour
{
    public Transform MainCamera; // Reference to the main camera
    public Rigidbody2D rb;       // Reference to the Rigidbody2D component
    public float forceAmount = 9.8f; // Force applied when moving

    private Vector2 appliedForce;

    void Update()
    {
        // Check for key input to apply force
        if (Input.GetKey("d"))
            appliedForce = new Vector2(forceAmount, 0f);
        else if (Input.GetKey("a"))
            appliedForce = new Vector2(-forceAmount, 0f);
        else if (Input.GetKey("w"))
            appliedForce = new Vector2(0f, forceAmount);
        else
            appliedForce = Vector2.zero;
    }

    void FixedUpdate()
    {
        // Apply the force to the Rigidbody2D
        rb.AddForce(appliedForce);

        // Make the camera follow the circle
        if (MainCamera != null)
        {
            MainCamera.position = new Vector3(transform.position.x, transform.position.y, MainCamera.position.z);
        }
    }
}

