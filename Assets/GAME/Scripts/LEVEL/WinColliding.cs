using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinColliding : MonoBehaviour
{
    private bool Condition(GameObject go) => go.layer == LayerMask.NameToLayer("Player") && GameManager.Instance.GameStarted;
    
    private void OnTriggerEnter(Collider other)
    {
        if (Condition(other.gameObject))
        {
            PlayerController.Instance.ForceFinish("WORLD END");
        }
    }
}
