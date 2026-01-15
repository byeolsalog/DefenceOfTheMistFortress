using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class FieldSpawnEntry
{
    public int SPAWN_ID;
    public string TYPE;
    public int X;
    public int Y;
}

public class FieldSpawnTable : IFieldTable
{
    private readonly Dictionary<(int diff, int stage), List<FieldSpawnEntry>> _entries = new();

    public string GetPath(int diff, int stage)
    {
        return $"FieldTable/{diff}-{stage}Spawn.json";
    }

    public bool IsLoadFieldData(int diff, int stage)
    {
        return _entries.ContainsKey((diff, stage));
    }
    

    public void TableLoad(string text, int diff = -1, int stage = -1)
    {
        if (diff <= 0 || stage <= 0)
            throw new System.ArgumentException("diff와 stage는 1 이상의 값이어야 합니다.");        

        if (_entries.ContainsKey((diff, stage)))
        {
            Debug.LogWarning($"이미 로드된 Spawn 데이터 {diff}-{stage}");
            return;
        }

        List<FieldSpawnEntry> spawnDatas;
        try
        {
            spawnDatas = JsonConvert.DeserializeObject<List<FieldSpawnEntry>>(text);
        }
        catch(System.Exception e)
        {
            Debug.LogError($"Json 파싱 실패: {e.Message}");
            return;
        }

        if(spawnDatas == null || spawnDatas.Count == 0)
        {
            Debug.LogWarning("스폰 데이터가 없거나 빈 데이터입니다.");
            return;
        }

        _entries[(diff, stage)] = spawnDatas;
    }

    public List<FieldSpawnEntry> Get(int diff, int stage)
    {
        if (!_entries.TryGetValue((diff, stage), out var data))
            return null;

        return data;
    }
}
