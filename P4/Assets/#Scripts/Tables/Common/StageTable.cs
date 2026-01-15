using Newtonsoft.Json;
using System.Collections.Generic;

[System.Serializable]
public class StageEntry
{
    public int DIFFICULTY;
    public int STAGE;
    public string NAME;
    public int GRID_W;
    public int GRID_H;
    public int LIFE_COUNT;
    public string MAP;
    public List<int> MONSTERS;
    public int PLACEMENT_CAPACITY;
}

public class StageTable : ITable
{
    private readonly Dictionary<int, Dictionary<int, StageEntry>> _entries = new();

    public void TableLoad(string json, int diff = -1, int stage = -1)
    {
        List<StageEntry> list;
        try
        {
            list = JsonConvert.DeserializeObject<List<StageEntry>>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Json 파싱 실패: {e.Message}");
            return;
        }

        if(list == null || list.Count == 0)
        {
            Debug.LogError("StageTable 데이터 로드 실패");
            return;
        }

        _entries.Clear();
        foreach (var e in list)
        {
            if (!_entries.ContainsKey(e.DIFFICULTY))            
                _entries.Add(e.DIFFICULTY, new Dictionary<int, StageEntry>());
            
            if(!_entries[e.DIFFICULTY].ContainsKey(e.STAGE))
                _entries[e.DIFFICULTY].Add(e.STAGE, e);
        }
    }

    public StageEntry Get(int difficulty, int stage)
    {
        if (_entries.TryGetValue(difficulty, out var stageDict))
        {
            if (stageDict.TryGetValue(stage, out var entry))
                return entry;
        }
        return null;
    }

    public Dictionary<int, Dictionary<int, StageEntry>> Get()
    {
        return _entries;
    }
}
