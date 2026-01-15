using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class DraggableUnitCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image _veil;
    [SerializeField] private Image _unitImage;
    [SerializeField] private TextMeshProUGUI _costText;

    private TowerEntry _towerData;

    public void SetData(TowerEntry towerData)
    {
        _towerData = towerData;

        if (!GameManager.Addressables.TryGet<Sprite>(_towerData.SPRITE_PATH, out var sprite))
        {
            Debug.Log($"{_towerData.SPRITE_PATH} ¾øÀ½");
            _unitImage.gameObject.SetActive(false);
            return;
        }
        _unitImage.gameObject.SetActive(true);
        _unitImage.sprite = sprite;
        _unitImage.SetNativeSize();
        _costText.text = towerData.COST.ToString();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_towerData == null) return;
        if(BattleManager.Instance.Cost < _towerData.COST) return;
        if (BattleManager.Instance.IsAtPlacementCapacity()) return;
        UnitPlacementManager.Instance.StartDrad(_towerData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        UnitPlacementManager.Instance.UpdateDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        UnitPlacementManager.Instance.EndDrag(eventData);
    }

    public void SetVeilCard(int cost)
    {
        if (_towerData == null) return;
        _veil.gameObject.SetActive(_towerData.COST > cost);
    }
}
