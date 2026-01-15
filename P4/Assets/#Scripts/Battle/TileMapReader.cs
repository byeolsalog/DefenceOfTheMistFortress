using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CellData
{
    public Vector2Int _pos_index;
    public Vector2Int _pos_tile;
    public ETileType _tileType;
    public ClickableTileItem _tileItem;
}

public class TileMapReader : MonoBehaviour
{
    public class CellDataManager
    {
        private readonly Dictionary<Vector2Int, CellData> _indexSpawned = new();
        private readonly Dictionary<Vector2Int, CellData> _tileMapSpawned = new();

        public void AddSpwned(Vector2Int indexKey, Vector2Int tileMapKey, CellData value)
        {
            if (!_indexSpawned.ContainsKey(indexKey))
                _indexSpawned[indexKey] = value;

            if (!_tileMapSpawned.ContainsKey(tileMapKey))
                _tileMapSpawned[tileMapKey] = value;
        }
        public void Clear()
        {
            _indexSpawned.Clear();
            _tileMapSpawned.Clear();
        }

        public bool ContainsKeyByIndex(Vector2Int key)
        {
            return _indexSpawned.ContainsKey(key);
        }

        public bool ContainsKeyByTileMap(Vector2Int key)
        {
            return _tileMapSpawned.ContainsKey(key);
        }

        public bool TryGetByIndex(Vector2Int indexKey, out CellData cell)
        {
            return _indexSpawned.TryGetValue(indexKey, out cell);
        }

        public bool TryGetByTileMap(Vector2Int tileMapKey, out CellData cell)
        {
            return _tileMapSpawned.TryGetValue(tileMapKey, out cell);
        }

        public Dictionary<Vector2Int, CellData> GetAllIndexCells()
        {
            return _indexSpawned;
        }

        public Dictionary<Vector2Int, CellData> GetAllTileMapCells()
        {
            return _tileMapSpawned;
        }
    }


    public static TileMapReader Instance;

    [Header("Refs")]
    [SerializeField] private Camera cam;
    [SerializeField] private Tilemap targetTilemap;

    [Header("Default Visual")]
    [SerializeField] private bool fitToTile = true;
    [SerializeField] private Vector2 padding = Vector2.zero;

    private CellDataManager _spawned = new();

    private List<Vector3Int> _goalLocations = new List<Vector3Int>();
    private Dictionary<int, Vector3Int> _spawnLocations = new Dictionary<int, Vector3Int>();

    private Dictionary<Vector3Int, Vector3Int> _pathFlowField = new Dictionary<Vector3Int, Vector3Int>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;

        if (!cam) cam = Camera.main;

        var fieldTable = GameManager.Table.GetTable<FieldTable>();
        var fieldData = fieldTable.Get(GameManager.Data.StageData.Diff, GameManager.Data.StageData.Stage);

        var spawnTable = GameManager.Table.GetTable<FieldSpawnTable>();
        var spawnEntries = spawnTable.Get(GameManager.Data.StageData.Diff, GameManager.Data.StageData.Stage);

        _spawnLocations.Clear();
        _goalLocations.Clear();

        if (spawnEntries != null)
        {
            foreach (var entry in spawnEntries)
            {
                if (entry.TYPE == "Spawn")
                    _spawnLocations.Add(entry.SPAWN_ID, new Vector3Int(entry.X, entry.Y, 0));
                else if (entry.TYPE == "Goal")
                    _goalLocations.Add(new Vector3Int(entry.X, entry.Y, 0));
            }
        }
        else
        {
            Debug.LogError("FieldSpawnTable 로드 실패!");
        }

        if (fieldData == null || fieldData.Length == 0)
        {
            Debug.LogError("fieldData가 비어있습니다! 맵 로드를 중단합니다.");
            return;
        }

        int mapHeight = fieldData.GetLength(0);
        int mapWidth = fieldData.GetLength(1);

        var bounds = targetTilemap.cellBounds;
        int minX = bounds.xMin;
        int minY = bounds.yMin;

        _spawned.Clear();

        for (int y = 0; y < mapHeight; y++)
        {
            Debug.Log($"Map Info - Height: {mapHeight}, minY: {minY}, Total Top: {mapHeight + minY}");
            for (int x = 0; x < mapWidth; x++)
            {
                int tileTypeInt = fieldData[y, x];
                Vector2Int index = new Vector2Int(x, y);
                Vector2Int tile;
                if(mapHeight % 2 == 0)
                    tile = new Vector2Int((x + minX), ((mapHeight -1 - y) + minY));
                else
                    tile = new Vector2Int((x + minX), ((mapHeight - y) + minY));
                _spawned.AddSpwned(index, tile, new CellData() { _pos_index = index, _pos_tile = tile, _tileType = (ETileType)tileTypeInt });
            }
        }

        CalculateFlowField();


        foreach (var entry in _spawned.GetAllIndexCells())
        {
            Vector3Int cellPos = new Vector3Int(entry.Value._pos_tile.x, entry.Value._pos_tile.y, 0);
            SpawnAtCell(entry.Value._tileType, entry.Value._pos_index, cellPos, new Vector2(0.5f, 0.5f));
        }
    }

    public Camera GetCamera()
    {
        return cam ? cam : Camera.main;
    }

    public bool IsPlaceable(Vector3Int absoluteCell, EUnitMask unitType)
    {
        var cell = new Vector2Int(absoluteCell.x, absoluteCell.y);
        if (!_spawned.TryGetByIndex(cell, out CellData cellData)) return false;

        var tileTypeData = GameManager.Table.GetTable<TileTypeTable>().Get(cellData._tileType);

        if (tileTypeData == null)
            return false;

        return (tileTypeData.ALLOWED_MASK & (int)unitType) > 0;
    }

    public Dictionary<Vector2Int, CellData> GetAllClickableTiles()
    {
        return _spawned.GetAllIndexCells();
    }

    private void CalculateFlowField()
    {
        _pathFlowField.Clear();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        foreach (var goal in _goalLocations)
        {
            queue.Enqueue(goal);
            visited.Add(goal);
            _pathFlowField[goal] = goal;
        }

        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            foreach (var dir in directions)
            {
                Vector3Int neighbor = current + dir;
                Vector2Int neighbor2D = new Vector2Int(neighbor.x, neighbor.y);

                if(_spawned.TryGetByIndex(neighbor2D, out CellData cell) && !visited.Contains(neighbor) && IsWalkable(cell._tileType))
                {
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);

                    _pathFlowField[neighbor] = current;
                }
            }
        }
    }

    private bool IsWalkable(ETileType type)
    {
        // 몬스터는 Grass(1), Spawn(5), Goal(6) 타일로만 이동
        switch (type)
        {
            case ETileType.Grass:
            case ETileType.Spawn:
            case ETileType.Goal:
                return true;

            case ETileType.Road:
            case ETileType.Rock:
            case ETileType.Water:
            case ETileType.None:
            default:
                return false;
        }
    }

    public Vector3Int GetNextCell(Vector3Int currentCell)
    {
        if (_pathFlowField.TryGetValue(currentCell, out var nextCell))
        {
            return nextCell;
        }

        return currentCell;
    }

    public Vector3Int GetSpawnCell(int spawnId)
    {
        if (_spawnLocations.TryGetValue(spawnId, out var cell))
        {
            return cell;
        }
        Debug.LogError($"유효하지 않은 SPAWN_ID: {spawnId}");
        return _spawnLocations.Values.First();
    }

    public Vector3 GetWorldPosFromIndexCell(Vector3Int cell)
    {
        if(_spawned.TryGetByIndex(new Vector2Int(cell.x, cell.y), out var value))
        {
            return GetWorldPosInCell(new Vector3Int(value._pos_tile.x, value._pos_tile.y, 0), new Vector2(0.5f, 0.5f));
        }
        else
        {
            Debug.Log($"GetWorldPosFromCell 실패: {cell.x}, {cell.y}");
        }

            return Vector3.zero;
    }

    public Vector3 GetFirstSpawnWorldPos()
    {
        Vector3Int spawnCell = GetSpawnCell(0);
        return GetWorldPosFromIndexCell(spawnCell);
    }

    public Vector3Int GetIndexCellFromWorldPos(Vector3 worldPos)
    {
        var tileMapCell = targetTilemap.WorldToCell(worldPos);
        if (_spawned.TryGetByTileMap(new Vector2Int(tileMapCell.x, tileMapCell.y), out CellData cell))
            return new Vector3Int(cell._pos_index.x, cell._pos_index.y, 0);

        return new Vector3Int(-1, -1, 0);
    }

    private Vector3 GetWorldPosInCell(Vector3Int cell, Vector2 offset01)
    {
        Vector3 origin = targetTilemap.CellToWorld(cell);
        Vector3 size = targetTilemap.layoutGrid.cellSize;
        Vector3 p = origin + new Vector3(offset01.x * size.x, offset01.y * size.y, 0f);
        p.z = 0f;
        return p;
    }

    public ClickableTileItem SpawnAtCell(ETileType tileType, Vector2Int index, Vector3Int cell, Vector2 offset)
    {
        if (!targetTilemap || !targetTilemap.HasTile(cell))
        {
            var indexTile = targetTilemap.GetTile(new Vector3Int(index.x, index.y, 1));
            var cellTile = targetTilemap.GetTile(cell);
            Debug.Log($"타일 설치 못했음! ({cell.x}, {cell.y}) : {(int)tileType}, ({index.x}, {index.y}), {indexTile?.name} {cellTile?.name}");
            return null;
        }

        var cells = _spawned.GetAllIndexCells();
        if(cells.TryGetValue(index, out CellData cellData))
        {
            if (cellData._tileItem != null)
            {
                Debug.Log($"이미 스폰됨: {index}");
                return cellData._tileItem;
            }
        }
        else
        {
            Debug.LogError($"타일 데이터가 없음: {index}");
            return null;
        }

        Vector3 pos = GetWorldPosInCell(cell, offset);
        if (GameManager.Addressables.TryGet<UnityEngine.Object>(Define.SLOT_TILE_PATH, out var asset))
        {
            var obj = GameObject.Instantiate(asset);
            var item = obj.GetComponent<ClickableTileItem>();
            if (item == null) return null;

            item.gameObject.name = $"TileItem_{cell.x}_{cell.y}";
            item.gameObject.transform.position = pos;

            var sr = item.GetComponent<SpriteRenderer>();
            var col = item.GetComponent<BoxCollider2D>();

            if (fitToTile && sr.sprite != null)
            {
                var cellSize = targetTilemap.layoutGrid.cellSize;
                var target = new Vector2(Mathf.Max(0.0001f, cellSize.x - padding.x),
                                         Mathf.Max(0.0001f, cellSize.y - padding.y));
                var spriteSize = sr.sprite.bounds.size;
                var scale = new Vector3(target.x / Mathf.Max(0.0001f, spriteSize.x),
                                        target.y / Mathf.Max(0.0001f, spriteSize.y),
                                        1f);
                item.gameObject.transform.localScale = scale;
                col.size = spriteSize;
            }

            cellData._tileItem = item;

            item._type = tileType;
            return item;
        }

        return null;
    }
}