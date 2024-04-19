using System;
using System.Collections;
using System.Collections.Generic;
using AppsFlyerSDK;
using UnityEngine;
using Cysharp.Threading.Tasks;
using GAME.Scripts.MONETIZATION;
using Unity.VisualScripting;
using Zenject;

public class GameManager : MonoBehaviour
{
    [Inject] public static GameManager Instance { get; private set; }

    public bool GameStarted { get; private set; }
    
    public static Action OnMergeGame { get; set; }
    void GameMerge() => OnMergeGame?.Invoke();
    
    public static Action OnGameStart { get; set; }
    void GameStart() => OnGameStart?.Invoke();
    
    public static Action OnGameFinish { get; set; }
    void GameFinish() => OnGameFinish?.Invoke();

    public float FlyHeight => PlayerController.Instance.GetDistanceToGround() / FlyDistanceRelativity;
    public float FlySpeed => PlayerController.Instance.Body.velocity.magnitude / FlyDistanceRelativity;

    [Space] 
    [SerializeField] private Transform flyStart;
    
    private Transform FlyStart;
    private float _flyLength;
    
    public float FlyLength
    {
        get
        {
            if (GameStarted)
            {
                Vector3 from = FlyStart.position;
                Vector3 to = PlayerController.Instance.Center;
            
                from.y = 0;
                to.y = 0;
            
                Vector3 direction = (from - to).normalized;
                float distance = (from - to).magnitude;
                
                if (Vector3.Dot(-FlyStart.forward, direction) < 0 || distance > 9999999f)
                {
                    _flyLength = 0;
                }
                else
                {
                    _flyLength = Mathf.Lerp(_flyLength, distance, 0.05f);
                }
            }
            
            return _flyLength / FlyDistanceRelativity;
        }
    }

    [SerializeField] private float flyDistanceRelativity = 1;
    private float FlyDistanceRelativity { get; set; }
    private Vector3 LaunchPos { get; set; }
    
    [Space]
    [SerializeField] private PlayerController player;

    [Space] 
    [SerializeField] private float debugTimeScale = 1;
    [SerializeField] private bool UseDebugFPS = false;
    [SerializeField] private int DebugFPS = 15;

    [Inject] private void Awake()
    {
        Application.targetFrameRate = !UseDebugFPS ? 60 : DebugFPS;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        Time.timeScale = debugTimeScale;
        
        Instance = this;
        FlyDistanceRelativity = flyDistanceRelativity;

        FlyStart = flyStart;
        LaunchPos = player.transform.position;

        Vibration.Init();
        
        OnGameFinish += RecordMaxFlyLength;
    }

    async void Start()
    {
        if (!Tutorial.Instance.Completed)
        {
            OnMergeGame += CheckTutorial;
        }
        else
        {
            AdsManager.ToggleBanner(true, MaxSdkBase.BannerPosition.BottomCenter);
        }
        
        MergeGame();

        await UniTask.WaitUntil(() =>
        {
            return MaxSdk.IsInitialized() && MaxSdk.IsUserConsentSet() && !MaxSdk.IsDoNotSell() && !MaxSdk.IsAgeRestrictedUser();
        });
        GameAnalyticsEventsSuite.GameReady();

        string key = "";
        await UniTask.WaitUntil(() =>
        {
            if (Input.GetMouseButtonDown(0))
            {
                key = "TOUCH_INPUT";
                return true;
            }

            return false;
        });
        GameAnalyticsEventsSuite.FirstInteraction(key);
    }

    void CheckTutorial()
    {
        if (!Tutorial.Instance.Completed)
        {
            Tutorial.Instance.StartTutorial();
        }
        else
        {
            OnMergeGame -= CheckTutorial;
            AdsManager.ToggleBanner(true, MaxSdkBase.BannerPosition.BottomCenter);
        }
    }
    
    public async void MergeGame()
    {
        LaunchController.Blocked = true;
        PlayerController.Instance.SpawnToPos(LaunchPos);
        
        _flyLength = 0;
        
        GameStarted = false;
        GameMerge();
        
        VibrationController.Instance.VibrateHeavy();
        
        await DarkEclipse.Play();
    }
    
    public async void StartGame()
    {
        if (GameStarted) return;

        _flyLength = 0;

        GameStarted = true;
        GameStart();
        
        GameAnalyticsEventsSuite.LevelProgressionStart();
        
        VibrationController.Instance.VibrateHeavy();
    }

    public void RecordMaxFlyLength()
    {
        if (FlyLength >= Records.Instance.MaxDistance)
        {
            Records.Instance.RecordMaxDistance(FlyLength);
        }
    }
    
    public void FinishGame(string cause)
    {
        if (!GameStarted) return;
        
        GameAnalyticsEventsSuite.EngagementWithCore($"End_flying_by_{cause.ToUpper()}");
        
        GameStarted = false;
        GameFinish();
        
        VibrationController.Instance.VibrateHeavy();

        // ShowInterstationalAd();
    }
    
    public void ShowInterstationalAd()
    {
        if (Tutorial.Instance.Completed)
        {
            AdsManager.ShowInter("FullScreen");
        }
    }

    private void OnDestroy()
    {
        GameAnalyticsEventsSuite.GameEnd();
    }
}