using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
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

    [Inject] private void Awake()
    {
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
        Time.timeScale = debugTimeScale;
        
        Instance = this;
        FlyDistanceRelativity = flyDistanceRelativity;

        FlyStart = flyStart;
        LaunchPos = player.transform.position;

        Vibration.Init();
        
        OnGameFinish += RecordMaxFlyLength;
    }

    void Start()
    {
        if (!Tutorial.Instance.Completed) OnMergeGame += CheckTutorial;
        MergeGame();
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
        
        VibrationController.Instance.VibrateHeavy();
    }

    public void RecordMaxFlyLength()
    {
        if (FlyLength >= Records.Instance.MaxDistance)
        {
            Records.Instance.RecordMaxDistance(FlyLength);
        }
    }
    
    public void FinishGame()
    {
        if (!GameStarted) return;
        
        GameStarted = false;
        GameFinish();
        
        VibrationController.Instance.VibrateHeavy();
    }
}