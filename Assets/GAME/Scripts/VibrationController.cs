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
    
    public void VibrateLow()
    {
        if (!SettingsModes.Vibration) return;
        
#if UNITY_ANDROID
        Vibration.VibrateAndroid(50);
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
