using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GridCell : MonoBehaviour
{
    public static event Action OnUpdateState;
        
    public static GridCell SelectedCell { get; private set; }
    public Part Part;
    public Part AdditionalPart { get; private set; }

    private int _id;
    private bool _showed;

    private void OnMouseOver()
    {
        SelectedCell = this;
    }

    private void OnMouseExit()
    {
        SelectedCell = null;
    }

    public void Registry(Part part)
    {
        Part = part;
        OnUpdateState?.Invoke();
        Save();
    }
    
    public void RegistryAdditional(Part part)
    {
        AdditionalPart = part;
        OnUpdateState?.Invoke();
        Save();
    }
    
    public void UnRegistryAdditional()
    {
        AdditionalPart = null;
        OnUpdateState?.Invoke();
        Save();
    }
    
    public void UnRegistry()
    {
        Part = null;

        Part part = AdditionalPart;
        if (part)
        {
            MergeGrid.Instance.SpawnPart(part.Type.GetPart(AdditionalPart.Level));
            part._currentGridCell.UnRegistryAdditional();
            part.DestroyPart();
        }
        
        AdditionalPart = null;
        
        OnUpdateState?.Invoke();
        Save();
    }

    private void Save()
    {
        SaveManager.Instance.Save($"GridCell{_id}", Part);
        SaveManager.Instance.Save($"AdditionalGridCell{_id}", AdditionalPart);
    }
        
    public void Load(int id)
    {
        _id = id;
        Part = SaveManager.Instance.Load($"GridCell{_id}");
        AdditionalPart = SaveManager.Instance.Load($"AdditionalGridCell{_id}");
            
        if (Part) PlayerGrid.Instance.SpawnPartToCell(Part, this);
        if (AdditionalPart) PlayerGrid.Instance.SpawnAdditionalPartToCell(AdditionalPart, this);
        OnUpdateState?.Invoke();
    }
}
