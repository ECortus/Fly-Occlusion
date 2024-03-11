using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;

public class IDFAObject : MonoBehaviour, IGameAnalyticsATTListener
{
    void Start()
    {
        if(Application.platform == RuntimePlatform.IPhonePlayer)
        {
            GameAnalytics.RequestTrackingAuthorization(this);
        }
        else
        {
            Init();
        }
    }

    public void GameAnalyticsATTListenerNotDetermined()
    {
        
    }
    
    public void GameAnalyticsATTListenerRestricted()
    {
        Init();
    }
    
    public void GameAnalyticsATTListenerDenied()
    {
        
    }
    
    public void GameAnalyticsATTListenerAuthorized()
    {
        Init();
        
    }

    void Init()
    {
        GameAnalytics.Initialize();
        GameAnalyticsILRD.SubscribeMaxImpressions();
    }
}
