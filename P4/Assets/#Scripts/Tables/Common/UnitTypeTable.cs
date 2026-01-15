using Newtonsoft.Json;
using System.Collections.Generic;

[System.Serializable]
public class UnitTypeEntry
{
    public int UNIT_ID;
    public string CODE;
    public string DISPLAY;
}

public class UnitTypeTable : ITable
{
    private readonly Dictionary<EUnitMask, UnitTypeEntry> _entries = new Dictionary<EUnitMask, UnitTypeEntry>();

    public void TableLoad(string json, int diff = -1, int stage = -1)
    {
        List<UnitTypeEntry> list;
        try
        {
            list = JsonConvert.DeserializeObject<List<UnitTypeEntry>>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Json 파싱 실패: {e.Message}");
            return;
        }

        if(list == null || list.Count == 0)
        {
            Debug.LogWarning("UnitTypeTable 데이터가 없음");
            return;
        }

        _entries.Clear();
        foreach (var e in list)
        {
            if (!_entries.ContainsKey((EUnitMask)e.UNIT_ID))
                _entries.Add((EUnitMask)e.UNIT_ID, e);                
        }
    }    
}
