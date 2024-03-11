using System.Collections;
using System.Collections.Generic;
using AppsFlyerSDK;
using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class Tutorial : MonoBehaviour
{
    [Inject] public static Tutorial Instance { get; private set; }
    
    public bool Completed => IterationsCompleted >= 4;
    
    public int IterationsCompleted
    {
        get => PlayerPrefs.GetInt("IterationsCompleted", 0);
        set
        {
            PlayerPrefs.SetInt("IterationsCompleted", value);
            PlayerPrefs.Save();
        }
    }
    
    public void StartTutorial()
    {
        if (IterationsCompleted == 0)
        {
            Instance.FirstIteration();
        }
        else if (IterationsCompleted == 1 && PartUnlocked.Wheels)
        {
            Instance.SecondIteration();
        }
        else if (IterationsCompleted == 2 && PartUnlocked.Wings)
        {
            Instance.ThirdIteration();
        }
        else if (IterationsCompleted == 3 && PartUnlocked.Grids)
        {
            Instance.FourthIteration();
        }
        else if (Completed)
        {
            Instance.gameObject.SetActive(false);
            Instance = null;
        }
    }
    
    [Space]
    [SerializeField] private Transform hand;
    [SerializeField] private float doTime = 0.75f;

    [Space] 
    [SerializeField] private CinemachineBrain brain;
    [SerializeField] private PartType wheels, fans, wings;
    [SerializeField] private UpgradeObject launch, currency, level;
    
    [Space] 
    [SerializeField] private GameObject[] layers;

    void SetLayer(int index)
    {
        foreach (var VARIABLE in layers)
        {
            VARIABLE.SetActive(false);
        }

        if (index >= 0 && index < layers.Length)
        {
            SetCellLayer(-1);
            layers[index].SetActive(true);
        }
    }
    
    [Space]
    [SerializeField] private Button play1Button;
    [SerializeField] private Button play2Button, mergeButton, buyButton, backButton, restartButton, settingsButton;

    [Space]
    [SerializeField] private Transform mergePos;
    [SerializeField] private Transform playPos;
    [SerializeField] private Transform buyPos;
    [SerializeField] private Transform launchPos, currencyPos, levelPos;
    [SerializeField] private Transform grid1Pos, grid2Pos;

    [Space] 
    [SerializeField] private GameObject layersBG;
    [SerializeField] private GameObject[] layersCells;

    void SetCellLayer(int index)
    {
        if (index < 0 || index >= layersCells.Length)
        {
            layersBG.SetActive(false);
            foreach (var VARIABLE in layersCells)
            {
                VARIABLE.SetActive(false);
            }

            return;
        }
        
        SetLayer(-1);
        
        layersBG.SetActive(true);
        layersCells[index].SetActive(true);
    }
    
    [SerializeField] private Transform[] cellsPoses;
    [SerializeField] private Transform detailPosWheel, detailPosFan, detailPosWings;

    [Space] 
    [SerializeField] private GameObject rotateObj;
    [SerializeField] private Transform rotatePos1, rotatePos2;

    int GetIndexOfPartOnMerge(PartCategory type, int lvl = -1, int indexIgnored = -1)
    {
        int pos = -1;
        MergeCell cell;

        for (int i = 0; i < MergeGrid.Instance._cells.Length; i++)
        {
            cell = MergeGrid.Instance._cells[i];
            if (cell && cell.Part && cell.Part.Type.Category == type
                && (lvl == -1 || cell.Part.Level == lvl)
                && (indexIgnored == -1 || i != indexIgnored))
            {
                pos = i;
                break;
            }
        }
        
        return pos;
    }

    Part GetPartOnGrid(int index)
    {
        return PlayerGrid.Instance._cells[index].Part;
    }

    [Inject] void Awake()
    {
        Instance = this;
        
        if (Completed)
        {
            gameObject.SetActive(false);
        }
    }

    private bool MergeFieldHaveType(PartCategory type, int count, int lvl = 0) => MergeGrid.Instance.HavePartOfType(type, lvl) >= count;
    private bool PlayerGridHaveType(PartCategory type, int count, int lvl = 0) => PlayerGrid.Instance.HavePartOfType(type, lvl) >= count;
    
    private async void FirstIteration()
    {
        PlayerGrid.Instance.ClearMergeParts();
        MergeGrid.Instance.ClearAll();
        
        Part.SetBlock(true);
        
        SetAllButtons(false);

        int index;
        Transform target;

        HandScale(mergePos.position);
        SetLayer(0);

        await UniTask.WaitUntil(() => brain.IsBlending);

        HandOff();
        SetLayer(-1);

        await UniTask.Delay(100);
        await UniTask.WaitUntil(() => !brain.IsBlending);

        if (!PlayerGridHaveType(PartCategory.Boost, 1))
        {
            if (!MergeFieldHaveType(PartCategory.Boost, 1))
            {
                BuyPart.SetPartToBuy(fans);
                buyButton.interactable = true;

                HandScale(buyPos.position);
                SetLayer(1);
                
                Gold.Instance.Plus(Mathf.Clamp(BuyPart.Cost - Gold.Instance.Value, 0, 999));
                await UniTask.WaitUntil(() => MergeFieldHaveType(PartCategory.Boost, 1));
            }

            buyButton.interactable = false;

            index = GetIndexOfPartOnMerge(PartCategory.Boost);
            target = cellsPoses[index];
            
            HandMove(target.position, detailPosFan.position);
            SetCellLayer(index);
            
            Part.SetBlock(false);

            await UniTask.WaitUntil(() => PlayerGridHaveType(PartCategory.Boost, 1));
        }
        
        BuyPart.NullPartToBuy();

        HandScale(playPos.position);
        SetLayer(5);

        play2Button.interactable = true;
        
        Part.SetBlock(true);

        await UniTask.WaitUntil(() => GameManager.Instance.GameStarted);
        
        Part.SetBlock(false);

        HandOff();
        SetLayer(-1);

        SetAllButtons(true);

        IterationsCompleted = 1;
        
        AppsFlyerEventsSuite.AF_TUTORIAL_COMPLETION("MAIN", "ID-01", "Boost-Tutorial");
    }
    
    private async void SecondIteration()
    {
        MergeGrid.Instance.ClearAll();
        Part.SetBlock(true);
        
        SetAllButtons(false);

        int index;
        Transform target;
        
        mergeButton.interactable = false;

        if (launch.Level == 0)
        {
            Gem.Instance.Plus(Mathf.Clamp(launch.Cost - Gem.Instance.Value, 0, 999));
            
            HandScale(launchPos.position);
            SetLayer(2);

            await UniTask.WaitUntil(() => launch.Level > 0);
        }
        else if (currency.Level == 0)
        {
            Gem.Instance.Plus(Mathf.Clamp(currency.Cost - Gem.Instance.Value, 0, 999));
            
            HandScale(currencyPos.position);
            SetLayer(3);
            
            await UniTask.WaitUntil(() => currency.Level > 0);
        }
        else if (level.Level == 0)
        {
            Gem.Instance.Plus(Mathf.Clamp(level.Cost - Gem.Instance.Value, 0, 999));
            
            HandScale(levelPos.position);
            SetLayer(4);
            
            await UniTask.WaitUntil(() => level.Level > 0);
        }

        HandScale(mergePos.position);
        SetLayer(0);
        
        mergeButton.interactable = true;

        await UniTask.WaitUntil(() => brain.IsBlending);

        HandOff();
        SetLayer(-1);

        await UniTask.Delay(100);
        await UniTask.WaitUntil(() => !brain.IsBlending);

        if (!PlayerGridHaveType(PartCategory.Wheels, 1, 1))
        {
            MergeGrid.Instance.SpawnPart(wheels.GetPart(0));
            
            if (!MergeFieldHaveType(PartCategory.Wheels, 2))
            {
                BuyPart.SetPartToBuy(wheels);
                buyButton.interactable = true;

                HandScale(buyPos.position);
                SetLayer(1);
                
                Gold.Instance.Plus(Mathf.Clamp(BuyPart.Cost - Gold.Instance.Value, 0, 999));
                await UniTask.WaitUntil(() => MergeFieldHaveType(PartCategory.Wheels, 2));
            }
            
            Part.SetBlock(false);

            if (!MergeFieldHaveType(PartCategory.Wheels, 1, 1))
            {
                buyButton.interactable = false;

                Transform trg1, trg2;
                
                index = GetIndexOfPartOnMerge(PartCategory.Wheels, 0);
                trg1 = cellsPoses[index];
                SetCellLayer(index);

                index = GetIndexOfPartOnMerge(PartCategory.Wheels, 0, index);
                trg2 = cellsPoses[index];
                SetCellLayer(index);
                
                HandMove(trg1.position, trg2.position);
                await UniTask.WaitUntil(() => MergeFieldHaveType(PartCategory.Wheels, 1, 1));
            }

            index = GetIndexOfPartOnMerge(PartCategory.Wheels, 1);
            target = cellsPoses[index];
            
            HandMove(target.position, detailPosWheel.position);
            SetCellLayer(index);

            await UniTask.WaitUntil(() => PlayerGridHaveType(PartCategory.Wheels, 1, 1));
        }
        
        BuyPart.NullPartToBuy();

        // HandScale(playPos.position);
        // SetLayer(5);
        //
        // play2Button.interactable = true;
        //
        // Part.SetBlock(true);
        //
        // await UniTask.WaitUntil(() => GameManager.GameStarted);
        
        Part.SetBlock(false);

        HandOff();
        SetCellLayer(-1);
        SetLayer(-1);

        SetAllButtons(true);

        IterationsCompleted = 2;
        
        AppsFlyerEventsSuite.AF_TUTORIAL_COMPLETION("MAIN", "ID-03", "Wheels-Tutorial");
    }
    
    private async void ThirdIteration()
    {
        MergeGrid.Instance.ClearAll();
        Part.SetBlock(true);
        
        SetAllButtons(false);

        int index;
        Transform target;

        HandScale(mergePos.position);
        SetLayer(0);

        await UniTask.WaitUntil(() => brain.IsBlending);

        HandOff();
        SetLayer(-1);

        await UniTask.Delay(100);
        await UniTask.WaitUntil(() => !brain.IsBlending);

        if (!PlayerGridHaveType(PartCategory.Wings, 1))
        {
            if (!MergeFieldHaveType(PartCategory.Wings, 1))
            {
                BuyPart.SetPartToBuy(wings);
                buyButton.interactable = true;

                HandScale(buyPos.position);
                SetLayer(1);
                
                Gold.Instance.Plus(Mathf.Clamp(BuyPart.Cost - Gold.Instance.Value, 0, 999));
                await UniTask.WaitUntil(() => MergeFieldHaveType(PartCategory.Wings, 1));
            }

            buyButton.interactable = false;

            index = GetIndexOfPartOnMerge(PartCategory.Wings);
            target = cellsPoses[index];
            
            HandMove(target.position, detailPosWings.position);
            SetCellLayer(index);
            
            Part.SetBlock(false);

            await UniTask.WaitUntil(() => PlayerGridHaveType(PartCategory.Wings, 1));
        }
        
        BuyPart.NullPartToBuy();

        // HandScale(playPos.position);
        // SetLayer(5);
        //
        // play2Button.interactable = true;
        //
        // Part.SetBlock(true);
        //
        // await UniTask.WaitUntil(() => GameManager.GameStarted);
        
        Part.SetBlock(false);

        HandOff();
        SetCellLayer(-1);
        SetLayer(-1);

        SetAllButtons(true);

        IterationsCompleted = 3;
        
        AppsFlyerEventsSuite.AF_TUTORIAL_COMPLETION("MAIN", "ID-04", "Wings-Tutorial");
    }
    
    private async void FourthIteration()
    {
        PlayerGrid.Instance.ClearMergeParts();
        Part.SetBlock(true);
        
        SetAllButtons(false);

        int index;
        Transform target;

        HandScale(mergePos.position);
        SetLayer(0);

        await UniTask.WaitUntil(() => brain.IsBlending);

        HandOff();
        SetLayer(-1);

        await UniTask.Delay(100);
        await UniTask.WaitUntil(() => !brain.IsBlending);

        layersBG.SetActive(true);
        index = PlayerGrid.Instance.MainIndex;
        int index1 = index + 1;
        int index2 = index - 1;

        if (GetPartOnGrid(index1))
        {
            HandMove(grid1Pos.position, grid2Pos.position);
            // await UniTask.WaitUntil(() => GetPartOnGrid(index2));
        }
        else if (GetPartOnGrid(index2))
        {
            HandMove(grid2Pos.position, grid1Pos.position);
            // await UniTask.WaitUntil(() => GetPartOnGrid(index1));
        }
        
        await UniTask.WaitUntil(() => Input.GetMouseButtonUp(0));

        layersBG.SetActive(false);
        HandOff();
        SetCellLayer(-1);
        SetLayer(-1);

        SetAllButtons(true);

        IterationsCompleted = 4;
        
        AppsFlyerEventsSuite.AF_TUTORIAL_COMPLETION("MAIN", "ID-05", "Additional-Block-Tutorial");
    }

    public bool PlaneIterationComplete
    {
        get => PlayerPrefs.GetInt("PlaneIterationsCompleted", 0) != 0;
        set
        {
            PlayerPrefs.SetInt("PlaneIterationsCompleted", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public  void PlaneIteration()
    {
        if (!PlaneIterationComplete) _PlaneIteration();
    }

    private async void _PlaneIteration()
    {
        SetAllButtons(false);
        SetLayer(-1);

        Time.timeScale = 0;
        
        rotateObj.SetActive(true);
        HandMove(rotatePos1.position, rotatePos2.position);

        await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0));
        
        rotateObj.SetActive(false);
        
        Time.timeScale = 1;
        
        HandOff();
        SetAllButtons(true);

        PlaneIterationComplete = true;
        
        AppsFlyerEventsSuite.AF_TUTORIAL_COMPLETION("MAIN", "ID-2", "Plane-Control-Tutorial");
    }

    void SetAllButtons(bool state)
    {
        play1Button.interactable = state;
        play2Button.interactable = state;
        backButton.interactable = state;
        buyButton.interactable = state;
        restartButton.interactable = state;
        settingsButton.interactable = state;
    }

    void HandMove(Vector3 first, Vector3 second)
    {
        hand.DOKill();
        hand.gameObject.SetActive(true);
        
        hand.position = first;
        hand.localScale = Vector3.one;
        hand.DOMove(second, doTime)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true);
    }

    void HandScale(Vector3 pos)
    {
        hand.DOKill();
        hand.gameObject.SetActive(true);
        
        hand.position = pos;
        hand.localScale = Vector3.one;
        hand.DOScale(Vector3.one * 0.75f, doTime)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true);
    }

    void HandOff()
    {
        hand.DOKill();
        hand.gameObject.SetActive(false);
    }
}
