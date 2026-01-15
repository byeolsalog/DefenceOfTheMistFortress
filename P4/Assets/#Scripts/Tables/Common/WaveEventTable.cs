using System.Collections.Generic;
using Newtonsoft.Json;

[System.Serializable]
public class WaveEventEntry
{
    public int DIFFICULTY;
    public int STAGE;
    public int WAVE;
    public int EVENT;
    public float TIME;
    public int MONSTER_ID;
    public int COUNT;
    public float INTERVAL;
    public int SPAWN_ID;
    public bool IS_BOSS;
}

public class WaveEventTable : ITable
{
    private readonly Dictionary<(int diff, int stage), List<WaveEventEntry>> _entries = new Dictionary<(int diff, int stage), List<WaveEventEntry>>();

    public void TableLoad(string text, int diff = -1, int stage = -1)
    {
        List<WaveEventEntry> list;
        try
        {
            list = JsonConvert.DeserializeObject<List<WaveEventEntry>>(text);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"JSON 파싱 오류: {e.Message}");
            return;
        }
        
        if(list == null || list.Count == 0)
        {
            Debug.LogWarning("WaveEventTable: 로드된 데이터가 없습니다.");
            return;
        }

        _entries.Clear();
        foreach (var e in list)
        {
            if (!_entries.ContainsKey((e.DIFFICULTY, e.STAGE)))
                _entries.Add((e.DIFFICULTY, e.STAGE), new List<WaveEventEntry>());

            _entries[(e.DIFFICULTY, e.STAGE)].Add(e);
        }
    }

    public List<WaveEventEntry> Get(int diff, int stage)
    {
        if (!_entries.TryGetValue((diff, stage), out var e))
            return null;

        return e;
    }
}
