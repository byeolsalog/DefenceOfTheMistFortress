using Newtonsoft.Json;
using System.Collections.Generic;

[System.Serializable]
public class WaveEntry
{
    public int DIFFICULTY;
    public int STAGE;
    public int WAVE;
    public float START_TIME;
}

public class WaveTable : ITable
{
    private readonly Dictionary<(int diff, int stage), Dictionary<int, WaveEntry>> _entries = new();

    public void TableLoad(string text, int diff = -1, int stage = -1)
    {
        List<WaveEntry> list;
        try
        {
            list = JsonConvert.DeserializeObject<List<WaveEntry>>(text);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Json 파싱 실패: {e.Message}");
            return;
        }
        
        if(list == null || list.Count == 0)
        {
            Debug.LogWarning("WaveTable: 로드된 데이터가 없습니다.");
            return;
        }

        _entries.Clear();
        foreach (var e in list)
        {
            if (!_entries.ContainsKey((e.DIFFICULTY, e.STAGE)))
                _entries.Add((e.DIFFICULTY, e.STAGE), new Dictionary<int, WaveEntry>());

            if (!_entries[(e.DIFFICULTY, e.STAGE)].ContainsKey(e.WAVE))
                _entries[(e.DIFFICULTY, e.STAGE)].Add(e.WAVE, e);
        }
    }

    public Dictionary<int, WaveEntry> Get(int diff, int stage)
    {
        if (!_entries.TryGetValue((diff, stage), out var data))
            return null;

        return data;
    }
}
