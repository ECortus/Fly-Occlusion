using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create player parameters/FlyBoost")]
public class FlyBoostParameter : ParameterObject
{
    public override float Value
    {
        get
        {
            Vector3 direction = new Vector3(
                Sign(ConnectedParts.Instance.BoostDirection.x),
                Sign(ConnectedParts.Instance.BoostDirection.y),
                Sign(ConnectedParts.Instance.BoostDirection.z)
                );
            Vector3 forceV = -direction * ConnectedParts.Instance.BoostModificator;
            
            float force1 = Mathf.Clamp(forceV.z, 0f, 999f);
            float force2 = Mathf.Clamp(-forceV.y, 0f, 999f);
            
            // Debug.Log("Boost1 par - " + force1 + ", boost2 par - " + force2);

            // float force;
            // if (force1 > force2)
            // {
            //     force = Mathf.Pow(force1, 2) - Mathf.Pow(force2, 2);
            // }
            // else
            // {
            //     force = Mathf.Pow(force2, 2) - Mathf.Pow(force1, 2);
            // }

            // return Mathf.Pow(force, 1/2f);
            return Formula(force1, 0);
        }
    }

    float Sign(float value)
    {
        if (Mathf.Abs(value) <= 0.15f) return 0;
        return Mathf.Sign(value);
    }

    float Formula(float first, float second)
    {
        return first + second / 6f;
    }

    public override float MaxValue => Formula(ConnectedParts.Instance.MaxBoostModificator, 0);
}
