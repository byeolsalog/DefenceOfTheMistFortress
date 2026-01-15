using Newtonsoft.Json;
using System.Collections.Generic;

[System.Serializable]
public class TileTypeEntry
{
    public int ID;
    public ETileType tileType;
    public List<string> tags;
    public string TILE_TYPE;
    public string TAG;
    public int ALLOWED_MASK;
}

public class TileTypeTable : ITable
{
    private readonly Dictionary<ETileType, TileTypeEntry> _entries = new();
    public void TableLoad(string json, int diff = -1, int stage = -1)
    {
        List<TileTypeEntry> list;
        try
        {
            list = JsonConvert.DeserializeObject<List<TileTypeEntry>>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Json 파싱 실패: {e.Message}");
            return;
        }

        if(list == null || list.Count == 0)
        {
            Debug.LogWarning("TileTypeTable 데이터가 없음.");
            return;
        }

        _entries.Clear();
        foreach (var e in list)
        {
            if (System.Enum.TryParse<ETileType>(e.TILE_TYPE, out var type))
            {
                if(!_entries.ContainsKey(type))
                {                    
                    _entries.Add(type, new TileTypeEntry()
                    {
                        ID = e.ID,
                        tileType = type,
                        TILE_TYPE = e.TILE_TYPE,
                        ALLOWED_MASK = e.ALLOWED_MASK,
                        tags = Utils.ToStringList(e.TAG)
                    });
                }                    
            }
            else
            {
                Debug.LogWarning($"Invalid TILE_TYPE: {e.TILE_TYPE}");
            }
        }
    }

    public TileTypeEntry Get(ETileType type)
    {
        if (_entries.TryGetValue(type, out var res))
            return res;

        return null;
    }
}
