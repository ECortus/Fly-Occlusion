using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct OrientationParameters
{
    public OrientationParameter[] Data;
    public OrientationBlock Block;

    public OrientationParameter GetOrientationParameter(PartOrientation type)
    {
        // return Data.FirstOrDefault(c => c.Type == type);

        foreach (var VARIABLE in Data)
        {
            if (VARIABLE.Type == type)
            {
                // Debug.Log("return right - " + type);
                return VARIABLE;
            }
        }

        Debug.Log("return wrong - " + type);
        return new OrientationParameter();
    }
    
    public Vector3 GetAnglesByOrient(PartOrientation type)
    {
        return GetOrientationParameter(type).RequireAngles;
    }

    public PartOrientation[] GetOrientations()
    {
        List<PartOrientation> list = new List<PartOrientation>();
        foreach (var VARIABLE in Data)
        {
            list.Add(VARIABLE.Type);
        }

        return list.ToArray();
    }

    public bool HaveOrientation(PartOrientation type)
    {
        bool value = false;
        foreach (var VARIABLE in Data)
        {
            if (VARIABLE.Type == type)
            {
                value = true;
                break;
            }
        }

        return value;
    }
}

[System.Serializable]
public struct OrientationParameter
{
    public Vector3 FixedPosition;
    public Vector3 RequireAngles;
    public PartOrientation Type;
    
    public bool ApplyMirror;
}

[System.Serializable]
public enum PartOrientation
{
    Default = 0, Bottom = 1, Top = 2, Left = 4, Right = 8, Front = 16
}

[System.Serializable]
public struct OrientationBlock
{
    public bool Top, Bottom, Right, Left, Front;
}
