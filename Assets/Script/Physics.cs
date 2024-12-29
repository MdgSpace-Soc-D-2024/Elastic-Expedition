using UnityEngine;
using System;
using UnityEngine.UIElements;
using UnityEditor;

public class Physics : MonoBehaviour
{
    [SerializeField]
    Transform Wheel, Square, MainCam;
    //Value of some Constants
    [SerializeField, Range(0, 1)]
    float mu = 0.5f, e = 0.5f;

    //Initial Physical Values and Some Variables
    public float radius = 1.31f, mass = 1f, theta = 0f, torque = 0f, alpha = 0f, omega = 0f, times = 0f, timess = 0f, fmax = 0f, MOI, pi = (float)Math.PI;
    //Essential Vectors And initializing them
    public Vector2 pos, velocity = new Vector2(0f, 0f), acc = new Vector2(0f, 0f), gravity = new Vector2(0f, -9.8f), friction = new Vector2(0f, 0f);
    public Vector2 Normal = new Vector2(0f, 0f), Applied_Force = new Vector2(0f, 0f), net_force = new Vector2(0f, 0f), tangent = new Vector2(0f, 0f);
    //Defining Raycast object to use CircleCast
    public RaycastHit2D hit;
    void Start()
    {   //Initializing Position
        pos = Wheel.position;
    }
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        //Applying Forces through Input
        if (Input.GetKey("d"))
            Applied_Force = new Vector2(9.8f, 0f);
        else if (Input.GetKey("a"))
            Applied_Force = new Vector2(-9.8f, 0f);
        else if (Input.GetKey("w"))
            Applied_Force = new Vector2(0f, 30f);
        else
            Applied_Force = new Vector2(0f, 0f);
        /*Keeping a counter of time so that their is a buffer period between inelastic collisions 
        As if in a frame a body goes inside collider and in the next frame it is not able to come 
        completely outside then it glitches due to some problems in calculation of hit.normal*/
        timess += dt;
        //Defining hit with CircleCast(origin,radius,direction,distance)
        hit = Physics2D.CircleCast(Wheel.position, 1.31f, velocity.normalized, 0f);
        if (hit.collider != null)
        {
            Debug.Log($"Hit {hit.collider.name} at point {hit.point} with normal {hit.normal}" + hit.distance);
            //Doing Physics of Inelastic Collision
            if (timess > 2 * dt)
            {
                tangent = new Vector2(-hit.normal.y, hit.normal.x);
                float mag1 = Vector2.Dot(hit.normal, velocity);
                float mag2 = Vector2.Dot(tangent, velocity);
                //Giving New Velocity After Collision: Velocity changes along normal and remains same along tangent
                velocity = -e * mag1 * hit.normal + mag2 * tangent;
            }
            //Defining Normal
            Normal = -1f * hit.normal * (Vector2.Dot(hit.normal, gravity)) * 2f;
            //Max Friction
            fmax = mu * Normal.magnitude / 2f;
            //Physics of Rotation,Torque and Angular Acceleration
            net_force = Applied_Force + mass * gravity + Normal;
            MOI = (mass * radius * radius) / 2f;
            //Solving basic rotational dynamics
            float test_fric_mag = Vector2.Dot(net_force, tangent) * MOI / (MOI + mass * radius * radius);
            if (test_fric_mag < fmax)
            {
                if (net_force.x > 0)
                    friction = -1f * test_fric_mag * tangent;
                else
                    friction = test_fric_mag * tangent;
                acc = (net_force - friction) / mass;
                torque = radius * (friction.magnitude);
                if (velocity.x > 0f)
                    torque = -torque;
                alpha = torque / MOI;
                omega = Vector2.Dot(velocity, tangent) / radius;
            }
            else
            {
                if (net_force.x > 0)
                    friction = -1f * mu * (Normal.magnitude) * tangent;
                else
                    friction = mu * (Normal.magnitude) * tangent;
                acc = (net_force - friction) / mass;
                torque = radius * (friction.magnitude);
                if (velocity.x > 0f)
                    torque = -torque;
                alpha = torque / MOI;
            }
        }
        //Making Normal and other parameters zero if not in contact
        else
        {
            Normal = new Vector2(0f, 0f);
            friction = new Vector2(0f, 0f);
            torque = 0f;
            alpha = 0f;
            omega+=(omega > 0) ? -0.1f : 0.1f;
        }
        //Updating omega,theta and rotation
        omega = omega + alpha * dt;
        theta += omega * dt;
        Wheel.localRotation = Quaternion.Euler(0f, 0f, theta * 180f / pi);
        //Physics of Translation
        net_force = Applied_Force + mass * gravity + Normal + friction;
        acc = net_force / mass;
        velocity = velocity + acc * dt;
        pos = pos + velocity * dt + (1f / 2f) * acc * dt * dt;
        Wheel.localPosition = pos;
        MainCam.localPosition = Wheel.localPosition + new Vector3(0f, 0f, -10f);
    }
}