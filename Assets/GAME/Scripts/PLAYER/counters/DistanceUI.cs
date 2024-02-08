using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DistanceUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private DistanceMilestonesUI milestones;
    
    private void Update()
    {
        milestones.Refresh();
        text.text = $"{Mathf.RoundToInt(GameManager.Instance.FlyLength)}m";
    }
}
