using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class ShowUnitData : MonoBehaviour, IPointerClickHandler
{   
    [SerializeField] private TextMeshProUGUI _hp;
    [SerializeField] private TextMeshProUGUI _atk;
    [SerializeField] private TextMeshProUGUI _def;
    [SerializeField] private TextMeshProUGUI _speed;
    [SerializeField] private TextMeshProUGUI _block;
    [SerializeField] private TextMeshProUGUI _cost;

    [SerializeField] private Button _acceptBtn;
    [SerializeField] private Button _cancelBtn;

    private bool _isPlacing = false;
    private Tower _currentUnit;         

    private void Awake()
    {
        SetActiveUnitData(false);
        _currentUnit = null;
    }

    private void OnEnable()
    {
        BattleManager.Instance.CurrentBattleSpeedMode = EBattleSpeedMode.Placement;
    }

    public void SetCallback(Action accept, Action cancel)
    {
        _acceptBtn.onClick.RemoveAllListeners();
        _cancelBtn.onClick.RemoveAllListeners();

        _acceptBtn.onClick.AddListener(() => 
        {
            accept?.Invoke();
            Close(); 
        });
        _cancelBtn.onClick.AddListener(() => 
        {
            if (_currentUnit == null)
            {
                cancel?.Invoke();
            }
            else
            {
                if (_currentUnit == null)
                    return;

                _currentUnit.Retreat();                
            }

            Close();
        });
    }

    public void SetData(Tower unit)
    {
        _currentUnit = unit;
        var data = unit.GetTowerData();
        if (data == null) return;

        _hp.text = $"HP : {unit.CurrentHealth}/{data.HP}";
        _atk.text = $"ATK : {unit.ATK}";
        _def.text = $"DEF : {unit.DEF}";
        _speed.text = $"SPEED : {unit.SPD}";

        int blockCount = unit is IBlocker ? (unit as IBlocker).GetBlockCapacity() : data.BLOCK_COUNT;
        _block.text = $"BLOCK : {blockCount}";
        _cost.text = $"COST : {data.COST}";
        UnitPlacementManager.Instance.ShowAttackRange(unit);
    }

    public void SetActiveUnitData(bool isActive)
    {
        _isPlacing = !isActive;
        _hp.gameObject.SetActive(isActive);
        _atk.gameObject.SetActive(isActive);
        _def.gameObject.SetActive(isActive);
        _speed.gameObject.SetActive(isActive);
        _block.gameObject.SetActive(isActive);
        _cost.gameObject.SetActive(isActive);
        _acceptBtn.gameObject.SetActive(_isPlacing);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isPlacing)
        {
            _cancelBtn.onClick?.Invoke();
        }
        Close();        
    }

    private void Close()
    {
        SetActiveUnitData(false);
        this.gameObject.SetActive(false);
        _currentUnit = null;
        CameraZoomController.Instance.ResetCamera();
        UnitPlacementManager.Instance.ShowAttackRange();
        BattleManager.Instance.ReturnToOriginSpeedMode();
    }
}
