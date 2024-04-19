using System.Collections;
using System.Collections.Generic;
using AppsFlyerSDK;
using UnityEngine;

public class GridPart : Part
{
    public override ParametersModifier GetFlyParameters()
    {
        ParametersModifier modif = new ParametersModifier(
            ModifierType.Default,
            0,
            Vector3.zero, 
            transform.localPosition,
            Mass
        );

        return modif;
    }
    
    public override void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Hit"))
        {
            PlayerController.Instance.Stop();
            GameManager.Instance.FinishGame("CRASH");
        }
    }
}
