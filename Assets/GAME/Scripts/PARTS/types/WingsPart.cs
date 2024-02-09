using System;
using System.Collections;
using System.Collections.Generic;
using ModestTree;
using UnityEngine;

public class WingsPart : Part
{
    [Space]
    [SerializeField] private WingsParameters pars;

    [SerializeField] private TrailRenderer trail1, trial2;

    private void FixedUpdate()
    {
        if (GameManager.Instance.GameStarted && PlayerController.Instance.Launched)
        {
            trail1.enabled = true;
            trial2.enabled = additionalObject.activeSelf;
        }
        else
        {
            trail1.enabled = false;
            trial2.enabled = false;
        }
    }

    private Vector3 direction => -transform.up;

    public override ParametersModifier GetFlyParameters()
    {
        float forceMod = 1;

        int index = PlayerGrid.Instance._cells.IndexOf(_currentGridCell);

        if (index == PlayerGrid.Instance.MainIndex)
        {
            forceMod = 1;
        }
        else
        {
            if (Orientation == PartOrientation.Front)
            {
                forceMod = 0.75f;
            }
            else if (Orientation == PartOrientation.Top)
            {
                forceMod = 0.5f;
            }
            else
            {
                forceMod = 0.25f;
            }
        }
        
        ParametersModifier modif = new ParametersModifier(
            ModifierType.Wings,
            pars.GetFlyModifier(Level) * forceMod,
            Vector3.up,
            transform.localPosition,
            Mass
        );

        return modif;
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.magenta;
    //     Gizmos.DrawRay(transform.position, direction * 99999f);
    // }
}
