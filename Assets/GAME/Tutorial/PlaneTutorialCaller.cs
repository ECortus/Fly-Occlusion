using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneTutorialCaller : MonoBehaviour
{
    private bool Showed
    {
        get => PlayerPrefs.GetInt("PlaneTutorialShowed", 0) != 0;
        set
        {
            PlayerPrefs.SetInt("PlaneTutorialShowed", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    [SerializeField] private float timeDelay = 5f;
    [SerializeField] private float heightToShow = 99f;

    private void Awake()
    {
        if (Showed)
        {
            Off();
            return;
        }

        time = timeDelay;
        gameObject.SetActive(true);
    }

    private float time;

    void Update()
    {
        if (GameManager.Instance.GameStarted && PlayerController.Instance.Launched && !Showed)
        {
            time -= Time.unscaledDeltaTime;

            if (GameManager.Instance.FlyHeight >= heightToShow && time <= 0)
            {
                Show();
            }
        }
    }

    void Show()
    {
        Showed = true;
        time = 9999f;
        
        Tutorial.Instance.PlaneIteration();
        Off();
    }

    void Off()
    {
        gameObject.SetActive(false);
    }
}
