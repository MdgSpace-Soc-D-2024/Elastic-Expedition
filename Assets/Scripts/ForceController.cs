using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ForceController : MonoBehaviour
{
    public Transform wheel1, wheel2;
    public Vector2 AppliedForce { get; private set; } = Vector2.zero;
    public GameObject rear,front;
    public Rear_wheel Script_R;
    public Front_wheel Script_F;
    public Vector2 rod_along,force=new Vector2(0,0);
    private void Start()
    {
        Script_R=rear.GetComponent<Rear_wheel>();
        Script_F=front.GetComponent<Front_wheel>();
        
    }
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        rod_along = (wheel1.localPosition - wheel2.localPosition).normalized;
        float horizontalInput = Input.GetAxis("Horizontal");
        float VerticalInput = Input.GetAxis("Vertical");
        //Update applied force based on user input
        if (horizontalInput == 1)
        {
            AppliedForce = new Vector2(30f, 0f);
            if (Script_F.hits.Length == 0)
            {
                Vector2 force = new Vector2(-rod_along.y,rod_along.x).normalized*0.05f;
                Script_F.velocity+=force;
                //Script_R.ApplyForces(-force, dt);
            }
        }
        else if (horizontalInput == -1)
        {
            AppliedForce = new Vector2(-50f, 0f);
            if (Script_F.hits.Length == 0)
            {
                Vector2 force = new Vector2(-rod_along.y,rod_along.x).normalized*0.05f;
                Script_F.velocity -= force;
                //Script_R.ApplyForces(-force, dt);
            }
        }
        else if (Input.GetKey("e"))
            AppliedForce = new Vector2(0f, 100f);
        else
            AppliedForce = Vector2.zero;
        if (VerticalInput == 1)
        {
            force = 100 * rod_along;
            Script_F.ApplyForces(force, dt);
            Script_R.ApplyForces(-force, dt);
        }
        else if(VerticalInput == -1)
        {
            force = -100 * rod_along;
            Script_F.ApplyForces(force, dt);
            Script_R.ApplyForces(-force, dt);
        }
        else
            force = new Vector2(0, 0);  
    }
}
