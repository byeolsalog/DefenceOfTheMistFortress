using System;
using System.Collections.Generic;

public class FieldTable : IFieldTable
{
    private readonly Dictionary<(int diff, int stage), int[,]> _mapDic = new();

    public void TableLoad(string text, int diff = -1, int stage = -1)
    {
        if (diff <= 0 || stage <= 0)
            throw new ArgumentException("diff와 stage는 1 이상의 값이어야 합니다.");

        if (string.IsNullOrWhiteSpace(text))
        {
            Debug.LogError($"CSV 텍스트가 비어 있습니다. {diff}-{stage}");
            return;
        }

        var mapData = Utils.LoadMapFromCSV(text);
        if (mapData == null)
        {
            Debug.LogError($"CSV 파싱 실패: {diff}-{stage}");
            return;
        }

        _mapDic[(diff, stage)] = mapData;
    }

    public bool IsLoadFieldData(int diff, int stage)
    {
        return _mapDic.ContainsKey((diff, stage));
    }

    public int[,] Get(int diff, int stage)
    {
        if (_mapDic.TryGetValue((diff, stage), out var map))
            return map;

        return new int[0,0];
    }

    public string GetPath(int diff, int stage)
    {
        return $"FieldTable/{diff}-{stage}.csv";
    }
}
