using System.Collections;
using System.Collections.Generic;
using AppsFlyerSDK;
using Cysharp.Threading.Tasks;
using GAME.Scripts.MONETIZATION;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UpgradeButtonUI : MonoBehaviour
{
    [SerializeField] private UpgradeObject upObject;

    [Space] 
    [SerializeField] private GameObject enoughObject;
    [SerializeField] private GameObject noObject;
    [SerializeField] private GameObject adObject;
    [SerializeField] private TextMeshProUGUI[] costsText;

    private int currentCost => upObject.Cost;

    private bool HaveAdRV = false;

    void Awake()
    {
        GameManager.OnMergeGame += RefreshAdRV;
        GameManager.OnMergeGame += Refresh;
        Gem.OnValueChange += Refresh;
        
        RefreshAdRV();
        Refresh();
    }

    public void OnButtonClick()
    {
        GameAnalyticsEventsSuite.EngagementWithCore($"Buy_upgrade_{upObject.name}_by_GEM");
        if (Gem.Instance.Value >= currentCost)
        {
            Gem.Instance.Minus(currentCost);
            upObject.Action();
            
            Refresh();
        }
    }

    public void OnButtonClickRV()
    {
        GameAnalyticsEventsSuite.EngagementWithCore($"Buy_upgrade_{upObject.name}_by_AD");
        AdsManager.ShowRewarded(gameObject, OnFinishAd, "FullScreen");
    }

    private void OnFinishAd(bool result)
    {
        if (result)
        {
            HaveAdRV = false;
        
            upObject.Action();
            Refresh();
        }
    }

    void RefreshAdRV()
    {
        HaveAdRV = Tutorial.Instance.Completed;
    }
    
    public void Refresh()
    {
        if (upObject.Level >= upObject.MaxLevel)
        {
            ChangeObject(false);
            SetText("---");
            return; 
        }
        
        if (currentCost > Gem.Instance.Value)
        {
            ChangeObject(false);
        }
        else
        {
            ChangeObject(true);
        }

        SetText($"{currentCost}");
    }

    void SetText(string text)
    {
        foreach (var VARIABLE in costsText)
        {
            VARIABLE.text = text;
        }
    }

    void ChangeObject(bool state)
    {
        noObject.SetActive(!state && !HaveAdRV);
        adObject.SetActive(!state && HaveAdRV);
        
        enoughObject.SetActive(state);
        
        // noObject.SetActive(!state);
        // enoughObject.SetActive(state);
    }
}
