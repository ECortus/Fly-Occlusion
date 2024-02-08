using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerationZone : MonoBehaviour
{
    [SerializeField] private float speed;
    
    private bool Condition(GameObject obj) => obj.layer == LayerMask.NameToLayer("Player")
                                              && PlayerController.Instance.Launched && GameManager.Instance.GameStarted;
    
    private void OnTriggerStay(Collider other)
    {
        if (Condition(other.gameObject))
        {
            AircraftEngine.EnterAccelerationZone(speed);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (Condition(other.gameObject))
        {
            AircraftEngine.ExitAccelerationZone();
        }
    }
}
