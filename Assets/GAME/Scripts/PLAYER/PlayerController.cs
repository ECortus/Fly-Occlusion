using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AppsFlyerSDK;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class PlayerController : MonoBehaviour
{
    [Inject] public static PlayerController Instance { get; private set; }

    public AircraftEngine engine;
    [SerializeField] private LaunchPower launchPower;
    [Space] 
    [SerializeField] private GameObject countersUI;
    
    public Action OnRepair { get; set; }
    public void Repair() => OnRepair?.Invoke();
    
    public Action OnCrash { get; set; }
    public void Crash() => OnCrash?.Invoke();
    
    public bool Launched { get; set; }
    public int Multiplier { get; private set; }
    public void SetMultiplier(int mult) => Multiplier = mult;
    
    public Transform Follow { get; private set; }

    public Vector3 Forward => -transform.forward;
    public Vector3 Down => -transform.up;

    public Rigidbody Body => engine.Body;
    public Vector3 Center => engine.Center;
    public void TakeControl() => AircraftEngine.SetControl(true);
    public void OffControl() => AircraftEngine.SetControl(false);
    
    public float GetDistanceToGround() => engine.GetDistanceToGround();
    
    [Inject] void Awake()
    {
        Instance = this;
        Follow = transform.Find("follow");
        
        OnCrash += PlayerCrash;
        OnRepair += PlayerRepair;

        // GameManager.OnGameStart += ConnectedParts.CalculatePosRot;
        GameManager.OnGameStart += OnGameStart;
        
        GameManager.OnMergeGame += TakeControl;
        GameManager.OnMergeGame += ResetBody;
        GameManager.OnMergeGame += Repair;

        GameManager.OnGameFinish += Crash;
        GameManager.OnGameFinish += TakeControl;
        
        countersUI.SetActive(false);
    }

    public async void PlayBarrelRoll(float time)
    {
        if (GameManager.Instance.GameStarted && Launched && !engine.onGround && !AircraftEngine.BlockRotation)
        {
            AircraftEngine.BlockRotation = true;
            ResetMouse();

            await engine.MakeBarrelRoll(time);
            
            ResetMouse();
            AircraftEngine.BlockRotation = false;
        }
    }
    
    public void AccelerateForwardForTime(float speed, float time)
    {
        _accelerationCoroutine ??= StartCoroutine(AccelerateForTime(speed, time));
    }

    void StopAccelerateForwardForTime()
    {
        if (_accelerationCoroutine != null)
        {
            StopCoroutine(_accelerationCoroutine);
            _accelerationCoroutine = null;
        }
    }

    private Coroutine _accelerationCoroutine;
    IEnumerator AccelerateForTime(float speed, float limit)
    {
        float time = limit;

        while (time > 0)
        {
            engine.AccelerateBody(speed);
            time -= Time.fixedDeltaTime;
            yield return null;
        }

        _accelerationCoroutine = null;
    }

    public void Launch(float percent = 1f, float angle = 0f)
    {
        Launched = true;
        OffControl();
        
        engine.inputPlaneRotate = 0;
        startMousePosition = Vector3.positiveInfinity;

        Vector3 direction = DirectionFromAngle(180f, angle);
        // Vector3 direction = Forward;
        direction.y = 0;
        engine.AccelerateBody(launchPower.Power, true, percent);
        
        countersUI.SetActive(true);
    }

    private Vector3 startMousePosition, currentMousePosition;
    public float mouseRotateInput { get; private set; }
    
    [SerializeField] private float mouseLength = 6f;
    [SerializeField] private float sphereRadius = 6f;

    [SerializeField] private PartDustParticle dustParticle;

    [Header("DEBUG")]
    public List<Part> Parts;
    public ConnectedParts.DefaultTransformValue[] DefaultValues;

    void FixedUpdate()
    {
        Parts = ConnectedParts.Instance.List;
        DefaultValues = ConnectedParts.Instance.DefaultTransforms;
        
        if (AircraftEngine.BlockRotation) return;
        
        if (GameManager.Instance.GameStarted && Launched)
        {
            // if (Body.velocity.z < 0.05f && engine.onGround)
            // {
            //     ForceFinish();
            //     return;
            // }

            if (GameManager.Instance.FlyLength > 25f && !Tutorial.Instance.PlaneIterationComplete)
            {
                Tutorial.Instance.PlaneIteration();
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                startMousePosition = Input.mousePosition;
                return;
            }
            
            if (Input.GetMouseButton(0) && startMousePosition != Vector3.zero)
            {
                if (AircraftEngine.BlockRotation)
                {
                    ResetMouse();
                }
                else
                {
                    currentMousePosition = Input.mousePosition;
                    mouseRotateInput = (currentMousePosition.x - startMousePosition.x) / 100f / mouseLength;

                    engine.inputPlaneRotate = Mathf.Clamp(mouseRotateInput, -1f, 1f);
                }
                
                engine.setMotor(2);
                return;
            }
            
            // if (Input.GetMouseButtonUp(0))
            // {
            //     ResetMouse();
            //     engine.setMotor(0);
            // }
            
            ResetMouse();
            engine.setMotor(0);
        }
        else
        {
            ResetMouse();
            engine.setMotor(0);
        }
    }

    public void ForceFinish(string cause = "")
    {
        foreach (var VARIABLE in Parts)
        {
            VARIABLE.SetActions(false);
        }
        
        GameManager.Instance.FinishGame(cause == "" ? "LAND" : cause);
    }

    void ResetMouse()
    {
        mouseRotateInput = 0;
        engine.inputPlaneRotate = 0;
        
        startMousePosition = Vector3.zero;
        currentMousePosition = Vector3.zero;
    }
    
    public void SpawnToPos(Vector3 pos)
    {
        Body.transform.position = pos;
        engine.SetVelocity(Vector3.zero);
        ResetBody();
    }

    public void CorrectSphereColliderCenter()
    {
        engine.Sphere.radius = sphereRadius;
        
        Vector3 center = new Vector3(0f, -0.5f, 0f);

        GridCell[] cells = PlayerGrid.Instance._cells;
        
        int size = PlayerGrid.Instance.Size;

        int lastrow = 0;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if (cells[i * 5 + j].Part)
                {
                    lastrow = i;
                }
            }
        }

        center.y += (lastrow - 2) * -1.5f + (engine.Sphere.radius - 0.5f);
        // center.z = ConnectedParts.BalanceCenter;

        // float z = (2 - ConnectedParts.BalanceCenter) * ConnectedParts.Balance;
        // center.z = z + ConnectedParts.BalanceCenter;
        
        // HERE CORRECT BY X ANGLE
        float anglex, posy, posz, dustZ;
        Calculate_Correction(out anglex, out posy, out posz, out dustZ);
        
        AircraftEngine.BalanceAngleX = anglex;
        center.y += posy;
        center.z += posz;
        
        dustParticle.gameObject.SetActive(anglex != 0);

        dustParticle.transform.localPosition = new Vector3(
            0,
            -1,
            dustZ);
        
        
        engine.Sphere.center = center;

        // if (Mathf.Abs(anglex) == 21 || Mathf.Abs(anglex) == 14)
        // {
        //     Body.position = new Vector3(
        //         Body.position.x,
        //         0,
        //         Body.position.z);
        //     transform.position = new Vector3(
        //         transform.position.x,
        //         0,
        //         transform.position.z);
        // }
        
        // Debug.Log("Balance - " + ConnectedParts.Balance);
        // Debug.Log("Center - " + ConnectedParts.BalanceCenter);
    }

    void Calculate_Correction(out float _angle, out float _y, out float _z, out float _dustZ)
    {
        float angle = 0;
        float y = 0;
        float z = 0;
        float dustZ = 0;

        int mainIndex = PlayerGrid.Instance.MainIndex;
        Part part;

        int index;
        int count = 5;

        int wheelCount = 0;
        int gridsCount = 0;
        int[] gridsIndex = new int[] { -999, -999, -999, -999, -999 };
        int[] wheelsIndex = new int[] { -999, -999, -999, -999, -999 };
        
        for (int i = 0; i < count; i++)
        {
            part = PlayerGrid.Instance.GetByIndex(mainIndex + 2 - i).Part;
            if (part && (part.Type.Category == PartCategory.Grid || part.Type.Category == PartCategory.Cabin))
            {
                index = mainIndex + 2 - i;
                part = PlayerGrid.Instance.GetByIndex(index + 5).Part;

                gridsIndex[i] = 2 - i;
                gridsCount++;
                
                if (part && part.Type.Category == PartCategory.Wheels)
                {
                    wheelsIndex[i] = 2 - i;
                    wheelCount++;
                }
            }
        }

        int min = 999, max = -999;
        foreach (var VARIABLE in gridsIndex)
        {
            if (VARIABLE > -999)
            {
                min = VARIABLE < min ? VARIABLE : min;
                max = VARIABLE > max ? VARIABLE : max;
            }
        }
        z = -(min + max);

        if (wheelCount == 1 && gridsCount > 1)
        {
            int wheel = -999;

            for (int i = 0; i < wheelsIndex.Length; i++)
            {
                if (wheelsIndex[i] != -999)
                {
                    wheel = wheelsIndex[i];
                    break;
                }
            }
            
            if (gridsCount == 2)
            {
                int mod;

                if (Mathf.Abs(min) > Mathf.Abs(max)) mod = min;
                else mod = max;

                mod = (int)(wheel != 0 ? Mathf.Sign(wheel) : -Mathf.Sign(mod));
                
                angle = 21 * mod;
                y += 0;
                z += -1 * mod;
                
                dustZ = z - 3 * (z != 0 ? Mathf.Sign(z) : -Mathf.Sign(mod));
            }
            else if (gridsCount == 3)
            {
                int middle = (max + min) / 2;

                if (middle != wheel)
                {
                    angle = 14 * Mathf.Sign(wheel - middle);
                    y += 0;
                    z += -2 * Mathf.Sign(wheel - middle);
                    
                    dustZ = z - 5 * (z != 0 ? Mathf.Sign(z) : Mathf.Sign(middle));
                }
                else
                {
                    float mod = ConnectedParts.Instance.Balance;
                
                    angle = 21 * Mathf.Sign(mod);
                    y += 0;
                    z += -1 * Mathf.Sign(mod);
                
                    dustZ = z + 4 * Mathf.Sign(mod);
                }
            }
        }
        else
        {
            angle = 0;
            y = 0;
        }
        
        _angle = angle;
        _y = y;
        _z = z;
        _dustZ = dustZ;
    }
    
    void OnGameStart()
    {
        PartDustParticle.On();
        
        // ConnectedParts.CalculatePosRot();
        AircraftEngine.BlockRotation = false;
        
        countersUI.SetActive(false);
        CorrectSphereColliderCenter();
        
        Body.isKinematic = false;
        
        Body.velocity = Vector3.zero;
        Body.angularVelocity = Vector3.zero;
        
        engine.enabled = true;
        OffControl();

        // engine.SetParts(GetComponentsInChildren<Part>());
        
        // Body.velocity = Vector3.zero;
        // Body.angularVelocity = Vector3.zero;
        
        Launched = false;
        
        Multiplier = 1;
    }
    
    void PlayerRepair()
    {
        StopAccelerateForwardForTime();
        
        Body.isKinematic = true;
        engine.enabled = false;
        
        Launched = true;
    }

    void PlayerCrash()
    {
        StopAccelerateForwardForTime();
        
        Body.isKinematic = true;
        engine.enabled = false;

        // engine.Sphere.radius = 0.5f;
        Launched = true;
        
        PartDustParticle.Off();
    }

    public void Stop()
    {
        TakeControl();
        Body.isKinematic = true;
    }

    private void ResetBody()
    {
        engine.Sphere.radius = 0.5f;
        Body.isKinematic = true;
        
        engine.inputPlaneRotate = 0;
        engine.Clear();
        
        engine.SetRotation(Quaternion.Euler(new Vector3(0, 180f, 0)));
    }
    
    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
