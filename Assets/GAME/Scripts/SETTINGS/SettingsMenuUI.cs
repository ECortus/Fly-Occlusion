using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenuUI : MonoBehaviour
{
    [SerializeField] private GameObject menu;
    
    public void Open()
    {
        menu.SetActive(true);
        Time.timeScale = 0f;
        
        GameManager.Instance.ShowInterstationalAd();
    }

    public void Close()
    {
        menu.SetActive(false);
        Time.timeScale = 1f;
        
        GameManager.Instance.ShowInterstationalAd();
    }
}
