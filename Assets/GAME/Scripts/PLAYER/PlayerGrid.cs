using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using System.Linq;
using ModestTree;

public class PlayerGrid : MonoBehaviour
{
    public static PlayerGrid Instance { get; private set; }

    [SerializeField] private GridsUploads Uploads;
    
    public readonly int Size = 5;// should be odd numbers
    private int Space = 2;
    
    public int MainIndex => (Size * Size - 1) / 2; // 5 - 12, 3 - 8
    public Vector3 Center => transform.position;
    
    public static Part FindPartByType(PartType partType) =>
        Instance._cells.FirstOrDefault(c => c != null && c.Part != null && c.Part.Type == partType)?.Part;

    public static event Action<Part, bool> OnAddPart;

    public static int FreeCount => Cells.Count(c => c.Part == null);
    public static IReadOnlyCollection<GridCell> Cells => Instance._cells;

    [HideInInspector] public GridCell[] _cells;
    private PartOrientation[] requireOrientations { get; set; }
    
    [field: SerializeField] public Transform parentForParts { get; private set; }
    
    [SerializeField] private GameObject gridUI;
    public void SetGridUILocalX(float x)
    {
        gridUI.transform.localPosition = new Vector3(
            x,
            gridUI.transform.localPosition.y,
            gridUI.transform.localPosition.z);
    }
    
    [Inject] private void Awake()
    {
        Instance = this;
        _cells = gridUI.GetComponentsInChildren<GridCell>(true);

        GameManager.OnMergeGame += LoadAll;
    }

    void OnEnable()
    {
        
    }

    public void ClearMergeParts()
    {
        Part part;
        
        foreach (var VARIABLE in _cells)
        {
            part = VARIABLE.Part;

            if (part && part.Type.Category != PartCategory.Cabin && part.Type.Category != PartCategory.Grid)
            {
                MergeGrid.Instance.SpawnPart(part.Type.GetPart(part.Level));
                part._currentGridCell.UnRegistry();
                part.DestroyPart();
            }
            else if (VARIABLE.AdditionalPart)
            {
                part = VARIABLE.AdditionalPart;
                if (part.Type.Category != PartCategory.Cabin && part.Type.Category != PartCategory.Grid)
                {
                    MergeGrid.Instance.SpawnPart(part.Type.GetPart(part.Level));
                    part._currentGridCell.UnRegistryAdditional();
                    part.DestroyPart();
                }
            }
        }
    }

    public int HavePartOfType(PartCategory type, int lvl = -999)
    {
        GridCell cell;
        Part part;
        
        int count = 0;
        
        foreach (var VARIABLE in _cells)
        {
            cell = VARIABLE;
            
            if (cell)
            {
                part = cell.Part;
                if (part && part.Type.Category == type && (lvl == -999 || part.Level == lvl))
                {
                    count++;
                }
                
                part = cell.AdditionalPart;
                if (part && part.Type.Category == type && (lvl == -999 || part.Level == lvl))
                {
                    count++;
                }
            }
        }

        return count;
    }

    public Part[] GetPartsByTypes(PartType[] types)
    {
        List<Part> parts = new List<Part>();
        
        foreach (var VARIABLE in _cells)
        {
            if (VARIABLE && VARIABLE.Part)
            {
                if (types.IndexOf(VARIABLE.Part.Type) != -1)
                {
                    parts.Add(VARIABLE.Part);
                }
            }
        }

        return parts.ToArray();
    }
    
    public Part[] GetPartsByType(PartType type)
    {
        List<Part> parts = new List<Part>();
        
        foreach (var VARIABLE in _cells)
        {
            if (VARIABLE && VARIABLE.Part)
            {
                if (VARIABLE.Part.Type == type)
                {
                    parts.Add(VARIABLE.Part);
                }
            }
        }

        return parts.ToArray();
    }

    public GridCell GetByIndex(int index) => _cells[index];

    void LoadAll()
    {
        // PlayerController.Instance.ResetBody();
        Part part;
        foreach (Transform VARIABLE in parentForParts)
        {
            part = VARIABLE.GetComponent<Part>();
            if (part) part.DestroyPart();
            else Destroy(VARIABLE.gameObject);
        }
        
        ConnectedParts.Instance.Clear();
        
        for (int i = 0; i < _cells.Length; i++)
        {
            GridCell cell = _cells[i];
            cell.Load(i);
        }
        
        UploadGrids();

        foreach (var VARIABLE in _cells)
        {
            if (VARIABLE.Part)
            {
                SetDefaultPartParsOnGrid(VARIABLE.Part);
            }
        }
    }

    void UploadGrids()
    {
        GridsUploads.UploadStat stat = Uploads.CurrentStat;

        int index;
        GridCell cell;
        Part part;
        
        for(int i = 0; i < stat.Parts.Length; i++)
        {
            if (stat.Parts[i].Type.Category == PartCategory.Cabin)
            {
                if (HavePartOfType(PartCategory.Cabin, stat.LevelsOf) > 0)
                {
                    continue;
                }
            }
            else if (stat.Parts[i].Type.Category == PartCategory.Grid)
            {
                if (HavePartOfType(PartCategory.Grid, stat.LevelsOf) >= stat.Parts.Length - 1 || !PartUnlocked.Grids)
                {
                    break;
                }
                
                ClearMergeParts();
            }

            if (HavePartOfType(PartCategory.Grid, stat.LevelsOf - 1) >= 0)
            {
                CleanGrids(stat.LevelsOf - 1);
            }
            
            index = MainIndex + stat.Parts[i].IndexOffset.x + stat.Parts[i].IndexOffset.y * 5;
            cell = _cells[index];
            
            SpawnPartToCell(stat.Parts[i].Type.GetPart(stat.LevelsOf), cell);
        }
    }

    void CleanGrids(int lvl = -999)
    {
        GridCell cell;
        Part part;
        
        for (int i = 0; i < 5; i++)
        {
            cell = GetByIndex(MainIndex + 2 - i);
            part = cell.Part;
            
            if (part && part.Type.Category == PartCategory.Grid && (lvl == -999 || part.Level == lvl))
            {
                cell.UnRegistry();
                part.DestroyPart();
            }
        }
    }
    
    public void RefreshPartsToPartsLevelUpgrade(int level)
    {
        Part part;
        Part toSpawn;
        
        foreach (var VARIABLE in _cells)
        {
            if (VARIABLE && VARIABLE.Part)
            {
                if (VARIABLE.Part.Type.Category != PartCategory.Cabin
                    && VARIABLE.Part.Type.Category != PartCategory.Grid && VARIABLE.Part.Level < level)
                {
                    part = VARIABLE.Part;
                    toSpawn = part.Type.GetPart(level);
                    
                    VARIABLE.UnRegistry();
                    part.DestroyPart();
                    
                    SpawnPartToCell(toSpawn, VARIABLE);
                }
                
                if (VARIABLE.AdditionalPart)
                {
                    if (VARIABLE.AdditionalPart.Level < level)
                    {
                        part = VARIABLE.AdditionalPart;
                        toSpawn = part.Type.GetPart(level);
                    
                        VARIABLE.UnRegistryAdditional();
                        part.DestroyPart();
                    
                        SpawnAdditionalPartToCell(toSpawn, VARIABLE);
                    }
                }
            }
        }
    }

    public void SpawnPartToCell(Part partPref, GridCell cell, bool merged = false)
    {
        Part part = Instantiate(partPref);
        
        cell.Registry(part);
        part._placedOnGrid = false;
        part.SetGrid(cell);
        
        SetDefaultPartParsOnGrid(part);
        
        // OnAddPart?.Invoke(part, merged);
    }
    
    public void SpawnAdditionalPartToCell(Part partPref, GridCell cell, bool merged = false)
    {
        Part part = Instantiate(partPref);
        
        cell.RegistryAdditional(part);
        part.SetAdditionalGrid(cell);
        
        SetDefaultPartParsOnGrid(part, false, PartOrientation.Front);

        // OnAddPart?.Invoke(part, merged);
    }

    public void SetDefaultPartParsOnGrid(Part part, bool defaultOrientation = true, PartOrientation ownOrientation = PartOrientation.Default)
    {
        int index = _cells.IndexOf(part.GetGridCell());
        Vector3 pos = GetRequireLocalPosition(index);

        if (defaultOrientation)
        {
            if(UpdateRequireOrientationByPart(part, part.GetGridCell())) part.SetOrientation(requireOrientations[0]);
            else part.SetOrientation(part.Orientation);
        }
        else
        {
            part.SetOrientation(ownOrientation);
        }
        
        part.SetDefaultParsOnGrid(pos, parentForParts);
        
        part._placedOnGrid = false;
        // part.AddMod();
    }
    
    public void UpdatePartParsOnGrid(Part part, GridCell cell)
    {
        int index = _cells.IndexOf(cell);
        Vector3 pos = GetRequireLocalPosition(index);
        
        part.SetDefaultParsOnGrid(pos, parentForParts);
    }

    public void UpdatePartParsOnGridOnlyPos(Part part, GridCell cell, bool blockOrientation = false)
    {
        int index = _cells.IndexOf(cell);
        Vector3 pos = GetRequireLocalPosition(index);

        if (UpdateRequireOrientationByPart(part, cell) && !blockOrientation) part.SetOrientation(requireOrientations[0]);
        else part.SetOrientation(part.Orientation);
        
        part.SetDefaultParsOnGridOnlyPos(pos, parentForParts);
    }
    
    public void UpdatePartParsOnGridOnlyRot(Part part, GridCell cell, bool blockOrientation = false)
    {
        if (UpdateRequireOrientationByPart(part, cell) && !blockOrientation) part.SetOrientation(requireOrientations[0]);
        else part.SetOrientation(part.Orientation);
        
        part.SetDefaultParsOnGridOnlyRot(parentForParts);
    }
    
    public bool UpdateRequireOrientationByPart(Part part, GridCell cell)
    {
        requireOrientations = GetAvailableOrientations(part, cell);

        // foreach (var VARIABLE in requireOrientations)
        // {
        //     Debug.Log(part.name + ": " + VARIABLE);
        // }
        
        return requireOrientations.Length > 0;
    }
    
    public bool HaveNeighbors(Part part, GridCell cell, List<PartCategory> typeExceptation = null, bool ignoreBlock = false)
    {
        bool value = false;
        GridCell[] neighbors = GetNeighborCellsIndex(part, cell, ignoreBlock);
        
        foreach (var VARIABLE in neighbors)
        {
            // Debug.Log(part.Type.Category + ", " + _cells.IndexOf(cell) + " - " + VARIABLE);
            if (VARIABLE && VARIABLE.Part && (VARIABLE != part.GetGridCell() || VARIABLE.AdditionalPart == part) 
                && VARIABLE.Part != part && (typeExceptation == null || !typeExceptation.Contains(VARIABLE.Part.Type.Category)
                && VARIABLE.Part.Type.Category != PartCategory.Cabin && VARIABLE.Part.Type.Category != PartCategory.Grid))
            {
                value = true;
                break;
            }
        }

        return value;
    }
    
    public bool HaveRequireNeighbors(Part part, GridCell cell, PartCategory requireCategory, bool ignoreBlock = false)
    {
        bool value = false;
        GridCell[] neighbors = GetNeighborCellsIndex(part, cell, ignoreBlock);
        
        foreach (var VARIABLE in neighbors)
        {
            if (VARIABLE && VARIABLE.Part && VARIABLE.Part != part && requireCategory == VARIABLE.Part.Type.Category)
            {
                value = true;
                break;
            }
        }

        return value;
    }
    
    public bool HaveNeighbors(Part part, GridCell cell, out Part heighbor, bool ignoreblock = false)
    {
        bool value = false;
        heighbor = null;
        
        GridCell[] neighbors = GetNeighborCellsIndex(part, cell, ignoreblock);
        
        foreach (var VARIABLE in neighbors)
        {
            // Debug.Log(part.Type.Category + ", " + _cells.IndexOf(cell) + " - " + VARIABLE);
            if (VARIABLE && VARIABLE.Part && (VARIABLE != part.GetGridCell() || VARIABLE.AdditionalPart == part) && VARIABLE.Part != part)
            {
                if(VARIABLE.Part.Type.Category != PartCategory.Grid && VARIABLE.Part.Type.Category != PartCategory.Cabin) continue;

                heighbor = VARIABLE.Part;
                value = true;
                break;
            }
        }
        
        return value;
    }

    public bool HaveOrientation(OrientationParameters orients)
    {
        if (requireOrientations.Length == 0) return false;
        
        bool value = false;

        foreach (var VARIABLE in requireOrientations)
        {
            if (orients.HaveOrientation(VARIABLE))
            {
                value = true;
                break;
            }
        }
        
        return value;
    }

    private PartOrientation[] GetAvailableOrientations(Part dragPart, GridCell cell, bool ignoreblock = false)
    {
        List<PartOrientation> list = new List<PartOrientation>();
        GridCell[] cells = GetNeighborCellsIndex(dragPart, cell, ignoreblock);

        if(cells[4] && dragPart.Orientations.HaveOrientation(PartOrientation.Front)) list.Add(PartOrientation.Front);
        
        if(cells[1] && dragPart.Orientations.HaveOrientation(PartOrientation.Right)) list.Add(PartOrientation.Right);
        if(cells[0] && dragPart.Orientations.HaveOrientation(PartOrientation.Left)) list.Add(PartOrientation.Left);
        if(cells[3] && dragPart.Orientations.HaveOrientation(PartOrientation.Bottom)) list.Add(PartOrientation.Bottom);
        if(cells[2] && dragPart.Orientations.HaveOrientation(PartOrientation.Top)) list.Add(PartOrientation.Top);

        
        return list.ToArray();
    }

    private GridCell[] GetNeighborCellsIndex(Part dragPart, GridCell cell, bool ignoreblock)
    {
        GridCell[] cells = new GridCell[5];

        int index = _cells.IndexOf(cell);
        if (index == -1) return cells;

        if (CheckIndex(index + 1))
        {
            if ((index + 1) % Size > 0 && _cells[index + 1].Part
                && (!_cells[index + 1].Part.Orientations.Block.Left || ignoreblock)) cells[0] = _cells[index + 1]; //left
        }

        if (CheckIndex(index - 1))
        {
            if((index - 1) % Size < Size - 1 && _cells[index - 1].Part
                && (!_cells[index - 1].Part.Orientations.Block.Right || ignoreblock)) cells[1] = _cells[index - 1]; //right
        }

        if (CheckIndex(index + Size))
        {
            if(_cells[index + Size].Part 
               && (!_cells[index + Size].Part.Orientations.Block.Top || ignoreblock)) cells[2] = _cells[index + Size]; //top
        }

        if (CheckIndex(index - Size))
        {
            if(_cells[index - Size].Part
               && (!_cells[index - Size].Part.Orientations.Block.Bottom || ignoreblock)) cells[3] = _cells[index - Size]; //bottom
        }

        Part frontPart = cell.Part;
        
        if (frontPart)
        {
            if(!frontPart.Orientations.Block.Front 
               && (!frontPart._currentGridCell.AdditionalPart || frontPart._currentGridCell.AdditionalPart == dragPart) && frontPart != dragPart)
            {
                cells[4] = _cells[index];
            }
        }

        return cells;
    }
    
    bool CheckIndex(int index) => index == Mathf.Clamp(index, 0, _cells.Length - 1);
    
    public Vector3 GetRequireLocalPosition(int index)
    {
        Vector2Int pos = Vector2Int.zero;

        pos.x = (MainIndex % Size - index % Size) * Space;
        pos.y = (MainIndex / Size - index / Size) * Space;

        return new Vector3(0, pos.y, pos.x);
    }
}