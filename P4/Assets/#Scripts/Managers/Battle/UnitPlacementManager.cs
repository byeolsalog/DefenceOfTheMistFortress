using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnitPlacementManager : MonoBehaviour
{
    private static UnitPlacementManager _instance;
    public static UnitPlacementManager Instance => _instance;
    [SerializeField] private ShowUnitData _unitDataUI;

    private TowerEntry _selectedUnitData;
    private GameObject _ghostUnit;
    private SpriteRenderer _ghostSpriteRenderer;
    private Vector3Int _lastValidCell;
    private bool _isPlaceable = false;
    private List<ClickableTileItem> _highlightedTiles = new();
    private List<ClickableTileItem> _activeRangeTiles = new();    

    private void Awake()
    {
        if(_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        _unitDataUI.SetCallback(ConfirmPlacement, CancelPlacement);
    }

    public void OnUnitClicked(Tower unit)
    {
        if(unit== null) return;
        _unitDataUI.SetData(unit);

        _unitDataUI.gameObject.SetActive(true);
        _unitDataUI.SetActiveUnitData(true);
    }

    public void StartDrad(TowerEntry towerData)
    {
        if (_ghostUnit != null) Destroy(_ghostUnit);

        _selectedUnitData = towerData;

        if (!GameManager.Addressables.TryGet<UnityEngine.Object>(_selectedUnitData.PREFAB_PATH, out var prefab))
        {
            Debug.LogError($"프리팹 로드 실패: {_selectedUnitData.PREFAB_PATH}");
            return;
        }

        if (prefab == null) return;

        _ghostUnit = GameObject.Instantiate(prefab) as GameObject;
        _ghostSpriteRenderer = _ghostUnit.GetComponentInChildren<SpriteRenderer>();

        Tower tower = _ghostUnit.GetComponent<Tower>();
        if (tower != null) 
            tower.enabled = false;

        foreach (var col in _ghostUnit.GetComponentsInChildren<Collider2D>()) 
            col.enabled = false;

        ShowPlaceableTiles();
        UpdateDrag();
    }

    public void UpdateDrag(PointerEventData eventData = null)
    {
        if (_ghostUnit == null) return;

        Vector3 worldPos = TileMapReader.Instance.GetCamera().ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;

        Vector3Int absoluteCell = TileMapReader.Instance.GetIndexCellFromWorldPos(worldPos);
        if (absoluteCell.x < 0) return;

        _ghostUnit.transform.position = TileMapReader.Instance.GetWorldPosFromIndexCell(absoluteCell);

        var unitScript = _ghostUnit.GetComponent<Unit>();
        if (unitScript != null)        
            unitScript.LookAtTarget(TileMapReader.Instance.GetFirstSpawnWorldPos());
        
        _isPlaceable = CheckPlacement(absoluteCell);
        
        if (_ghostSpriteRenderer != null)
        {
            _ghostSpriteRenderer.color = _isPlaceable ? Color.green : Color.red;
        }

        ShowRange(absoluteCell, true);

        _lastValidCell = absoluteCell;
        BattleManager.Instance.CurrentBattleSpeedMode = EBattleSpeedMode.Placement;
    }

    public void EndDrag(PointerEventData eventData)
    {
        if (_ghostUnit == null) return;

        if ((eventData.pointerEnter != null && eventData.pointerEnter.layer == LayerMask.NameToLayer("UI")) || !_isPlaceable)
        {
            CancelPlacement();
            return;
        }

        if (_selectedUnitData != null && BattleManager.Instance.Cost < _selectedUnitData.COST)
        {
            CancelPlacement();
            return;
        }

        ShowConfirmationUI();
    }

    private void ShowConfirmationUI()
    {
        CameraZoomController.Instance.FocusOnPosition(_ghostUnit.transform.position);
        _unitDataUI.gameObject.SetActive(true);
        ClearPlaceableTiles();
    }

    private void ConfirmPlacement()
    {
        if (_ghostUnit == null) return;

        BattleManager.Instance.Cost -= _selectedUnitData.COST;

        if (_ghostSpriteRenderer != null)
            _ghostSpriteRenderer.color = Color.white;

        var tower = _ghostUnit.GetComponent<Tower>();
        tower.enabled = true;
        tower.Init(_lastValidCell, _selectedUnitData);

        foreach (var col in _ghostUnit.GetComponentsInChildren<Collider2D>())
        {
            col.enabled = true;
        }

        Cleanup();        
    }

    public void CancelPlacement()
    {
        if (_ghostUnit != null)
        {
            Destroy(_ghostUnit);
        }
        Cleanup();
    }

    private void Cleanup()
    {
        ShowRange(Vector3Int.zero, false);
        ClearPlaceableTiles();

        _ghostUnit = null;
        _ghostSpriteRenderer = null;
        _selectedUnitData = null;
    }

    private bool CheckPlacement(Vector3Int absoluteCell)
    {
        bool isTilePlaceable = TileMapReader.Instance.IsPlaceable(absoluteCell, _selectedUnitData.UNIT_TYPE);
        bool isCellEmpty = (BattleManager.Grid.GetTowerInCell(absoluteCell) == null);
        return isTilePlaceable && isCellEmpty;
    }

    private void ShowPlaceableTiles()
    {
        ClearPlaceableTiles();

        var allTiles = TileMapReader.Instance.GetAllClickableTiles();
        if (allTiles == null) return;

        foreach (var entry in allTiles)
        {
            Vector3Int cell = new Vector3Int(entry.Key.x, entry.Key.y, 0);
            ClickableTileItem tileItem = entry.Value._tileItem;
            if (CheckPlacement(cell))
            {
                tileItem.SetHighlightColor(new Color(0.5f, 1f, 0.5f, 0.5f));
                _highlightedTiles.Add(tileItem);
            }
            else
            {
                tileItem.SetHighlightColor(new Color(1f, 0.5f, 0.5f, 0.5f));
                _highlightedTiles.Add(tileItem);
            }
        }
    }

    private void ClearPlaceableTiles()
    {
        foreach (var item in _highlightedTiles)
        {
            if (item != null)
                item.ResetColor();
        }
        _highlightedTiles.Clear();
    }

    public void ShowAttackRange(Tower tower = null)
    {
        if(tower == null)
        {
            ShowRange(Vector3Int.zero, false);
            return;
        }

        _selectedUnitData = tower.GetTowerData();
        ShowRange(BattleManager.Grid.GetTowerCell(tower), true);
    }

    private void ShowRange(Vector3Int centerCell, bool isActive, TowerEntry entry = null)
    {
        if (entry == null)
            entry = _selectedUnitData;

        if (entry == null && _selectedUnitData == null)
            return;

        if (!isActive)
        {
            foreach (var tile in _activeRangeTiles)
            {
                if (tile != null)
                    tile.SetActiveRangeSlot(false);
            }
            _activeRangeTiles.Clear();
            return;
        }

        foreach (var tile in _activeRangeTiles)
        {
            if (tile != null)
                tile.SetActiveRangeSlot(false);
        }
        _activeRangeTiles.Clear();

        foreach (var offset in entry.ParsedAttackRange)
        {
            Vector2Int cell = new Vector2Int(
                centerCell.x + offset.x,
                centerCell.y + offset.y
            );

            var tiles = TileMapReader.Instance.GetAllClickableTiles();
            if (tiles.TryGetValue(cell, out CellData cellData))
            {
                var tileItem = cellData._tileItem;
                tileItem.SetActiveRangeSlot(true);
                _activeRangeTiles.Add(tileItem);
            }
        }
    }
}
