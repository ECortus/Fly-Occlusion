using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Zenject;

public class Gem : MonoBehaviour
{
    public static Action OnValueChange { get; set; }
    
    [Inject] public static Gem Instance { get; private set; }

    [Inject] void Awake()
    {
        Instance = this;
    }
    
    public int Value
    {
        get => PlayerPrefs.GetInt("Gem", 0);
        private set
        {
            PlayerPrefs.SetInt("Gem", value);
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
