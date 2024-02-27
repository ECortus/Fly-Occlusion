using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SectionXMultiplier : MonoBehaviour
{
    public int Multiplier;
    public bool Entered { get; private set; }

    private PlatformX5 platform;

    private bool Condition(GameObject go) => go.layer == LayerMask.NameToLayer("Player") && GameManager.Instance.GameStarted;

    void Awake()
    {
        Entered = false;
        platform = GetComponentInParent<PlatformX5>();

        GameManager.OnMergeGame += Reset;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (Condition(other.gameObject))
        {
            Entered = true;
        }
    }

    void Reset()
    {
        Entered = false;
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (Condition(other.gameObject))
        {
            Entered = false;

            if (platform.CurrentMultiplier < 1)
            {
                PlayerController.Instance.SetMultiplier(1);
            }
        }
    }
}
