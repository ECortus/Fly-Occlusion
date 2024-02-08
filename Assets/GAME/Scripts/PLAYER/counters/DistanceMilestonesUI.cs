using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DistanceMilestonesUI : MonoBehaviour
{
    [SerializeField] private GridLayoutGroup icons;
    [SerializeField] private GameObject iconPrefab;

    [Space] 
    [SerializeField] private List<Transform> toggles;
    
    [Space]
    [SerializeField] private Slider flySlider;
    [SerializeField] private float[] milestones;

    [ContextMenu("Create")]
    public void Create()
    {
        for(int i = 0; i < icons.transform.childCount;)
        {
            DestroyImmediate(icons.transform.GetChild(0).gameObject);
        }

        int count = milestones.Length;
        if (count == 0) return;

        GameObject icon;
        float stat;
        
        toggles.Clear();
        
        for (int i = 0; i < count; i++)
        {
            icon = Instantiate(iconPrefab, icons.transform);
            stat = milestones[i];
            
            SetIcon(icon, i + 1, stat, false);
        }

        RectTransform flySliderTransform = flySlider.GetComponent<RectTransform>();
        flySliderTransform.sizeDelta = 
            new Vector2(icons.cellSize.x * (count - 1) + (icons.spacing.x + 5) * (count - 2), flySliderTransform.sizeDelta.y);
    }
    
    void SetIcon(GameObject icon, int index, float length, bool state)
    {
        TextMeshProUGUI text = icon.transform.GetChild(0).GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
        Transform toggle = icon.transform.GetChild(0);

        text.text = $"{index.ToString()}";
        
        toggle.GetChild(0).gameObject.SetActive(!state);
        toggle.GetChild(1).gameObject.SetActive(state);
        
        toggles.Add(toggle);
        
        icon.gameObject.SetActive(true);
    }

    void SetToggle(int index, bool state)
    {
        Transform toggle = toggles[index];
        toggle.GetChild(0).gameObject.SetActive(!state);
        toggle.GetChild(1).gameObject.SetActive(state);
    }
    
    public void Refresh()
    {
        flySlider.minValue = 0;
        flySlider.maxValue = 1;

        float count = milestones.Length;
        float space = 1f / count;
        
        int completed = 0;
        float requireDistance = 0;
        
        float distance = GameManager.Instance.FlyLength;

        if (distance > 0)
        {
            for(int i = 0; i < count; i++)
            {
                requireDistance = milestones[i];
            
                if (distance <= requireDistance)
                {
                    requireDistance = milestones[i] - milestones[i - 1];
                    distance -= milestones[i - 1];
                    break;
                }
            
                completed++;
            }
        }

        for (int i = 0; i < count; i++)
        {
            SetToggle(i, i < completed);
        }

        if (completed == 0) flySlider.value = 0;
        else flySlider.value = space * (completed - 1) + space * (distance / requireDistance);
    }
}
