using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostPart : Part
{
    [Space]
    [SerializeField] private BoostParameters pars;

    [Space] 
    [SerializeField] private float rotateSpeed = 50f;
    [SerializeField] private float rotateAcceleration = 50f;
    [SerializeField] private Transform fan;
    [SerializeField] private ParticleSystem trail;

    private float rotateTarget;
    private float rotateMotor;

    private Vector3 rotate;
    
    private void FixedUpdate()
    {
        if (GameManager.Instance.GameStarted && VisualMode)
        {
            if (!trail.isPlaying) trail.Play();
            rotateTarget = 1;
            
            // VibrationController.Instance.VibrateLow();
        }
        else
        {
            trail.Stop();
            trail.Clear();
            
            rotateTarget = 0;
        }

        rotateMotor = Mathf.Lerp(rotateMotor, rotateTarget, rotateAcceleration * Time.fixedDeltaTime);
        rotate = new Vector3(0, 0, rotateMotor * rotateSpeed * Time.fixedDeltaTime);

        if (rotate != Vector3.zero)
        {
            fan.Rotate(rotate, Space.Self);
        }
    }
    
    private Vector3 direction => transform.forward;
    
    public override ParametersModifier GetFlyParameters()
    {
        float forceMod = 1;

        if (_currentGridCell)
        {
            if (Orientation == PartOrientation.Top)
            {
                forceMod = 0.2f;
            }
            else if (Orientation == PartOrientation.Left)
            {
                forceMod = 1f;
            }
            else if (Orientation == PartOrientation.Right)
            {
                forceMod = 0.4f;
            }
        }
        
        ParametersModifier modif = new ParametersModifier(
            ModifierType.Boost,
            pars.GetMotorForce(Level) * forceMod,
            // direction,
            -Vector3.forward,
            transform.localPosition,
            Mass
        );
        
        return modif;
    }
    
    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.magenta;
    //     Gizmos.DrawRay(transform.position, direction * 999f);
    // }
}
