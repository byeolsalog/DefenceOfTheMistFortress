using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class TowerEntry
{
    public int TOWER_ID;
    public string NAME;
    public string PREFAB_PATH;
    public EUnitMask UNIT_TYPE;
    public float HP;
    public float ATTACK;
    public float DEFENCE;
    public float ATTACK_SPEED;
    public string ATTACK_RANGE;
    [JsonIgnore] public List<Vector3Int> ParsedAttackRange { get; private set; }
    public int BLOCK_COUNT;
    public int COST;
    public string SPRITE_PATH;
    public string ATTACK_SFX;

    public void ParseAttackRange()
    {
        ParsedAttackRange = new();
        if (ATTACK_RANGE == null) return;

        string[] ranges = ATTACK_RANGE.TrimEnd(';').Split(';');
        List<string> rangeList = new List<string>(ranges);

        foreach (var pattern in rangeList)
        {
            var parts = pattern.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
                ParsedAttackRange.Add(new Vector3Int(x, y, 0));
        }
    }
}

public class TowerTable : ITable
{
    private readonly Dictionary<int, TowerEntry> _entries = new Dictionary<int, TowerEntry>();

    public void TableLoad(string text, int diff = -1, int stage = -1)
    {
        List<TowerEntry> list;
        try
        {
            list = JsonConvert.DeserializeObject<List<TowerEntry>>(text);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON 파싱 오류: {e.Message}");
            return;
        }

        if(list == null || list.Count == 0)
        {
            Debug.LogError("TowerTable 데이터가 비어있습니다.");
            return;
        }

        _entries.Clear();
        foreach (var e in list)
        {
            if (!_entries.ContainsKey(e.TOWER_ID))
            {
                e.ParseAttackRange();
                _entries.Add(e.TOWER_ID, e);
            }
        }
    }

    public TowerEntry Get(int towerId)
    {
        _entries.TryGetValue(towerId, out var entry);
        return entry;
    }

    public Dictionary<int, TowerEntry> GetAllData()
    {
        return _entries;
    }
}