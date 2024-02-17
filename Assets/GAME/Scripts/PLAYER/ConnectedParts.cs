using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Zenject;

public class ConnectedParts : MonoBehaviour
{
    [Inject] public static ConnectedParts Instance { get; private set; }

    [Inject] private void Awake()
    {
        Instance = this;
    }

    public static Action OnUpdate;
    
    public List<Part> List = new List<Part>();
    
    private PartType _cabins, _grids, _fans, _wheels, _wings;
    private GridsUploads _gridsUploads;

    private readonly Vector3 DefaultBoostDirection = new Vector3(0, 0, -1);
    private readonly Vector3 DefaultAccelerationDirection = new Vector3(0, 0, -1);
    private readonly Vector3 DefaultPlaneDirection = new Vector3(0, 0, 1);
    
    public Vector3 BoostDirection = new Vector3(0, 0, -1);
    public Vector3 AccelerationDirection = new Vector3(0, 0, -1);
    public Vector3 PlaneDirection = new Vector3(0, 0, 1);

    private const float DefaultBoost = 0.1f;
    private const float DefaultAcceleration = 1f;
    private const float DefaultPlane = 0.05f;

    public float BoostModificator = 0.1f;
    public float AccelerationModificator = 1f;
    public float PlaneModificator = 0.05f;
    public float BalanceCenter = 0;
    public float Balance = 0;
    public float Mass = 0f;

    private const float PlaneParameterRelativity = 2.5f;

    public float MaxBoostModificator
    {
        get
        {
            if (!_fans) _fans = Resources.Load<PartType>("PARTS/Boost");
            return _fans.GetPart(4).GetFlyParameters().Force + DefaultBoost;
        }
    }

    public float MaxDistanceModificator
    {
        get
        {
            if (!_wheels) _wheels = Resources.Load<PartType>("PARTS/Wheels");
            if (!_wings) _wings = Resources.Load<PartType>("PARTS/Wings");
            
            // return (_wheels.GetPart(4).GetFlyParameters().Force + DefaultAcceleration) / PlaneParameterRelativity
            //        + (_wings.GetPart(4).GetFlyParameters().Force + DefaultPlane) * PlaneParameterRelativity;

            Part wheel = _wheels.GetPart(4);
            Part wings = _wings.GetPart(4);
            
            float wheelMax = wheel.GetFlyParameters().Force * 1f * 2f +
                             wheel.GetFlyParameters().Force * 0.5f * 2f +
                             wheel.GetFlyParameters().Force * 0.25f * 2f + DefaultAcceleration;
            float wingMax = wings.GetFlyParameters().Force + DefaultPlane;
            
            // Debug.Log("Wheels max - " + wheelMax + ", wings max - " + wingMax);

            return DistanceFormula(wingMax, wheelMax);
        }
    }

    public float DistanceFormula(float plane, float wheel)
    {
        return Mathf.Pow(Mathf.Pow(plane * PlaneParameterRelativity, 2) + Mathf.Pow(wheel / PlaneParameterRelativity, 2), 1/2f);
    }
    
    public float MaxMass
    {
        get
        {
            if (!_cabins) _cabins = Resources.Load<PartType>("PARTS/Cabin");
            if (!_grids) _grids = Resources.Load<PartType>("PARTS/Grid");
            if (!_fans) _fans = Resources.Load<PartType>("PARTS/Boost");
            if (!_wheels) _wheels = Resources.Load<PartType>("PARTS/Wheels");
            if (!_wings) _wings = Resources.Load<PartType>("PARTS/Wings");

            int max = 4;
            
            float cabin = _cabins.GetPart(max).Mass;
            float grid = _grids.GetPart(max).Mass;
            float fan = _fans.GetPart(max).Mass;
            float wheels = _wheels.GetPart(max).Mass;
            float wings = _wings.GetPart(max).Mass;
            
            return cabin * 1 + grid * 2 + Mathf.Max(new [] { fan, wheels, wings }) * 4 + wheels * 3 + wings * 3;
        }
    }

    public float GravityRelativity
    {
        get
        {
            if (!_cabins) _cabins = Resources.Load<PartType>("PARTS/Cabin");
            return Mathf.Pow(Mathf.Clamp(Mass, _cabins.GetPart(0).Mass, 999f) / MaxMass, 1/100f);
        }
    }

    public void Add(Part part)
    {
        if (!List.Contains(part))
        {
            List.Add(part);
            // Debug.Log("Added - " + part);
        }
        else
        {
            // Debug.Log("Updated - " + part);
        }
        
        ParametersModifier mod = part.GetFlyParameters();
        CalculateParameters(mod);
    }
    
    public void Remove(Part part)
    {
        if (List.Contains(part))
        {
            List.Remove(part);
            ParametersModifier mod = part.GetFlyParameters();
            CalculateParameters(mod);
            
            // Debug.Log("Removed - " + part);
        }
    }

    public void Clear()
    {
        List.Clear();

        BoostModificator = DefaultBoost;
        AccelerationModificator = DefaultAcceleration;
        PlaneModificator = DefaultPlane;
        Mass = 0;
        BalanceCenter = 0;
        Balance = 0;
        
        BoostDirection = DefaultBoostDirection;
        AccelerationDirection = DefaultAccelerationDirection;
        PlaneDirection = DefaultPlaneDirection;
        
        OnUpdate?.Invoke();
    }

    [Serializable]
    public struct DefaultTransformValue
    {
        public Part Part;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    public DefaultTransformValue[] DefaultTransforms;

    public void CalculatePosRot()
    {
        DefaultTransforms = new DefaultTransformValue[List.Count];
        
        for (int i = 0; i < List.Count; i++)
        {
            DefaultTransforms[i].Part = List[i];
            DefaultTransforms[i].Position = List[i].transform.localPosition;

            // if ((List[i].Type.Category == PartCategory.Grid 
            //     || List[i].Type.Category == PartCategory.Cabin))
            // {
            //     DefaultTransforms[i].Rotation = Quaternion.Euler(new Vector3(0, -180, 0));
            // }
            // else
            {
                DefaultTransforms[i].Rotation = List[i].transform.localRotation;
            }
            
            // Debug.Log("Part - " + List[i].name 
            //             + ",\ndef pos - " + DefaultTransforms[i].Position 
            //             + ",\ndef rot - " + DefaultTransforms[i].Rotation
            //             + ",\ndef rot - " + DefaultTransforms[i].Rotation.eulerAngles
            //             + "\n");
        }
    }
    
    async void CalculateParameters(ParametersModifier mod)
    {
        if (mod == null) return;
        
        if (mod.Type == ModifierType.Boost)
        {
            CalculateBoost();
        }
        else if (mod.Type == ModifierType.Wheels)
        {
            CalculateAcceleration();
        }
        else if (mod.Type == ModifierType.Wings)
        {
            CalculatePlane();
        }

        // await UniTask.Delay(100);
        
        CalculateMass();
        CalculateBalance();
        
        if (!GameManager.Instance.GameStarted) CalculatePosRot();
        
        if (GameManager.Instance.GameStarted) PlayerController.Instance.CorrectSphereColliderCenter();
        
        OnUpdate?.Invoke();
    }

    void CalculateBoost()
    {
        Vector3 direction = Vector3.zero;
        float force = 0f;
        int count = 0;

        Part part;
        ParametersModifier mod;

        for (int i = 0; i < List.Count; i++)
        {
            part = List[i];
            if (part)
            {
                mod = part.GetFlyParameters();
                if (mod != null && mod.Type == ModifierType.Boost)
                {
                    direction += mod.Direction;

                    // if (force < mod.Force)
                    // {
                    //     force = mod.Force;
                    // }
                    force += mod.Force;
                    
                    count++;
                }
            }
        }

        BoostDirection = (direction + (count > 0 ? Vector3.zero : DefaultBoostDirection)).normalized;
        // BoostModificator = (count > 0 ? force / count : 0) + DefaultBoost;
        BoostModificator = force + DefaultBoost;
        
        // Debug.Log("BD - " + BoostDirection + ", BM - " + BoostModificator + ", MDM - " + MaxBoostModificator);
    }

    void CalculateAcceleration()
    {
        Vector3 direction = Vector3.zero;
        float force = 0f;

        Part part;
        ParametersModifier mod;

        for (int i = 0; i < List.Count; i++)
        {
            part = List[i];
            if (part)
            {
                mod = part.GetFlyParameters();
                if (mod != null && mod.Type == ModifierType.Wheels)
                {
                    // Debug.Log(mod.Direction);
                    direction += mod.Direction;
                    
                    // if (force < mod.Force)
                    // {
                    //     force = mod.Force;
                    // }
                    force += mod.Force;
                }
            }
        }
        
        AccelerationDirection = (direction + DefaultAccelerationDirection).normalized;
        AccelerationModificator = force + DefaultAcceleration;
        
        // Debug.Log("AD - " + AccelerationDirection + ", AM - " + AccelerationModificator);
    }

    void CalculatePlane()
    {
        Vector3 direction = Vector3.zero;
        float force = 0f;
        int count = 0;

        Part part;
        ParametersModifier mod;

        for (int i = 0; i < List.Count; i++)
        {
            part = List[i];
            if (part)
            {
                mod = part.GetFlyParameters();
                if (mod != null && mod.Type == ModifierType.Wings)
                {
                    direction += mod.Direction;
                    
                    if (force < mod.Force)
                    {
                        force = mod.Force;
                    }
                    // force += mod.Force;
                    
                    count++;
                }
            }
        }
        
        PlaneDirection = (direction + DefaultPlaneDirection).normalized;
        // PlaneModificator = (count > 0 ? force / count : 0f) + DefaultPlane;
        PlaneModificator = force + DefaultPlane;
    }

    void CalculateBalance()
    {
        float balance = 0;

        Part part;
        
        int mainIndex = PlayerGrid.Instance.MainIndex;
        BalanceCenter = 0f;
        
        int count = 3;
        for (int i = 0; i < count; i++)
        {
            part = PlayerGrid.Instance.GetByIndex(mainIndex + 1 - i).Part;
            if (part && (part.Type.Category == PartCategory.Grid || part.Type.Category == PartCategory.Cabin))
            {
                if (i == 3 - 1 || i == 0)
                {
                    BalanceCenter += part.GetFlyParameters().LocalPosition.z / 2;
                }
                else
                {
                    BalanceCenter += part.GetFlyParameters().LocalPosition.z;
                }
            }
        }
        
        ParametersModifier mod;
        float z;
        
        for (int i = 0; i < List.Count; i++)
        {
            part = List[i];
            if (part)
            {
                mod = List[i].GetFlyParameters();
                if (mod != null)
                {
                    z = mod.LocalPosition.z - BalanceCenter;
                    z = Mathf.Clamp(z, -1, 1);
                    balance += z * mod.Mass;
                }
            }
        }

        if (Mass != 0)
        {
            balance /= Mass;
            Balance = balance;
        }
        else
        {
            Balance = 0;
        }
    }

    void CalculateMass()
    {
        float mass = 0;
        
        Part part;
        ParametersModifier mod;
        
        for (int i = 0; i < List.Count; i++)
        {
            part = List[i];
            if (part)
            {
                mod = part.GetFlyParameters();
                if (mod != null 
                    //&& part.Type.Category != PartCategory.Cabin && part.Type.Category != PartCategory.Grid
                    )
                {
                    mass += mod.Mass;
                }
            }
        }

        Mass = mass;
    }
}