using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class VibrationController : MonoBehaviour
{
    [Inject] public static VibrationController Instance { get; private set; }

    [Inject]
    private void Awake()
    {
        Instance = this;
    }

    public void Vibrate()
    {
        if (!SettingsModes.Vibration) return;
        
        Vibration.Vibrate();
    }
    
    public void VibrateHeavy()
    {
        if (!SettingsModes.Vibration) return;
        
#if UNITY_ANDROID
        Vibration.VibrateAndroid(125);
#elif UNITY_IOS
        Vibration.VibrateIOS(ImpactFeedbackStyle.Heavy);
#endif
        
    }
    
    public void VibrateLow()
    {
        if (!SettingsModes.Vibration) return;
        
#if UNITY_ANDROID
        Vibration.VibrateAndroid(25);
#elif UNITY_IOS
        Vibration.VibrateIOS(ImpactFeedbackStyle.Soft);
#endif
        
    }

    public void Cancel()
    {
        if (!SettingsModes.Vibration) return;
        
#if UNITY_ANDROID
        Vibration.CancelAndroid();
#elif UNITY_IOS
        
#endif
    }
}
