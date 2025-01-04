using UnityEngine;
using System;
using UnityEngine.UIElements;
using UnityEditor;
using System.Threading;

public class Physics : MonoBehaviour
{
    [SerializeField]
    Transform Wheel, Wheel2, Square, MainCam;
    //Value of some Constants
    [SerializeField, Range(0, 1)]
    float mu = 0.5f, e = 0.5f;
    //Initial Physical Values and Some Variables
    public float radius = 1.31f, mass = 1f, theta1 = 0f,  alpha1 = 0f, torque1 = 0f, theta2 = 0f,  alpha2 = 0f, torque2 = 0f,fmax1 = 0f, fmax2 = 0f, MOI, pi = (float)Math.PI;
    public float rod_length = 7.2f, omega_cm = 0f, theta_cm = 0f, alpha_cm = 0f,omega1 = 0f, omega2 = 0f;
    //Essential Vectors And initializing them
    public Vector2 pos1, pos2, velocity_cm = new Vector2(0f, 0f), velocity1 = new Vector2(0f, 0f), velocity2 = new Vector2(0f, 0f), acc = new Vector2(0f, 0f), gravity = new Vector2(0f, -9.8f), friction1 = new Vector2(0f, 0f), friction2 = new Vector2(0f, 0f);
    public Vector2 Normal1 = new Vector2(0f, 0f), Normal2 = new Vector2(0f, 0f), Applied_Force = new Vector2(0f, 0f), net_force = new Vector2(0f, 0f), tangent1 = new Vector2(0f, 0f), tangent2 = new Vector2(0f, 0f);
    public Vector2 rod_normal = new Vector2(0f, 0f), rod_along = new Vector2(0f, 0f);
    //Defining Raycast object to use CircleCast
    public RaycastHit2D hit1, hit2;
    public float MOI_cm = 0f;
    public float check = 0f, prev_check = 0f;
    void Start()
    {   //Initializing Position
        pos1 = Wheel.position;
        pos2 = Wheel2.position;
        MOI = (mass * radius * radius) / 2f;
        MOI_cm = 2 * MOI + (mass * (rod_length * rod_length) / 2f);
    }

    void FixedUpdate()
    {
        rod_along = new Vector2(pos2.x - pos1.x, pos2.y - pos1.y).normalized;
        rod_normal = new Vector2(-rod_along.y, rod_along.x).normalized;
        float dt = Time.fixedDeltaTime;
        //Applying Forces through Input
        if (Input.GetKey("d"))
        { Applied_Force = (new Vector2(30f, 0f).magnitude)*rod_along; } //alpha_cm -= 2f; }
        else if (Input.GetKey("a"))
        { Applied_Force = -30f*rod_along; }// alpha_cm += 2f; }
        else if (Input.GetKey("w"))
            Applied_Force = new Vector2(0f, 30f);
        else
            Applied_Force = new Vector2(0f, 0f);
        /*Keeping a counter of time so that their is a buffer period between inelastic collisions 
        As if in a frame a body goes inside collider and in the next frame it is not able to come 
        completely outside then it glitches due to some problems in calculation of hit.normal*/
        //Defining hit with CircleCast(origin,radius,direction,distance)
        hit1 = Physics2D.CircleCast(Wheel.position, 1.31f, velocity_cm.normalized, 0f);
        hit2 = Physics2D.CircleCast(Wheel2.position, 1.31f, velocity_cm.normalized, 0f);
        if (hit1.collider != null && hit2.collider != null)
        {
            check = 1f;
            hit1.normal = hit1.normal.normalized;
            hit2.normal = hit2.normal.normalized;
            Debug.Log($"Hit {hit1.collider.name} at point {hit1.point} with normal {hit1.normal}" + hit1.distance);
            //Doing Physics of Inelastic Collision
            tangent1 = new Vector2(hit1.normal.y, -hit1.normal.x);
            tangent2 = new Vector2(hit2.normal.y, -hit2.normal.x);
            float mag1_n = Vector2.Dot(hit1.normal, velocity_cm);
            float mag1_t = Vector2.Dot(tangent1, velocity_cm);
            float mag2_n = Vector2.Dot(hit2.normal, velocity_cm);
            float mag2_t = Vector2.Dot(tangent2, velocity_cm);
            //Giving New Velocity After Collision: Velocity changes along normal and remains same along tangent
            velocity1 = (-e * mag1_n * hit1.normal + mag1_t * tangent1);
            velocity2 = -e * mag2_n * hit1.normal + mag2_t * tangent1;
            velocity_cm = ((-e * mag1_n * hit1.normal + mag1_t * tangent1) + (-e * mag2_n * hit1.normal + mag2_t * tangent1)) / 2;
            //Defining Normal
            Normal2 = -1f * hit2.normal * (Vector2.Dot(hit2.normal, gravity));
            Normal1 = -1f * hit1.normal * (Vector2.Dot(hit1.normal, gravity));
            //Max Friction
            fmax1 = mu * Normal1.magnitude;
            fmax2 = mu * Normal2.magnitude;
            alpha_cm = 0f;
            omega_cm = 0f;
            //Physics of Rotation,Torque and Angular Acceleration
            net_force = Applied_Force + 2f * mass * gravity + Normal1 + Normal2;
            //Solving basic rotational dynamics
            float test_fric_mag1 = Math.Abs((Vector2.Dot(net_force, tangent1) * MOI) / (MOI + mass * radius * radius));
            float test_fric_mag2 = Math.Abs((Vector2.Dot(net_force, tangent2) * MOI) / (MOI + mass * radius * radius));
            if (Math.Abs(Vector2.Dot(velocity1, tangent1)) > 0.1f)
            {
                //ignoring the case where w>v
                if (Math.Abs(Vector2.Dot(velocity1, tangent1) + radius * omega1) < 0.3f)
                {
                    if (test_fric_mag1 < fmax1)
                    {
                        if (Vector2.Dot(velocity1, tangent1) + radius * omega1 > 0f)
                            friction1 = -test_fric_mag1 * tangent1;
                        else
                            friction1 = test_fric_mag1 * tangent1;
                        torque1 = radius * (friction1.magnitude);
                        if (Vector2.Dot(friction1, tangent1) < 0f && torque1 > 0)
                            torque1 = -torque1;
                        alpha1 = torque1 / MOI;
                        //omega1 = Vector2.Dot(velocity_cm, tangent1) / radius;
                    }
                    else
                    {
                        if (Vector2.Dot(velocity1, tangent1) + radius * omega1 > 0f)
                            friction1 = -mu * (Normal1.magnitude) * tangent1;
                        else
                            friction1 = mu * (Normal1.magnitude) * tangent1;
                        torque1 = radius * (friction1.magnitude);
                        if (Vector2.Dot(friction1, tangent1) < 0f && torque1 > 0)
                            torque1 = -torque1;
                        alpha1 = torque1 / MOI;
                        //omega1 = Vector2.Dot(velocity_cm, tangent1) / radius;
                    }
                }
                else
                {
                    if (Vector2.Dot(velocity1, tangent1) + radius * omega1 > 0f)
                        friction1 = -mu * (Normal1.magnitude) * tangent1;
                    else
                        friction1 = mu * (Normal1.magnitude) * tangent1;
                    torque1 = radius * (friction1.magnitude);
                    if (Vector2.Dot(friction1, tangent1) < 0f && torque1 > 0)
                        torque1 = -torque1;
                    alpha1 = torque1 / MOI;
                    //omega1 = Vector2.Dot(velocity_cm, tangent1) / radius;

                }

            }
            else
            {
                //it should be along tangent
                velocity_cm -= Vector2.Dot(velocity_cm, tangent1) * tangent1;
                friction1 = new Vector2(0f, 0f);
                torque1 = 0f;
                omega1 = 0f;
                alpha1 = 0f;
            }
            if (Math.Abs(Vector2.Dot(velocity2, tangent2)) > 0.1f)
            {
                if (Math.Abs(Vector2.Dot(velocity2, tangent2) + radius * omega2) < 0.5f)
                {
                    if (test_fric_mag2 < fmax2)
                    {
                        if (Vector2.Dot(velocity2, tangent2) + radius * omega2 > 0f)
                            friction2 = -test_fric_mag2 * tangent2;
                        else
                            friction2 = test_fric_mag2 * tangent2;
                        torque2 = radius * (friction2.magnitude);
                        if (Vector2.Dot(friction2, tangent2) < 0 && torque2 > 0)
                            torque2 = -torque2;
                        alpha2 = torque2 / MOI;
                        //omega2 = Vector2.Dot(velocity_cm, tangent2) / radius;
                    }
                    else
                    {
                        if (Vector2.Dot(velocity2, tangent2) + radius * omega2 > 0f)
                            friction2 = -mu * (Normal2.magnitude) * tangent2;
                        else
                            friction2 = mu * (Normal2.magnitude) * tangent2;
                        torque2 = radius * (friction2.magnitude);
                        if (Vector2.Dot(friction2, tangent2) < 0 && torque2 > 0)
                            torque2 = -torque2;
                        alpha2 = torque2 / MOI;
                        //omega2 = Vector2.Dot(velocity_cm, tangent2) / radius;
                    }
                }
                else
                {
                    if (Vector2.Dot(velocity2, tangent2) + radius * omega2 > 0f)
                        friction2 = -mu * (Normal2.magnitude) * tangent2;
                    else
                        friction2 = mu * (Normal2.magnitude) * tangent2;
                    torque2 = radius * (friction2.magnitude);
                    if (Vector2.Dot(friction2, tangent2) < 0 && torque2 > 0)
                        torque2 = -torque2;
                    alpha2 = torque2 / MOI;
                    //omega2 = Vector2.Dot(velocity_cm, tangent2) / radius;
                }
            }
            else
            {
                velocity_cm -= Vector2.Dot(velocity_cm,tangent2)*tangent2;
                friction2 = new Vector2(0f, 0f);
                torque2 = 0f;
                omega2 = 0f;
                alpha2 = 0f;
            }
            alpha_cm = 0f;
            omega_cm = 0f;

        }
        else if (hit1.collider != null)
        {
            check = 2f;
            hit1.normal=hit1.normal.normalized;
            Debug.Log($"Hit {hit1.collider.name} at point {hit1.point} with normal {hit1.normal}" + hit1.distance);
            tangent1 = new Vector2(hit1.normal.y, -hit1.normal.x);
            float mag1_n = Vector2.Dot(hit1.normal, velocity_cm);
            float mag1_t = Vector2.Dot(tangent1, velocity_cm);
            velocity1 = (-e * mag1_n * hit1.normal + mag1_t * tangent1);
            velocity2 = velocity_cm;
            velocity_cm = ((-e * mag1_n * hit1.normal + mag1_t * tangent1) + velocity_cm) / 2;
            double angle_theta = Math.Atan((double)(tangent1.y / tangent1.x));
            double angle_phi = Math.Atan((double)(rod_along.y / rod_along.x));
            Normal1 = -2f*mass* hit1.normal * (Vector2.Dot(hit1.normal, gravity));
            alpha_cm = (mass*rod_length * (Vector2.Dot(gravity, rod_normal))) / MOI_cm;
            net_force = Applied_Force + 2f * mass * gravity + Normal1;
            float test_fric_mag1 = Math.Abs(Vector2.Dot(Vector2.Dot(net_force, rod_along) * (rod_along), tangent1));
            fmax1 = mu * Normal1.magnitude;
            if (Math.Abs(Vector2.Dot(velocity1, tangent1)) > 0.1f)
            {
                //ignoring the case where w>v
                if (Math.Abs(Vector2.Dot(velocity1, tangent1) + radius * omega1) < 0.3f)
                {
                    if (test_fric_mag1 < fmax1)
                    {
                        if (Vector2.Dot(velocity1, tangent1) + radius * omega1 > 0f)
                            friction1 = -test_fric_mag1 * tangent1;
                        else
                            friction1 = test_fric_mag1 * tangent1;
                        torque1 = radius * (friction1.magnitude);
                        if (Vector2.Dot(friction1, tangent1) < 0f && torque1 > 0)
                            torque1 = -torque1;
                        alpha1 = torque1 / MOI;
                        //omega1 = Vector2.Dot(velocity_cm, tangent1) / radius;
                    }
                    else
                    {
                        if (Vector2.Dot(velocity1, tangent1) + radius * omega1 > 0f)
                            friction1 = -mu * (Normal1.magnitude) * tangent1;
                        else
                            friction1 = mu * (Normal1.magnitude) * tangent1;
                        torque1 = radius * (friction1.magnitude);
                        if (Vector2.Dot(friction1, tangent1) < 0f && torque1 > 0)
                            torque1 = -torque1;
                        alpha1 = torque1 / MOI;
                        //omega1 = Vector2.Dot(velocity_cm, tangent1) / radius;
                    }
                }
                else
                {
                    if (Vector2.Dot(velocity1, tangent1) + radius * omega1 > 0f)
                        friction1 = -mu * (Normal1.magnitude) * tangent1;
                    else
                        friction1 = mu * (Normal1.magnitude) * tangent1;
                    torque1 = radius * (friction1.magnitude);
                    if (Vector2.Dot(friction1, tangent1) < 0f && torque1 > 0)
                        torque1 = -torque1;
                    alpha1 = torque1 / MOI;
                    //omega1 = Vector2.Dot(velocity_cm, tangent1) / radius;

                }
            }
            else
            {
                //it should be along tangent
                velocity_cm = Vector2.Dot(velocity_cm, tangent1) * tangent1;
                friction1 = new Vector2(0f, 0f);
                torque1 = 0f;
                omega1 = 0f;
                alpha1 = 0f;
            }
        }
        else if (hit2.collider != null)
        {
            check = 3f;
            hit2.normal = hit2.normal.normalized;
            tangent2 = new Vector2(-hit2.normal.y, hit2.normal.x);
            float mag2_n = Vector2.Dot(hit2.normal, velocity_cm);
            float mag2_t = Vector2.Dot(tangent2, velocity_cm);
            velocity1 = velocity_cm;
            velocity2 = (-e * mag2_n * hit2.normal + mag2_t * tangent2);
            velocity_cm = ((-e * mag2_n * hit2.normal + mag2_t * tangent2) + velocity_cm) / 2;
            double angle_theta = Math.Atan((double)(tangent2.y / tangent2.x));
            double angle_phi = Math.Atan((double)(rod_along.y / rod_along.x));
            Normal2 = -2f * mass * hit2.normal * (Vector2.Dot(hit2.normal, gravity));
            alpha_cm = -(mass*rod_length * (Vector2.Dot(gravity, rod_normal))) / MOI_cm;
            //omega_cm += alpha_cm * dt;
            net_force = Applied_Force + 2f * mass * gravity + Normal2;
            float test_fric_mag2 = Vector2.Dot(Vector2.Dot(net_force, rod_along) * (rod_along), tangent2);
            fmax2 = mu * Normal2.magnitude;
            if (Math.Abs(Vector2.Dot(velocity2, tangent2)) > 0.1f)
            {
                if (Math.Abs(Vector2.Dot(velocity2, tangent2) + radius * omega2) < 0.5f)
                {
                    if (test_fric_mag2 < fmax2)
                    {
                        if (Vector2.Dot(velocity2, tangent2) + radius * omega2 > 0f)
                            friction2 = -test_fric_mag2 * tangent2;
                        else
                            friction2 = test_fric_mag2 * tangent2;
                        torque2 = radius * (friction2.magnitude);
                        if (Vector2.Dot(friction2, tangent2) < 0 && torque2 > 0)
                            torque2 = -torque2;
                        alpha2 = torque2 / MOI;
                        //omega2 = Vector2.Dot(velocity_cm, tangent2) / radius;
                    }
                    else
                    {
                        if (Vector2.Dot(velocity2, tangent2) + radius * omega2 > 0f)
                            friction2 = -mu * (Normal2.magnitude) * tangent2;
                        else
                            friction2 = mu * (Normal2.magnitude) * tangent2;
                        torque2 = radius * (friction2.magnitude);
                        if (Vector2.Dot(friction2, tangent2) < 0 && torque2 > 0)
                            torque2 = -torque2;
                        alpha2 = torque2 / MOI;
                        //omega2 = Vector2.Dot(velocity_cm, tangent2) / radius;
                    }
                }
                else
                {
                    if (Vector2.Dot(velocity2, tangent2) + radius * omega2 > 0f)
                        friction2 = -mu * (Normal2.magnitude) * tangent2;
                    else
                        friction2 = mu * (Normal2.magnitude) * tangent2;
                    torque2 = radius * (friction2.magnitude);
                    if (Vector2.Dot(friction2, tangent2) < 0 && torque2 > 0)
                        torque2 = -torque2;
                    alpha2 = torque2 / MOI;
                    //omega2 = Vector2.Dot(velocity_cm, tangent2) / radius;
                }
            }
            else
            {
                velocity_cm -= Vector2.Dot(velocity_cm, tangent2) * tangent2;
                friction2 = new Vector2(0f, 0f);
                torque2 = 0f;
                omega2 = 0f;
                alpha2 = 0f;
            }
        }
        //Making Normal and other parameters zero if not in contact
        else
        {
            Normal1 = new Vector2(0f, 0f); Normal2 = new Vector2(0f, 0f); friction1 = new Vector2(0f, 0f); friction2 = new Vector2(0f, 0f);
            torque1 = 0f; torque2 = 0f; alpha1 = 0f; alpha2 = 0f;
            //omega1 += (omega1 > 0) ? -0.1f : 0.1f;
            //omega2 += (omega2 > 0) ? -0.1f : 0.1f;
            alpha_cm = 0f;
        }
        //Updating omega,theta and rotation
        omega1 = omega1 + alpha1 * dt;
        theta1 += omega1 * dt;
        omega2 = omega2 + alpha2 * dt;
        theta2 += omega2 * dt;
        Wheel.localRotation = Quaternion.Euler(0f, 0f, theta1 * 180f / pi);
        Wheel2.localRotation = Quaternion.Euler(0f, 0f, theta2 * 180f / pi);
        //Physics of Translation
        net_force = Applied_Force + 2f*mass * gravity + Normal1 + Normal2 + friction1 + friction2;
        acc = net_force / (mass * 2);
        velocity_cm = velocity_cm + acc * dt;
        float v1 = Vector2.Dot(velocity1, rod_normal), v2 = Vector2.Dot(velocity2, rod_normal);
        if (velocity1 != velocity2 && prev_check!=check)
        {
            if (v1 * v2 > 0f)
            {
                if (Math.Abs(v1) > Math.Abs(v2))
                    omega_cm = Math.Abs((v1 - v2) / rod_length);
                else
                    omega_cm = -Math.Abs((v1 - v2) / rod_length);
            }
            else
            {
                if (v1 > 0)
                    omega_cm = Math.Abs((v1 - v2) / rod_length);
                else
                    omega_cm = -Math.Abs((v1 - v2) / rod_length);
            }
        }
        prev_check= check;
        omega_cm += alpha_cm * dt;
        velocity1 = velocity_cm + -1f * rod_length * omega_cm * rod_normal ;
        velocity2 = velocity_cm +1f * rod_length * omega_cm * rod_normal ;        
        //omega_cm += (omega_cm > 0) ? -0.1f : 0.1f;
        pos1 = pos1 + velocity1 * dt + (1f / 2f) * acc * dt * dt;
        pos2 = pos2 + velocity2 * dt + (1f / 2f) * acc * dt * dt;
        Wheel.localPosition = pos1;
        Wheel2.localPosition = pos2;
        MainCam.localPosition = (Wheel.localPosition + Wheel2.localPosition) / 2f + new Vector3(0f, 0f, -10f);
    }
}