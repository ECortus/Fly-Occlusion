using System;
using System.Collections;
using System.Collections.Generic;
using AppsFlyerSDK;
using UnityEngine;

public class GemPickUp : MonoBehaviour
{
    [SerializeField] private int _amount = 5;
    [SerializeField] private float accelerateSpeed = 0.25f;
    [SerializeField] private float accelerateTime = 1f;

    [Space] 
    [SerializeField] private Transform toRotate;
    [SerializeField] private float rotateSpeed = 25;

    [Space] 
    [SerializeField] private ParticleSystem particle;
    
    private bool Condition(GameObject obj) => obj.layer == LayerMask.NameToLayer("Player") 
                                              && PlayerController.Instance.Launched && GameManager.Instance.GameStarted;

    void Awake()
    {
        GameManager.OnMergeGame += On;
    }

    private void OnDestroy()
    {
        GameManager.OnMergeGame -= On;
    }

    private void On()
    {
        gameObject.SetActive(true);
        particle.Play();
    }
    
    private void Off()
    {
        particle.Stop();
        gameObject.SetActive(false);
    }

    void Update()
    {
        toRotate.Rotate(new Vector3(0, 1, 0) * rotateSpeed * Time.deltaTime, Space.Self);
    }

    private void PickUp()
    {
        Off();
        
        PlayerController.Instance.AccelerateForwardForTime(accelerateSpeed, accelerateTime);
        PlayerController.Instance.PlayBarrelRoll(accelerateTime);
        
        Gem.Instance.Plus(_amount);
        
        AppsFlyerEventsSuite.AF_BONUS_CLAIMED($"Crystal-PickUp-Amount-{_amount.ToString()}-On-Fly-Distance-{((int)GameManager.Instance.FlyLength).ToString()}");
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (Condition(other.gameObject))
        {
            PickUp();
        }
    }
}
