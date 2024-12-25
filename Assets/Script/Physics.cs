using UnityEngine;
using System;
using UnityEngine.UIElements;

public class Physics : MonoBehaviour
{
    [SerializeField]
    Transform Wheel;
    [SerializeField, Range(0, 1)]
    float mu=0.5f;
    [SerializeField, Range(0f, 10f)]
    float mass = 1f;
    [SerializeField, Range(0, 1)]
    float e = 0.5f;
    public Collider2D otherCollider;
    public float radius = 1.31f;
    public Vector2 pos;
    public float theta = 0f;
    public Quaternion ang_pos;      
    public Vector2 velocity = new Vector2(0f, 0f);
    public Vector2 acc = new Vector2(0f, 0f);
    public float torque, alpha, omega = 0f;
    public Vector2 gravity = new Vector2(0f, -9.8f);
    public Vector2 friction = new Vector2(0f, 0f);
    public Vector2 Normal = new Vector2(0f, 0f);
    public Vector2 Applied_Force = new Vector2(0f, 0f);
    public Vector2 net_force = new Vector2(0f, 0f);    
    public float times = 0f;
    public float MOI;
    public Vector2 Contact_Normal = new Vector2(0f, 0f); 
    void Start()
    {
        pos = Wheel.position;
        ang_pos = Wheel.rotation;
        theta = 0f;
    }
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        if (Input.GetKey("d"))
        {
            Applied_Force = new Vector2(20f, 0f);
        }
        else if (Input.GetKey("a"))
        {
            Applied_Force = new Vector2(-20f, 0f);
        }
        else
        {
            Applied_Force = new Vector2(0f, 0f);
        }
        times += dt;
        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider != null && otherCollider != null)
        {
            if (myCollider.bounds.Intersects(otherCollider.bounds))
            {
                if (times > 5 * dt)
                {
                    velocity.y = -e * velocity.y;
                    times = 0f;
                }
                Normal = new Vector2(0f, 9.8f);
                if (Math.Abs(velocity.y) < 0.01f)
                    velocity.y = 0f;
                if (Math.Abs(velocity.x) < 0.1f)
                {
                    friction = new Vector2(0f,0f);
                    torque = 0f;
                    alpha = 0f;
                    omega = 0f;
                }
                else
                {
                    friction = mu * (Normal.magnitude) * ((-velocity) / velocity.magnitude);
                    friction.y = 0f;
                    MOI = (mass * radius * radius)/2f;
                    torque = radius * (friction.magnitude);
                    if (velocity.x > 0f)
                        torque = -torque;                 
                    alpha = torque / MOI;
                    omega = omega + alpha * dt;
                }
            }
            else
            {
                Normal = new Vector2(0f, 0f);
                friction=new Vector2(0f, 0f);
                torque = 0f;
                alpha = 0f;
            }        
        }
        theta = theta+ omega * dt+(1f/2f)*alpha*dt*dt;
        ang_pos = Quaternion.Euler(0f, 0f, theta);
        Wheel.localRotation = ang_pos;
        net_force = Applied_Force + mass * gravity + Normal+friction;
        acc = net_force / mass;
        velocity = velocity + acc * dt;
        pos = pos + velocity * dt + (1f / 2f) * acc * dt * dt;
        Wheel.localPosition = pos;
    }
}