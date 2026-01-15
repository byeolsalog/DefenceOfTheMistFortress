using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    private static BattleManager _instance;
    public static BattleManager Instance => _instance;

    private GridManager _grid;
    private PoolManager _pool;

    public static GridManager Grid => Instance.GetOrCreateManager(ref Instance._grid);
    public static PoolManager Pool => Instance.GetOrCreateManager(ref Instance._pool);


    [SerializeField] private Transform _unitCardParent;
    [SerializeField] private TextMeshProUGUI _costText;
    [SerializeField] private TextMeshProUGUI _placementCapacityText;
    [SerializeField] private TextMeshProUGUI _currentMonsterCountText;
    [SerializeField] private TextMeshProUGUI _killCountText;
    [SerializeField] private TextMeshProUGUI _lifeCountText;

    [SerializeField] private GameObject _result;
    [SerializeField] private GameObject _clearText;
    [SerializeField] private GameObject _failText;

    [SerializeField] private Image _fadeImage;
    [SerializeField] private TextMeshProUGUI _stageFadeText;

    [SerializeField] private Image _speedModeIcon;

    private Dictionary<EBattleSpeedMode, float> _battleSpeedModes = new Dictionary<EBattleSpeedMode, float>()
    {
        { EBattleSpeedMode.Normal, 0.7f },
        { EBattleSpeedMode.Placement, 0.1f },
        { EBattleSpeedMode.PausedOrEnd, 0.0f },
        { EBattleSpeedMode.Fast, 1.0f }
    };

    private const string SPEED_NORMAL_ICON = "Sprites/SpeedNormalIcon.png";
    private const string SPEED_FAST_ICON = "Sprites/SpeedModeIcon.png";
    private const string POPUP_OPTION_UI = "Prefabs_Common/PopupPrefabs/Option.prefab";

    private EBattleSpeedMode _originSpeedMode = EBattleSpeedMode.Normal;
    private EBattleSpeedMode _currentBattleSpeedMode = EBattleSpeedMode.Normal;
    public EBattleSpeedMode CurrentBattleSpeedMode
    {
        get => _currentBattleSpeedMode;
        set
        {
            if(_currentBattleSpeedMode == EBattleSpeedMode.Normal || _currentBattleSpeedMode == EBattleSpeedMode.Fast)
                _originSpeedMode = _currentBattleSpeedMode;

            _currentBattleSpeedMode = value;

            Sprite sprite = null;
            switch (_currentBattleSpeedMode)
            {
                case EBattleSpeedMode.Fast:
                    GameManager.Addressables.TryGet<Sprite>(SPEED_FAST_ICON, out sprite);                    
                    break;

                case EBattleSpeedMode.Normal:
                default:
                    GameManager.Addressables.TryGet<Sprite>(SPEED_NORMAL_ICON, out sprite);
                    break;
            }

            if(sprite != null) _speedModeIcon.sprite = sprite;
            Time.timeScale = _battleSpeedModes[_currentBattleSpeedMode];
        }
    }

    private event Action<int> OnVeilUnitCardByCost;
    private float _cost = 0;
    public float Cost 
    {
        get => _cost;
        set
        {
            _cost = value;
            _costText.text = "cost".GetLanguage(((int)_cost).ToString());
        }
    }

    private int _placementCapacity = 0;    
    private int _currentPlacementCount = 0;
    public int CurrentPlacementCount
    {
        get => _currentPlacementCount;
        set
        {
            _currentPlacementCount = value;
            _placementCapacityText.text = "unit_placement_capacity".GetLanguage(_currentPlacementCount, _placementCapacity);
        }
    }


    private int _currentMonsterCount = 0;
    public int CurrentMonsterCount
    {
        get => _currentMonsterCount;
        set
        {
            _currentMonsterCount = value;
            int temp = RemainMonsterCount;
            RemainMonsterCount = Mathf.Max(0, temp - _currentMonsterCount);
            _currentMonsterCountText.text = _currentMonsterCount.ToString();
        }
    }

    private int _killCount = 0;
    public int KillCount
    {
        get => _killCount;
        set
        {
            _killCount = value;
            int temp = RemainMonsterCount;
            RemainMonsterCount = Mathf.Max(0, temp - _killCount);
            _killCountText.text = "monster_count".GetLanguage(_killCount, _totalMonsterCount);
        }
    }

    private int _lifeCount = 0;
    public int LifeCount
    {
        get => _lifeCount;
        set
        {
            _lifeCount = value;
            _lifeCountText.text = _lifeCount.ToString();

            if(_lifeCount <= 0)
            {
                // Game Over
                GameFail();
            }
        }
    }

    private int _totalMonsterCount = 0;
    private int _remainMonsterCount = 0;
    public int RemainMonsterCount
    {
        get => _remainMonsterCount;
        set
        {
            _remainMonsterCount = value;
            if(_remainMonsterCount <= 0 && CurrentMonsterCount <= 0 && LifeCount > 0)
            {
                // Clear
                CurrentBattleSpeedMode = EBattleSpeedMode.PausedOrEnd;
                _result.SetActive(true);
                _clearText.SetActive(true);
                _failText.SetActive(false);
                GameManager.Data.StageData.SetMaxData();
                GameManager.Data.SaveData(EFileDataType.Stage);
            }
        }
    }

    public bool IsBattleReady = false;

    private void Awake()
    {
        if(_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        
        OnVeilUnitCardByCost = null;
        Cost = 0;

        int diff = GameManager.Data.StageData.Diff;
        int stage = GameManager.Data.StageData.Stage;
        _totalMonsterCount = GameManager.Table.GetTable<WaveEventTable>().Get(diff, stage).FindAll(x => x.DIFFICULTY == diff && x.STAGE == stage).Sum(y => y.COUNT);
        RemainMonsterCount = _totalMonsterCount;
        LifeCount = GameManager.Table.GetTable<StageTable>().Get(diff, stage).LIFE_COUNT;

        _placementCapacity = GameManager.Table.GetTable<StageTable>().Get(diff, stage).PLACEMENT_CAPACITY;
        CurrentPlacementCount = 0;

        CurrentMonsterCount = 0;
        KillCount = 0;

        if (!GameManager.Addressables.TryGet<UnityEngine.Object>(Define.UNIT_CARD_PATH, out var prefab))
            return;

        var towerTable = GameManager.Table.GetTable<TowerTable>();
        var towerTableData = towerTable.GetAllData();
        foreach (var e in towerTableData.Values)
        {
            var obj = Instantiate(prefab, _unitCardParent) as GameObject;
            if (obj == null) continue;
            var item = obj.GetComponent<DraggableUnitCard>();
            if (item == null) continue;

            item.SetData(e);
            OnVeilUnitCardByCost += item.SetVeilCard;
        }

        CurrentBattleSpeedMode = EBattleSpeedMode.Normal;
        GameManager.Audio.StopBGM();
        GameManager.Audio.PlayBGM(EBGM.BattleBGM);
        StartCoroutine(CoBattleStartFade());
    }

    private T GetOrCreateManager<T>(ref T manager) where T : class, new()
    {
        if (manager == null)
            manager = new T();

        return manager;
    }


    private IEnumerator CoBattleStartFade()
    {
        float duration = 3f;
        float elapsedTime = 0f;

        _fadeImage.gameObject.SetActive(true);
        var stageData = GameManager.Data.StageData;
        _stageFadeText.text = $"{stageData.Diff} - {stageData.Stage}";

        Color startColor = _fadeImage.color;
        startColor.a = 1;
        _fadeImage.color = startColor;

        Color startTextColor = _stageFadeText.color;
        startTextColor.a = 1;
        _stageFadeText.color = startTextColor;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            Color newColor = _fadeImage.color;
            newColor.a = alpha;
            _fadeImage.color = newColor;

            Color newTextColor = _stageFadeText.color;
            newTextColor.a = alpha;
            _stageFadeText.color = newTextColor;
            yield return null;
        }

        Color endColor = _fadeImage.color;
        endColor.a = 0f;
        _fadeImage.color = endColor;

        Color endTextColor = _stageFadeText.color;
        endTextColor.a = 0f;
        _stageFadeText.color = endTextColor;

        _fadeImage.gameObject.SetActive(false);
        IsBattleReady = true;
    }

    public void OnClickEndBattleBtn()
    {
        CurrentBattleSpeedMode = EBattleSpeedMode.Fast;
        GameManager.Scene.LoadScene(EGameScene.Robby);
    }

    public void OnClickChangeSpeed()
    {
        CurrentBattleSpeedMode = _currentBattleSpeedMode == EBattleSpeedMode.Normal ? EBattleSpeedMode.Fast : EBattleSpeedMode.Normal;
    }

    public void OnClickOption()
    {
        _currentBattleSpeedMode = EBattleSpeedMode.PausedOrEnd;
        GameManager.Addressables.TryGet<UnityEngine.Object>(POPUP_OPTION_UI, out var prefab);
        var popup = prefab.Get<OptionUI>();
        GameManager.UI.ShowUI(popup);
    }

    public void ReturnToOriginSpeedMode()
    {
        CurrentBattleSpeedMode = _originSpeedMode;
    }

    public void GameFail()
    {
        GameManager.UI.CloseAllUIs();
        CurrentBattleSpeedMode = EBattleSpeedMode.PausedOrEnd;
        _result.SetActive(true);
        _clearText.SetActive(false);
        _failText.SetActive(true);
    }

    public bool IsAtPlacementCapacity()
    {
        return _currentPlacementCount >= _placementCapacity;
    }

    private void Update()
    {
        Cost += Define.DEFAULT_COST_SPEED * Time.deltaTime;
        OnVeilUnitCardByCost?.Invoke((int)Cost);
    }
}
