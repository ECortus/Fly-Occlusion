using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Records : MonoBehaviour
{
    [Inject] public static Records Instance { get; private set; }
    
    public float MaxDistance
    {
        get => PlayerPrefs.GetFloat("MaxDistance_Record", 0);
        private set
        {
            PlayerPrefs.SetFloat("MaxDistance_Record", value);
            PlayerPrefs.Save();
        }
    }

    public void RecordMaxDistance(float distance)
    {
        MaxDistance = distance;
    }

    [Inject]
    private void Awake()
    {
        Instance = this;
    }
}
