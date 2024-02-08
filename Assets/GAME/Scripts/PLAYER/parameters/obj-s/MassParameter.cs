using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create player parameters/Mass")]
public class MassParameter : ParameterObject
{
    public override float Value => ConnectedParts.Instance.Mass;
    public override float MaxValue => ConnectedParts.Instance.MaxMass;
}
