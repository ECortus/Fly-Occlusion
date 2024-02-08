using System.Collections;
using System.Collections.Generic;
using ModestTree;
using UnityEngine;
using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro.Examples;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]

[ExecuteInEditMode]
public class AircraftEngine : MonoBehaviour
{
    [field: SerializeField] public Rigidbody Body { get; private set; }
    [field: SerializeField] public SphereCollider Sphere { get; private set; }
    
    public static bool NoBreaking { get; set; }
    public static bool BlockRotation { get; set; }
    
    private static float GroundCheckDistanceDelta = 0.1f;
    private static float GroundCheckSkinWidthDelta = 0.05f;

    public enum GRAVITY_MODE
    {
        AlwaysDown, TowardsGround
    }
    
    public Vector3 Center => transform.position + Sphere.center;
    
    [SerializeField] private LayerMask GroundMask;
    private RaycastHit rayHitGround;

    private static bool _TakeControl { get; set; }
    public static void SetControl(bool take) => _TakeControl = take;
    
    [Header("Physics")]
    public float colliderRadius = 2f;
    public float bodyMass = 1f;
    public GRAVITY_MODE gravityMode = GRAVITY_MODE.TowardsGround;
    public float gravityVelocity = 80;
    public float maxGravity = 50;
    public float gravityMultiplierOnMotor = 0.8f;
    public bool applyBalanceSlope = true;
    public float slopeAngleBalance = 15f;

    [Space] 
    public float maxPlaneAngle = 50f;
    public float maxSlopeAngle = 50f;
    public float rotateVelocity = 45f;
    public float rotateForce = 35f;
    public Vector3 globalRotateDeltaOnGround;
    public Vector3 globalRotateDeltaOffGround;
    public float planeRotateMultiplier = 1.2f;
    public float planeRotateBackMultiplier = 0.75f;
    
    [Space]
    public LayerMask collidableLayers = ~0;
    public bool adjustToScale = false;
    
    [Header("Engine")]
    public AnimationCurve accelerationCurve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 0) });
    public float passiveAccelerationForward = 100;
    public float maxAccelerationForward = 100;
    public float maxPassiveSpeedForward = 40;
    public float maxSpeedForward = 40;
    public float maxAccelerationReverse = 50;
    public float boostMultiplierOnGround = 1.5f;
    
    [Space]
    public float maxSpeedReverse = 20;
    public float brakeStrength = 200;
    
    [Space]
    public float slopeFriction = 1f;
    public float forwardFriction = 40;
    public float lateralFriction = 80;

    [Header("DEBUG: ")]
    public bool onGround = false;
    private Vector3 crossForward = Vector3.zero;
    private Vector3 crossUp = Vector3.zero;
    private Vector3 crossRight = Vector3.zero;
    private bool hitSide = false;
    private float hitSideForce = 0;
    private float hitSideMass = 0;
    private float hitGroundMass = 0;
    private Vector3 hitSidePosition = Vector3.zero;
    private bool hitGround = false;
    private float hitGroundForce = 0;
    private bool hitSideStayStatic = false;
    private bool hitSideStayDynamic = false;
    private float groundVelocity = 0;
    private float forwardVelocity = 0;
    private float rightVelocity = 0;
    private Vector3 gravityDirection = Vector3.zero;
    private float inputSteering = 0;
    private float inputMotor = 0;
    public float inputPlaneRotate;
    private PhysicMaterial customPhysicMaterial;
    private Quaternion bodyVelocityLook;
    private float slopeDelta = 0;
    private float boostMultiplier = 1;
    private float averageScale = 1;
    private float realColliderRadius = 0;
    private float scaleAdjustment = 1;
    private float cubicScale = 1;
    private float inverseScaleAdjustment = 1;

    // private ConnectedParts.DefaultTransformValue[] defaultPartsValue => ConnectedParts.DefaultTransforms;
    private static Vector3 boostDirection => -ConnectedParts.Instance.BoostDirection * ConnectedParts.Instance.BoostModificator;
    private static Vector3 accelerationDirection => -ConnectedParts.Instance.AccelerationDirection * ConnectedParts.Instance.AccelerationModificator;
    private static Vector3 planeDirection => ConnectedParts.Instance.PlaneDirection * ConnectedParts.Instance.PlaneModificator;

    private static bool InAccelerationZone;
    private static float AccelerationZoneSpeed;
    
    public static float BalanceAngleX { get; set; }

    public static void EnterAccelerationZone(float speed)
    {
        AccelerationZoneSpeed = speed * Mathf.Abs(accelerationDirection.z);
        InAccelerationZone = true;
    }
    
    public static void ExitAccelerationZone()
    {
        InAccelerationZone = false;
        AccelerationZoneSpeed = 0;
    }

    public void Clear()
    {
        // ConnectedParts.Clear();
    }
    
    void Start()
    {
        customPhysicMaterial = new PhysicMaterial();
        customPhysicMaterial.bounciness = 0;
        customPhysicMaterial.bounceCombine = PhysicMaterialCombine.Minimum;
        customPhysicMaterial.staticFriction = 0;
        customPhysicMaterial.dynamicFriction = 0;
        customPhysicMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
    
        if (!Application.isPlaying) return;
    
        crossForward = transform.forward;
        crossRight = transform.right;
        crossUp = transform.up;

        NoBreaking = false;
        BlockRotation = false;
    }

    public void AccelerateBody(float speed, bool resetrot = false, float percentForce = 1)
    {
        // SetRotation(resetrot ? Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0) : transform.rotation);
        // SetRotation(resetrot ? Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0) : transform.rotation);
        
        // Vector3 dir = -crossForward;
        // dir.y = 0;

        // Debug.Log(dir);
        
        // Vector3 forceAccelerationDirection = dir;
        float forceAccelerationSpeed = speed * Mathf.Abs(accelerationDirection.z) * percentForce;

        // Vector3 velocity = Body.velocity;
        // velocity += ForceAccelerationDirection * ForceAccelerationSpeed * Time.fixedDeltaTime;
        // SetVelocity(velocity);

        ForceAccelerationBody = forceAccelerationSpeed;
    }

    private float ForceAccelerationBody;

    public async UniTask MakeBarrelRoll(float time)
    {
        Quaternion rot = Quaternion.Euler(0, 180f, 0);
        SetRotation(rot);

        Body.freezeRotation = true;
        Body.angularVelocity = Vector3.zero;

        transform.DORotate(new Vector3(0, 0, -360f), time * 2f, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear);
        await Body.DORotate(new Vector3(0, 0, -360f), time * 2f, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .AsyncWaitForCompletion();
        
        Body.angularVelocity = Vector3.zero;
        Body.freezeRotation = false;
        
        SetRotation(rot);
    }
    
    void Update()
    {
        // refresh rigid body and collider parameters
        
        //body.hideFlags = HideFlags.NotEditable;
        //sphereCollider.hideFlags = HideFlags.NotEditable;
    
        // if (_TakeControl) return;
    
        Body.mass = bodyMass * (adjustToScale ? cubicScale : 1);
        Body.drag = 0;
        Body.angularDrag = 1f;
        // Body.constraints = RigidbodyConstraints.FreezeRotation;
        // Body.useGravity = false;
        // Body.isKinematic = false;
        // Body.interpolation = RigidbodyInterpolation.Extrapolate;
        // Body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    
        // Sphere.radius = colliderRadius;
        // Sphere.isTrigger = false;
        Sphere.material = customPhysicMaterial;
        
        if (!Application.isPlaying) return;
    }
    
    void FixedUpdate()
    {
        if (_TakeControl)
        {
            SetVelocity(Vector3.zero);
            return;
        }
        
        #region FirstPart    
        
        averageScale = (transform.lossyScale.x + transform.lossyScale.y + transform.lossyScale.z) / 3f;
        realColliderRadius = Sphere.radius * averageScale;
    
        float deltaTime = Time.fixedDeltaTime;
        float motor = inputMotor;
        float groundCheckSkinWidth = realColliderRadius * GroundCheckSkinWidthDelta;
        float groundCheckDistance = realColliderRadius * GroundCheckDistanceDelta;
    
        //// basic ground check
        onGround = false;
        crossUp = transform.up;
        
        RaycastHit hitSphere;
        if (Physics.SphereCast(
                Sphere.bounds.center, realColliderRadius - groundCheckSkinWidth, Vector3.down, 
                out hitSphere, groundCheckDistance + groundCheckSkinWidth, collidableLayers, QueryTriggerInteraction.Ignore))
        {
            crossUp = hitSphere.normal;
            if (Vector3.Angle(-crossUp, Vector3.up) <= maxSlopeAngle)
            {
                onGround = true;
            }
        }
    
        //// get average ground direction from corners (forward-left, forward-right, back-left, back-right)
        Vector3[] groundCheckSource = new Vector3[]{
            Sphere.bounds.center + Vector3.forward * realColliderRadius + Vector3.left * realColliderRadius,
            Sphere.bounds.center + Vector3.forward * realColliderRadius + Vector3.right * realColliderRadius,
            Sphere.bounds.center + Vector3.back * realColliderRadius + Vector3.left * realColliderRadius,
            Sphere.bounds.center + Vector3.back * realColliderRadius + Vector3.right * realColliderRadius
        };
    
        Vector3[] groundCheckHits = new Vector3[groundCheckSource.Length];
        bool[] groundCheckFound = new bool[groundCheckSource.Length];
        
        RaycastHit rayHit;
        for (int i = 0; i < groundCheckSource.Length; i++)
        {
            groundCheckFound[i] = Physics.Raycast(
                groundCheckSource[i], Vector3.down, out rayHit, realColliderRadius * 2, 
                collidableLayers, QueryTriggerInteraction.Ignore);
            if (groundCheckFound[i])
            {
                groundCheckHits[i] = rayHit.point;
            }
        }
    
        //// append the calculated corner normals to the center normal
        Vector3 triFRNormal = Vector3.zero;
        if (groundCheckFound[0] && groundCheckFound[1] && groundCheckFound[2])
        {
            triFRNormal = getTriangleNormal(groundCheckHits[0], groundCheckHits[1], groundCheckHits[2]);
        }
    
        Vector3 triBLNormal = Vector3.zero;
        if (groundCheckFound[1] && groundCheckFound[3] && groundCheckFound[2])
        {
            triBLNormal = getTriangleNormal(groundCheckHits[1], groundCheckHits[3], groundCheckHits[2]);
        }
    
        crossUp = (crossUp + triFRNormal + triBLNormal).normalized;
    
        //// calculate ground rotation
        Vector3 velocity = Body.velocity;
        groundVelocity = (velocity - Vector3.up * velocity.y).magnitude;
        crossForward = Vector3.Cross(-crossUp, transform.right);
        crossRight = Vector3.Cross(-crossUp, transform.forward);

        forwardVelocity = Vector3.Dot(velocity, crossForward);
        rightVelocity = Vector3.Dot(velocity, crossRight);
    
        Quaternion groundRotation = Quaternion.LookRotation(crossForward, crossUp);

        float groundXAngle, groundYAngle, groundZAngle, targetX = 0, targetY = 0, targetZ = 0;
        Vector3 globalRotateDelta;

        if (!PlayerController.Instance.Launched)
        {
            globalRotateDelta = globalRotateDeltaOnGround / 9;
        }
        else if (onGround)
        {
            globalRotateDelta = globalRotateDeltaOnGround;
        }
        else
        {
            globalRotateDelta = globalRotateDeltaOffGround;
        }

        #endregion        
        
        float signX = Mathf.Sign(180f - Body.rotation.eulerAngles.y);
        groundXAngle = Body.rotation.eulerAngles.x >= 180f
            ? -(360f - Body.rotation.eulerAngles.x)
            : Body.rotation.eulerAngles.x;

        float signY = Mathf.Sign(180f - Body.rotation.eulerAngles.y);
        groundYAngle = Body.rotation.eulerAngles.y >= 180f
            ? 360f - Body.rotation.eulerAngles.y
            : Body.rotation.eulerAngles.y;
        
        groundZAngle = groundRotation.eulerAngles.z >= 180f
            ? -(360f - groundRotation.eulerAngles.z)
            : groundRotation.eulerAngles.z;

        if (PlayerController.Instance.Launched)
        {
            Vector3 rotVel = Body.velocity;
            rotVel.y = 0;
            bodyVelocityLook = Quaternion.LookRotation(rotVel.normalized);

            if (onGround)
            {
                targetX = Mathf.Abs(groundRotation.eulerAngles.x);
            }
            else
            {
                // targetX = -slopeAngleBalance * ConnectedParts.Balance;
                targetX = 0;
            }

            if (onGround)
            {
                groundXAngle = targetX + BalanceAngleX * (InAccelerationZone ? 0 : 1);
            }
            else
            {
                groundXAngle = Mathf.MoveTowards(groundXAngle,
                    targetX * signX + BalanceAngleX,
                    globalRotateDelta.x * deltaTime);
            }
            
            targetY = bodyVelocityLook.eulerAngles.y >= 180f
                ? -(360f - bodyVelocityLook.eulerAngles.y)
                : bodyVelocityLook.eulerAngles.y;
            targetY = onGround ? targetY + 180f : 180f;
            groundYAngle = Mathf.MoveTowards((groundYAngle * signY + 360f) % 360f,
                targetY % 360,
                globalRotateDelta.y * deltaTime);
            // Debug.Log(signY + ", " + targetY % 360 + ", " + groundYAngle);
            
            targetZ = !onGround ? inputPlaneRotate * maxPlaneAngle : 0f;
            groundZAngle = Mathf.MoveTowards(groundZAngle, 
                targetZ, 
                globalRotateDelta.z * deltaTime);
        }
        else
        {
            targetX = 0;
            // groundXAngle = BalanceAngleX;
            
            groundXAngle = Mathf.MoveTowards(groundXAngle,
                targetX * signX + BalanceAngleX,
                globalRotateDelta.x * deltaTime);
            
            // groundYAngle = LaunchController.Rotate.eulerAngles.y;
            targetY = LaunchController.Rotate.eulerAngles.y;
            groundYAngle = Mathf.Lerp((groundYAngle * signY + 360f) % 360f,
                targetY % 360,
                globalRotateDelta.y * deltaTime);
            
            groundZAngle = 0;
        }
        
        // targetX += BalanceAngleX;
        
        // if (InAccelerationZone)
        // {
        //     groundXAngle = targetX + BalanceAngleX;
        // }
        // else
        // {
        //     groundXAngle = Mathf.MoveTowards(groundXAngle,
        //         targetX * signX + BalanceAngleX,
        //         globalRotateDelta.x * deltaTime);
        // }

        Quaternion rotation = Quaternion.Euler(groundXAngle, groundYAngle, groundZAngle);
        
        if (!BlockRotation)
        {
            SetRotation(rotation);
        }
        
        // Debug.Log(Vector3.Dot(-Vector3.forward, planeDirection));
        float planeMod = Vector3.Dot(-Vector3.forward, planeDirection);
        // Debug.Log("Plane mod - " + (1f + planeMod));
        
        gravityDirection = Vector3.zero;
        float gravityMultiplier = (getMotor() > 0 ? gravityMultiplierOnMotor : 1f) 
                                  // * ConnectedParts.Instance.GravityRelativity
                                  * (PlayerController.Instance.Launched ? 1f : 10f)
                                  * (1f + planeMod);

        gravityMultiplier *= PartUnlocked.Wheels ? (PartUnlocked.Wings ? 1f : 1.2f) : 1.35f;
        
        switch (gravityMode)
        {
            case GRAVITY_MODE.AlwaysDown:
                gravityDirection = Vector3.down;
                break;
            case GRAVITY_MODE.TowardsGround:
                gravityDirection = onGround ? -crossUp : Vector3.down;
                break;
        }
        
        float adjustedMaxGravity = maxGravity * gravityMultiplier;
        float adjustedGravityVelocity = gravityVelocity * gravityMultiplier;
        
        Vector3 gravity = gravityDirection * Mathf.Min(Mathf.Max(0, adjustedMaxGravity + velocity.y),
            adjustedGravityVelocity * deltaTime);
        gravity = Quaternion.AngleAxis(BalanceAngleX, Vector3.up) * gravity;
        // Debug.DrawLine(transform.position, transform.position + gravity * 999f, Color.blue);
        velocity += gravity;

        Vector3 motorDirection = -crossForward + boostDirection;
        motorDirection.x = 0;
        motorDirection.y = 0;
        motorDirection.z = Mathf.Abs(motorDirection.z);
        // motorDirection = Quaternion.AngleAxis(BalanceAngleX, Vector3.up) * motorDirection;
        Debug.DrawLine(transform.position, transform.position + motorDirection * 999f, Color.magenta);
        
        if (PlayerController.Instance.Launched)
        {
            if (!onGround && !InAccelerationZone)
            {
                float motorForce = maxPassiveSpeedForward * (onGround ? boostMultiplierOnGround : 1f);
            
                if (velocity.z < maxPassiveSpeedForward)
                {
                    velocity += motorDirection * motorForce * deltaTime;
                    velocity.z = Mathf.MoveTowards(
                        velocity.z, 
                        maxPassiveSpeedForward * Mathf.Clamp(boostDirection.magnitude, 1, 9999f), 
                        12 * deltaTime);
                }
                else
                {
                    if (getMotor() > 0)
                    {
                        motorForce = maxAccelerationForward * (onGround ? boostMultiplierOnGround : 1f);
            
                        velocity += motorDirection * motorForce * deltaTime;
                        velocity.z = Mathf.Min(
                            velocity.z, 
                            maxSpeedForward * Mathf.Clamp(boostDirection.magnitude, 1, 9999f));
                    }
                    else
                    {
                        velocity.z = Mathf.MoveTowards(velocity.z, 
                            maxPassiveSpeedForward * Mathf.Clamp(boostDirection.magnitude, 1, 9999f), 
                            12 * deltaTime);
                    }
                }
                
                // Debug.Log(velocity.z);
            }
            
            if (AccelerationZoneSpeed > 0.05f)
            {
                velocity += motorDirection * AccelerationZoneSpeed * deltaTime;
                AccelerationZoneSpeed = Mathf.Lerp(AccelerationZoneSpeed, 0, 50f * deltaTime);
            }

            if (ForceAccelerationBody > 0.05f)
            {
                velocity += motorDirection * ForceAccelerationBody * deltaTime;
                ForceAccelerationBody = Mathf.Lerp(ForceAccelerationBody, 0, 400f * deltaTime);
            }
            
            float rotateMultiplier = Mathf.Abs(inputPlaneRotate) > 0.05f ? planeRotateMultiplier : planeRotateBackMultiplier;
            float rotateDelta = rotateForce * rotateMultiplier * deltaTime;

            if (onGround)
            {
                if (!NoBreaking)
                {
                    // Debug.Log($"Before stopping - {velocity}");
                    velocity -= crossForward * Mathf.Sign(forwardVelocity) * Mathf.Min(Mathf.Abs(forwardVelocity), 
                        forwardFriction * scaleAdjustment * deltaTime) * brakeStrength;
                    
                    velocity.x = Mathf.Lerp(velocity.x, 0, rotateDelta * 3);
                    // Debug.Log($"After stopping - {velocity}");
                }
            }
            else
            {
                velocity.x = Mathf.Lerp(velocity.x, inputPlaneRotate * rotateVelocity, rotateDelta);
            }
        }
        else
        {
            velocity.x = velocity.z = 0;
            if (onGround) velocity.y = 0;
        }

        SetVelocity(velocity);
        
        SetVisuals(getMotor() > 0);
        
        // Debug.Log("Velocity - " + velocity);
        // OnDebug();
    }

    void OnDebug()
    {
        Debug.DrawRay(transform.position, boostDirection * 25f, Color.magenta);
        Debug.DrawRay(transform.position, accelerationDirection * 50f, Color.green);
        Debug.DrawRay(transform.position, gravityDirection * Vector3.Dot(-Vector3.forward, planeDirection) * 250f, Color.black);
        
        Debug.Log("Boost Direction " + boostDirection + ", acceleration direction " + accelerationDirection + ", plane direction - " + planeDirection);
    }

    public void SetVelocity(Vector3 velocity)
    {
        Body.velocity = velocity;
        
        Part part;
        ConnectedParts.DefaultTransformValue defaultValue;

        ConnectedParts.DefaultTransformValue[] values = ConnectedParts.Instance.DefaultTransforms;
        if (values == null) return;
        
        for (int i = 0; i < values.Length; i++)
        {
            part = values[i].Part;
            
            if (part)
            {
                defaultValue = values[i];
                
                part.Body.velocity = velocity;
                part.SetLocalPosition(defaultValue.Position);
            }
        }
    }

    void SetVisuals(bool state)
    {
        if (ConnectedParts.Instance.DefaultTransforms == null) return;

        Part part;
        for(int i = 0; i < ConnectedParts.Instance.DefaultTransforms.Length; i++)
        {
            part = ConnectedParts.Instance.DefaultTransforms[i].Part;
            if (part)
            {
                part.VisualMode = state;
            }
        }
    }

    public void SetRotation(Quaternion rotation)
    {
        if (PlayerController.Instance.Launched)
        {
            Body.rotation = rotation;
            transform.rotation = Body.rotation;
        }
        else
        {
            transform.rotation = rotation;
            Body.rotation = transform.rotation;
        }

        Part part;
        ConnectedParts.DefaultTransformValue defaultValue;
        
        ConnectedParts.DefaultTransformValue[] values = ConnectedParts.Instance.DefaultTransforms;
        if (values == null) return;
        
        for (int i = 0; i < values.Length; i++)
        {
            part = values[i].Part;
            
            if (part)
            {
                defaultValue = values[i];

                part.SetLocalRotation(defaultValue.Rotation);

                // Debug.Log($"----------------{part.name}----------------");
                // Debug.Log(defaultPosition[i]);
                // Debug.Log(velocity);
            }
        }
    }
    
    void OnDestroy()
    {
        if (Body != null) Body.hideFlags = HideFlags.None;
        if (Sphere != null) Sphere.hideFlags = HideFlags.None;
    }
    
    public float GetDistanceToGround()
    {
        Physics.Raycast(Center, Vector3.down * 9999f, out rayHitGround, 9999f, GroundMask);

        if (rayHitGround.transform)
        {
            return (Center - rayHitGround.point).magnitude;
        }
        
        return 0f;
    }
    
    private float getAcceleration(float accelerationMultiplier = 1f, float speedMultiplier = 1f)
    {
        if (getMotor() == 0 || speedMultiplier == 0)
        {
            return brakeStrength;
        }
        else
        {
            float accelerationCurveDelta = Mathf.Abs(forwardVelocity) / (getMaxSpeed() * speedMultiplier);
            if (accelerationCurveDelta > 1) return -getMaxAcceleration() * accelerationMultiplier;
            if (accelerationCurveDelta < 0) return getMaxAcceleration() * accelerationMultiplier;
            return accelerationCurve.Evaluate(accelerationCurveDelta) * getMaxAcceleration() * accelerationMultiplier;
        }
    }
    
    private int getZeroSign(float value)
    {
        if (value == 0) return 0;
        else return (int)Mathf.Sign(value);
    }
    
    public int getVelocityDirection()
    {
        return getZeroSign(forwardVelocity);
    }

    public float getMaxSpeed()
    {
        return getVelocityDirection() >= 0 ? maxSpeedForward * scaleAdjustment : maxSpeedReverse * scaleAdjustment;
    }

    public float getMaxAcceleration()
    {
        return getVelocityDirection() >= 0 ? maxAccelerationForward * scaleAdjustment : maxAccelerationReverse * scaleAdjustment;
    }

    public void setMotor(int mtr) => inputMotor = mtr;
    public float getMotor() => inputMotor;
    
    private Vector3 getTriangleNormal(Vector3 pa, Vector3 pb, Vector3 pc)
    {
        Vector3 u = pb - pa;
        Vector3 v = pc - pa;

        return new Vector3(u.y * v.z - u.z * v.y, u.z * v.x - u.x * v.z, u.x * v.y - u.y * v.x).normalized;
    }
}
