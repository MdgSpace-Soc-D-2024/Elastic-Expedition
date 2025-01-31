using UnityEngine;
using System;
using static Unity.Burst.Intrinsics.X86;
using System.Collections.Generic;
using System.Linq.Expressions;

public class Front_wheel : MonoBehaviour
{
    [SerializeField]
    float camerY = 1f;
    [SerializeField] Transform rearWheel;
    [SerializeField] Transform Wheel;
    [SerializeField] Transform MainCam,sprite;
    [SerializeField, Range(0, 1)] float mu = 0.5f, e = 0.5f; // Coefficient of friction (mu) and restitution (e)
    [SerializeField, Range(0,5)] float airdrag=1f;
    [SerializeField] float fixedDistance = 7.2f;
    public float radius = 1.31f, mass = 1f, omega = 0f, fmax = 0f, MOI, pi = Mathf.PI, theta = 0f, torque = 0f,maxVelocity=70f;
    public Vector2 pos, velocity = Vector2.zero, gravity = new Vector2(0f, -9.8f), acc = Vector2.zero,rod_along=Vector2.zero, appliedForce = Vector2.zero;
    [SerializeField] public Vector2 tangent = Vector2.zero, normal = Vector2.zero,netForce = Vector2.zero, friction = Vector2.zero,Netnormal=Vector2.zero;
    [SerializeField] float springConstant = 0f;  // Default spring stiffness (k)
    [SerializeField] float dampingCoefficient = 0f; // Default damping coefficient (b)
    [SerializeField] float minDistance =3.1f;  // Minimum allowed distance between wheels (to prevent overlap)
    [SerializeField] float maxDistance = 7.7f;
    public GameObject rearwheel;
    public Rear_wheel Script_R;
    public RaycastHit2D[] hits;
    private RaycastHit2D[] previousHits = new RaycastHit2D[0];

    private void Start()
    {
        pos = Wheel.position;
        MOI = 0.5f * mass * radius * radius; // Moment of inertia for a solid disk
        rod_along=(Wheel.position-rearWheel.position);
        Script_R = rearwheel.GetComponent<Rear_wheel>();
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;
        appliedForce = Script_R.appliedForce;
        hits = Physics2D.CircleCastAll(Wheel.position, radius, velocity.normalized, 0.01f,LayerMask.GetMask("Terrain"));
        // Check for collision using CircleCast
        List<RaycastHit2D> newHits = new List<RaycastHit2D>();
        foreach (var currentHit in hits)
        {
            bool isNew = true;
            foreach (var previousHit in previousHits)
            {
                // Check if the normal is similar (threshold for similarity) and if they hit the same collider
                if (Vector2.Dot(currentHit.normal.normalized, previousHit.normal.normalized) > 0.99f)
                {
                    isNew = false;
                    break;
                }
            }
            if (isNew)
                newHits.Add(currentHit);
        }
        // Convert new hits list to array for further processing
        RaycastHit2D[] newHitsArray = newHits.ToArray();
        if (hits.Length > 0)
        {
            if (newHitsArray.Length > 0)
                HandleCollision(newHitsArray);            
            else
                HandleCollision(hits);            
        }
        else
            HandleNoCollision(dt);
        
        // Apply net forces and update the physics
        MaintainOscillation(dt);
        netForce =appliedForce+mass * gravity + normal + friction-airdrag*velocity;
        ApplyForces(netForce, dt);
        MainCam.localPosition =(rearWheel.localPosition+Wheel.localPosition)/2f + new Vector3(0f, camerY, -7f);
        sprite.localPosition = (rearWheel.localPosition + Wheel.localPosition) / 2f+new Vector3(0f,3f,0f);
        Vector3 tangent = Wheel.localPosition - rearWheel.localPosition;
        float angle = Mathf.Atan2(tangent.y, tangent.x) * Mathf.Rad2Deg;
        sprite.localRotation = Quaternion.Euler(0,0,angle);
    }

    private void HandleCollision(RaycastHit2D[] hits)
    {
        Netnormal = new Vector2(0f, 0f);
        float netDistance = 0f;
        foreach (var hit in hits)
        {
            Netnormal += hit.normal;
            netDistance += hit.distance;
        }
        Netnormal = Netnormal.normalized;
        Vector2 collisionNormal = Netnormal.normalized;
        Vector2 relativeVelocity = velocity;
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
        normal = (-Vector2.Dot(gravity, collisionNormal) * mass - Vector2.Dot(appliedForce, Netnormal)) * Netnormal;

        if (Math.Abs(velocity.x) > 0.3f)
            HandleFrictionNormally(ref velocity, ref omega, hits[0].point, Netnormal, radius, mu, normal);
    }
    private void HandleFrictionNormally(ref Vector2 velocity1, ref float omega1, Vector2 contactPoint, Vector2 collisionNormal,
        float radius, float mu, Vector2 normal)
    {
        // Compute tangent vector (perpendicular to the collision normal)
        Vector2 tangent1 = new Vector2(collisionNormal.y, -collisionNormal.x).normalized;

        // Calculate net force
        netForce = appliedForce + mass * gravity + normal;
        // Compute friction magnitude
        float testFrictionMagnitude = Vector2.Dot(netForce, tangent) * MOI / (MOI + mass * radius * radius);
        //Debug.Log(testFrictionMagnitude);
        float fmax1 = mu * normal.magnitude;
        fmax = fmax1;
        // Tangential velocity (v + rw)
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
    public void ApplyForces(Vector2 netForce, float dt)
    {
        // Update linear velocity and position
        acc = netForce / mass;
        velocity += acc * dt;      
        if(velocity.magnitude>maxVelocity)
            velocity = velocity.normalized * maxVelocity;          
        pos += velocity * dt + 0.5f * acc * dt * dt;
        // Interpolate angular velocity (omega)
        float angularAcc = torque / MOI;
        float targetOmega = omega + angularAcc * dt * 8f; // Calculate target omega
        omega = Mathf.Lerp(omega, targetOmega, 0.1f); // Interpolate towards target omega
        if(omega>maxVelocity/radius)
            omega = maxVelocity/radius;

        // Update rotation
        theta += omega * dt;

        // Apply changes to the wheel transform
        Wheel.localPosition = pos;
        Wheel.localRotation = Quaternion.Euler(0, 0, theta * 180f / Mathf.PI);
    }
    private void MaintainOscillation(float dt)
    {
        // Get positions of the two wheels
        Vector2 rearWheelPos = rearWheel.position;
        Vector2 frontWheelPos = Wheel.position;
        Vector2 separationVector = frontWheelPos - rearWheelPos;
        float currentDistance = separationVector.magnitude;
        Vector2 direction = separationVector.normalized;
        Debug.Log("currentDistance :" + currentDistance);
        // Check if the distance between wheels is out of the desired range
        if (Math.Abs(currentDistance-7.2f)>0.01f)
        {
            // Calculate spring force (Hooke's Law)
            float springForceMagnitude = -springConstant * (currentDistance - fixedDistance);
            Debug.Log("springConstant :" + springConstant);
            Debug.Log("springForceMagnitude :" + springForceMagnitude);

            // Calculate damping force
            Vector2 relativeVelocity = velocity - Script_R.velocity; // Assuming RearWheel has velocity
            float dampingForceMagnitude = -dampingCoefficient * Vector2.Dot(relativeVelocity, direction);

            // Total force along the direction of the rod
            float totalForceMagnitude = springForceMagnitude + dampingForceMagnitude;

            Debug.Log("totalForceMagnitude :" + totalForceMagnitude);
            // Apply forces to the wheels
            Vector2 springforce = totalForceMagnitude * direction;
            Debug.Log("force : " + springforce);
            // Apply equal and opposite forces to each wheel
            ApplyForces(springforce, dt); // Front wheel force application
            if(hits.Length>0)
            {
                normal+=Math.Abs(Vector2.Dot(springforce,Netnormal))*Netnormal;
            }
            Script_R.ApplyForces(-springforce, dt); // Rear wheel force application
            if(Script_R.hits.Length>0)
            {
                Script_R.normal+=Math.Abs(Vector2.Dot(-springforce,Script_R.Netnormal))*Script_R.Netnormal;
            }          
            // netForce+=force;
            // Script_R.netForce+=-force;
            //normal+=Math.Abs(Vector2.Dot(force,direction))*direction;

        }
        if (currentDistance < minDistance)
        {
            // Correct penetration (if distance is less than minDistance)
            float penetrationDepth = minDistance - currentDistance;
            Vector2 penetrationCorrection = penetrationDepth * direction;

            //// Adjust front and rear wheel positions equally
            frontWheelPos += penetrationCorrection * 0.5f; // Adjust front wheel
            rearWheelPos -= penetrationCorrection * 0.5f;  // Adjust rear wheel

            //// Update positions
            Wheel.position = (Vector3)frontWheelPos;
            rearWheel.position = (Vector3)rearWheelPos;

            // Ensure current distance is reset to minimum distance
            currentDistance = minDistance;
        }
        if (currentDistance > maxDistance)
        {
            // Correct penetration (if distance is less than minDistance)
            float penetrationDepth = maxDistance - currentDistance;
            Vector2 penetrationCorrection = penetrationDepth * direction;

            //// Adjust front and rear wheel positions equally
            frontWheelPos += penetrationCorrection * 0.5f; // Adjust front wheel
            rearWheelPos -= penetrationCorrection * 0.5f;  // Adjust rear wheel

            //// Update positions
            Wheel.position = (Vector3)frontWheelPos;
            rearWheel.position = (Vector3)rearWheelPos;

            //// Ensure current distance is reset to minimum distance
            currentDistance = maxDistance;
        }
    }
    private void ResolvePenetration(float penetrationDepth, Vector2 penetrationDirection)
    {
        if (penetrationDepth > 0)
        {
                transform.position += (Vector3)(penetrationDepth * penetrationDirection);
        }
    }
   
}