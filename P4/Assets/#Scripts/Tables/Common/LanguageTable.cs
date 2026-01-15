using Newtonsoft.Json;
using System.Collections.Generic;

[System.Serializable]
public class LanguageEntry
{
    public int ID;
    public string IDENTIFIER;
    public string DISPLAY;
}

public class LanguageTable : ITable
{
    private readonly Dictionary<string, string> _entries = new();

    public void TableLoad(string json, int diff = -1, int stage = -1)
    {
        List<LanguageEntry> list;
        try
        {
            list = JsonConvert.DeserializeObject<List<LanguageEntry>>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Json 파싱 실패 {e.Message}");
            return;
        }
        _entries.Clear();

        if(list == null || list.Count == 0)
        {
            Debug.LogWarning("LanguageTable 데이터가 없음");
            return;
        }

        foreach (var e in list)
            if(!_entries.ContainsKey(e.IDENTIFIER))
                _entries.Add(e.IDENTIFIER, e.DISPLAY);
    }
    
    public string Get(string identifier, params object[] args)
    {
        if (_entries.TryGetValue(identifier, out var value))
            return args != null && args.Length > 0 ? string.Format(value, args) : value;

        return identifier;
    }
}
