using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class MonsterEntry
{
    public int MONSTER_ID;
    public string MONSTER_NAME;
    public string PREFAB_PATH;
    public float HP;
    public float SPEED;
    public float REWARD;
    public float ATTACK_SPEED;
    public float ATTACK;
    public float DEFENCE;
    public string SPRITE_PATH;
}

public class MonsterTable : ITable
{
    private readonly Dictionary<int, MonsterEntry> _entries = new();

    public void TableLoad(string text, int diff = -1, int stage = -1)
    {
        List<MonsterEntry> list;
        try
        {
            list = JsonConvert.DeserializeObject<List<MonsterEntry>>(text);
        }        
        catch (System.Exception e)
        {
            Debug.LogWarning($"Json 파싱 실패 {e.Message}");
            return;
        }

        if(list == null || list.Count == 0)
        {
            Debug.LogWarning("MonsterTable 데이터가 없음");
            return;
        }

        _entries.Clear();
        foreach (var e in list)        
            _entries[e.MONSTER_ID] = e;        
    }

    public MonsterEntry Get(int id)
    {
        if (!_entries.TryGetValue(id, out MonsterEntry entry))
            return null;

        return entry;
    }
}
