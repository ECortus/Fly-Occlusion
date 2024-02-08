using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class Gold : MonoBehaviour
{
    [Inject] public static Gold Instance { get; private set; }

    [Inject] void Awake()
    {
        Instance = this;
    }
    
    public static Action OnValueChange { get; set; }
    
    public int Value
    {
        get => PlayerPrefs.GetInt("Gold", 0);
        private set
        {
            PlayerPrefs.SetInt("Gold", value);
            PlayerPrefs.Save();
        }
    }

    public void Plus(int amount)
    {
        Value += amount;
        OnValueChange?.Invoke();
    }

    public void Minus(int amount)
    {
        Value -= amount;
        OnValueChange?.Invoke();
    }
}
