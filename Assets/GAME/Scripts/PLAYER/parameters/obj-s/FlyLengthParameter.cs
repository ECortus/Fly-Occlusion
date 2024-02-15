using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create player parameters/FlyLength")]
public class FlyLengthParameter : ParameterObject
{
    public override float Value
    {
        get
        {
            // Vector3 forceV1 = -ConnectedParts.Instance.AccelerationDirection 
            //         * ConnectedParts.Instance.AccelerationModificator / ConnectedParts.PlaneParameterRelativity;
            // // Vector3 forceV2 = ConnectedParts.Instance.PlaneDirection 
            // //     * ConnectedParts.Instance.PlaneModificator * ConnectedParts.PlaneParameterRelativity;
            //
            // float force1 = Mathf.Clamp(forceV1.z, 0f, 999f);
            // float force2 = Mathf.Clamp(forceV2.z, 0f, 999f);

            float wheelForce = ConnectedParts.Instance.AccelerationModificator;
            float wingForce = ConnectedParts.Instance.PlaneModificator;
            
            // Debug.Log("Wheels par - " + wheelForce + ", wings par - " + wingForce);

            return ConnectedParts.Instance.DistanceFormula(wingForce, wheelForce);
        }
    }
    public override float MaxValue => ConnectedParts.Instance.MaxDistanceModificator;
}
