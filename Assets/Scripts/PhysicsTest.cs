using UnityEngine;
using System;
using static Unity.Burst.Intrinsics.X86;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PhysicsTest : MonoBehaviour
{
    [SerializeField] Transform Wheel, Square, MainCam;
    [SerializeField, Range(0, 1)] float mu = 0.5f, e = 0.5f; // Coefficient of friction (mu) and restitution (e)

    public float radius = 1.31f, mass = 1f, omega = 0f, fmax = 0f, MOI, pi = Mathf.PI, theta = 0f, torque = 0f;
    public Vector2 pos, velocity = Vector2.zero, gravity = new Vector2(0f, -9.8f), acc = Vector2.zero;

    [SerializeField] Vector2 tangent = Vector2.zero, normal = Vector2.zero, appliedForce = Vector2.zero, netForce = Vector2.zero, friction = Vector2.zero;
    
    private RaycastHit2D[] hits;
    private RaycastHit2D[] previousHits = new RaycastHit2D[0];
    int c = 0;
    float timee = 0f;

    private void Start()
    {
        pos = Wheel.position;
        MOI = 0.5f * mass * radius * radius; // Moment of inertia for a solid disk
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        timee += dt;
        // Handle user input for applied forces
        UpdateAppliedForces();

        hits = Physics2D.CircleCastAll(Wheel.position, radius, velocity.normalized, 0.01f);
        // Check for collision using CircleCast
        List<RaycastHit2D> newHits = new List<RaycastHit2D>();
        foreach (var currentHit in hits)
        {
            bool isNew = true;
            foreach (var previousHit in previousHits)
            {
                // Check if the normal is similar (threshold for similarity) and if they hit the same collider
                if (Vector2.Dot(currentHit.normal.normalized, previousHit.normal.normalized) >0.99f)
                {
                    isNew = false;
                    break;
                }
            }
            if (isNew)
            {
                newHits.Add(currentHit);
            }
        }
        // Update previous hits for the next frame
        
        // Convert new hits list to array for further processing
        RaycastHit2D[] newHitsArray = newHits.ToArray();
        //foreach(var newhits in newHitsArray)
        //Debug.Log("newHitsArray" + newhits.normal);
        //Debug.Log("NewHitsArray :" + newHitsArray.Length);
        //Debug.Log("Hits :" + hits.Length);
        if (hits.Length > 0)
        {
            if (newHitsArray.Length > 0)
            {
                HandleCollision(newHitsArray, dt, true);
            }
            else
            {
                HandleCollision(hits, dt, false);
            }
        }
        else
        {
            HandleNoCollision(dt);
        }
        // Apply net forces and update the physics
        netForce = appliedForce + mass * gravity + normal + friction;
        ApplyForces(netForce, dt);
        // Update camera position
        MainCam.localPosition = Wheel.localPosition + new Vector3(0f, 0f, -10f);
        previousHits = hits;

    }

    private void UpdateAppliedForces()
    {
        if (Input.GetKey("d"))
            appliedForce = new Vector2(12f, 0f);
        else if (Input.GetKey("a"))
            appliedForce = new Vector2(-12f, 0f);
        else if (Input.GetKey("w"))
            appliedForce = new Vector2(0f, 70f);
        else
            appliedForce = Vector2.zero;
    }

    private void HandleCollision(RaycastHit2D[] hits, float dt,bool flag)
    {
        Debug.Log(flag);
        Debug.Log("Coll Chal rha hai");
        Vector2 Netnormal = new Vector2(0f, 0f);
        float netDistance = 0f;
        foreach (var hit in hits)
        {
            Netnormal += hit.normal;
            netDistance += hit.distance;
        }
        Netnormal = Netnormal.normalized;
        Vector2 collisionNormal = Netnormal.normalized;
        Vector2 relativeVelocity = velocity; // Assuming terrain is static
        float relativeNormalVelocity = Vector2.Dot(relativeVelocity, collisionNormal);
        Vector2 penetrationDirection = Netnormal.normalized;
        float penetrationDepth = radius - netDistance;
        ResolvePenetration(penetrationDepth, penetrationDirection);

        // If the objects are moving away, skip collision handling
        if (relativeNormalVelocity > 0)
            return;

        // Calculate impulse scalar
        float impulseMagnitude = -(1 + e) * relativeNormalVelocity / (1 / mass);

        // Calculate impulse vector   
        Vector2 impulse = impulseMagnitude * collisionNormal;

        // Apply impulse to velocity
        velocity += impulse / mass;

        // Update velocity along tangent (friction calculation)
        tangent = new Vector2(collisionNormal.y, -collisionNormal.x);
        float tangentVelocity = Vector2.Dot(velocity, tangent);

        // Update normal force
        normal = (-Vector2.Dot(gravity,collisionNormal) * mass - Vector2.Dot(appliedForce, Netnormal)) * Netnormal;
        
        if(Math.Abs(velocity.x)>0.3f)
        HandleFrictionNormally(ref velocity, ref omega, hits[0].point, Netnormal, radius,mu,normal);        
    }

    
   private void HandleFrictionNormally(ref Vector2 velocity1, ref float omega1, Vector2 contactPoint, Vector2 collisionNormal,
       float radius,float mu, Vector2 normal)
   {
       // Compute tangent vector (perpendicular to the collision normal)
       Vector2 tangent1 = new Vector2(collisionNormal.y,-collisionNormal.x).normalized;

       // Calculate net force
       netForce = appliedForce + mass * gravity + normal;
       // Compute friction magnitude
       float testFrictionMagnitude = Vector2.Dot(netForce, tangent) * MOI / (MOI + mass * radius * radius);
       //Debug.Log(testFrictionMagnitude);
       float fmax1 = mu * normal.magnitude;
       fmax = fmax1;
       // Tangential velocity (v + rω)
       float relativeVelocity = Vector2.Dot(velocity1, tangent1) + radius * omega1;

       if (Mathf.Abs(Vector2.Dot(velocity1, tangent1)) > 0.01f) // Moving
       {
           if (Mathf.Abs(relativeVelocity) < 0.1f)//V=rw
           {
               if (testFrictionMagnitude < fmax1) // Static friction case
               {
                   if (relativeVelocity > 0f)
                       friction = -testFrictionMagnitude * tangent1;
                   else
                       friction = testFrictionMagnitude * tangent1;

                   torque = radius * friction.magnitude;
                   if (Vector2.Dot(friction, tangent1) < 0f && torque > 0)
                       torque = -torque;
               }
               else // Kinetic friction case
               {
                   if (relativeVelocity > 0f)
                       friction = -mu * normal.magnitude * tangent1;
                   else
                       friction = mu * normal.magnitude * tangent1;

                   torque = radius * friction.magnitude;
                   if (Vector2.Dot(friction, tangent1) < 0f && torque > 0)
                       torque = -torque;                   
               }
           }
           else // Over threshold
           {
               if (relativeVelocity > 0f)
                   friction = -mu * normal.magnitude * tangent1;
               else
                   friction = mu * normal.magnitude * tangent1;
               torque = radius * friction.magnitude;
               if (Vector2.Dot(friction, tangent1) < 0f && torque > 0)
                   torque = -torque;               
           }
       }
       else // Tangential velocity is negligible
       {

           velocity1 = new Vector2(0f, 0f); // Project onto tangent
           friction = Vector2.zero;
           torque = 0f;
           omega1 = 0f;
       }       
   }
    private void HandleNoCollision(float dt)
    {
        normal = Vector2.zero;
        friction = Vector2.zero;
        torque = 0f;
        //omega = Mathf.Lerp(omega, 0f, 5f * dt); // Gradually reduce angular velocity
    }

    private void ApplyForces(Vector2 netForce, float dt)
    {
        // Update linear velocity and position
        acc = netForce / mass;
        velocity += acc * dt;
        pos += velocity * dt + 0.5f * acc * dt * dt;

        // Interpolate angular velocity (omega)
        float angularAcc = torque / MOI;
        float targetOmega = omega + angularAcc * dt * 8f; // Calculate target omega
        omega = Mathf.Lerp(omega, targetOmega, 0.1f); // Interpolate towards target omega

        // Update rotation
        theta += omega * dt;

        // Apply changes to the wheel transform
        Wheel.localPosition = pos;
        Wheel.localRotation = Quaternion.Euler(0, 0, theta * 180f / Mathf.PI);
    }
    private void ResolvePenetration(float penetrationDepth, Vector2 penetrationDirection)
    {
        if (penetrationDepth > 0)
        {
            transform.position += (Vector3)(penetrationDepth * penetrationDirection);
        }
    }
}