using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartDustParticle : MonoBehaviour
{
    private static PartDustParticle Instance { get; set; }

    private void Awake()
    {
        Instance = this;
    }

    public static void On() => Instance._On();
    public static void Off() => Instance._Off();

    void _On()
    {
        gameObject.SetActive(true);
    }

    void _Off()
    {
        gameObject.SetActive(false);
    }
    
    [SerializeField] private ParticleSystem particle;
    
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (!particle.isPlaying)
            {
                particle.Play();
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (particle.isPlaying)
            {
                particle.Stop();
            }
        }
    }
}
