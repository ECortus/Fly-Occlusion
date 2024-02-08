using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Create player parameters/Balance")]
public class BalanceParameter : ParameterObject
{
    public override float Value => 1f - Mathf.Abs(ConnectedParts.Instance.Balance);
    public override float MaxValue => 1f;
}
