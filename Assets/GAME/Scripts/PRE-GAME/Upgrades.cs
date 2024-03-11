using System.Collections;
using System.Collections.Generic;
using AppsFlyerSDK;
using UnityEngine;

public static class Upgrades
{
    public static int LaunchPower
    {
        get => PlayerPrefs.GetInt("LaunchPower", 0);
        private set
        {
            PlayerPrefs.SetInt("LaunchPower", value);
            PlayerPrefs.Save();
        }
    }

    public static void IncreaseLaunchPower()
    {
        LaunchPower++;
        AppsFlyerEventsSuite.AF_BONUS_CLAIMED($"LaunchPower-Upgrade-LVL-{LaunchPower.ToString()}");
    }
    public static void ResetLaunchPower() => LaunchPower = 0;
    
    public static int CurrencyAmount
    {
        get => PlayerPrefs.GetInt("CurrencyAmount", 0);
        private set
        {
            PlayerPrefs.SetInt("CurrencyAmount", value);
            PlayerPrefs.Save();
        }
    }
    
    public static void IncreaseCurrencyAmount()
    { 
        CurrencyAmount++;
        AppsFlyerEventsSuite.AF_BONUS_CLAIMED($"CurrencyAmount-Upgrade-LVL-{CurrencyAmount.ToString()}");
    }
    public static void ResetCurrencyAmount() => CurrencyAmount = 0;
    
    public static int PartsBuyLevel
    {
        get => PlayerPrefs.GetInt("PartsBuyLevel", 0);
        private set
        {
            PlayerPrefs.SetInt("PartsBuyLevel", value);
            PlayerPrefs.Save();
        }
    }
    
    public static void IncreasePartsBuyLevel()
    {
        PartsBuyLevel++;
        AppsFlyerEventsSuite.AF_BONUS_CLAIMED($"PartsBuyLevel-Upgrade-LVL-{PartsBuyLevel.ToString()}");
    }
    public static void ResetPartsBuyLevel() => PartsBuyLevel = 0;
}
