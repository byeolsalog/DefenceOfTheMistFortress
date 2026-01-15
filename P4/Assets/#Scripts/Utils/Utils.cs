using UnityEngine;
using UnityEngine.UI;
using Unity.VisualScripting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public static class Utils
{
    public static string GetLanguage(string str, params object[] args)
    {
        var table = GameManager.Table.GetTable<LanguageTable>();
        if (table == null)
            return str;

        return table.Get(str, args);
    }

    public static List<string> ToStringList(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new List<string>();

        return input.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
    }

    public static int[,] LoadMapFromCSV(string csvText)
    {
        var lines = csvText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        int height = lines.Length;
        if (height == 0) return new int[0, 0];

        int width = lines[0].Split(',').Length;

        string[,] map = new string[height, width];
        for (int y = 0; y < height; y++)
        {
            var cols = lines[y].Split(",").Select(c => c.Trim()).ToArray();
            int loopWidth = Math.Min(width, cols.Length);
            for (int x = 0; x < loopWidth; x++)
            {
                map[y, x] = cols[x];
            }
        }

        int h = map.GetLength(0);
        int w = map.GetLength(1);
        int[,] result = new int[h, w];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                if (int.TryParse(map[y, x], out int val))
                    result[y, x] = val;
                else
                    result[y, x] = -1;
            }
        }

        return result;
    }

    public static (int width, int height) GetAttackRangeSize(string attackRange)
    {
        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;

        string[] pairs = attackRange.Split(';', StringSplitOptions.RemoveEmptyEntries);

        foreach (var pair in pairs)
        {
            string[] xy = pair.Split(',');

            int x = int.Parse(xy[0]);
            int y = int.Parse(xy[1]);

            minX = Math.Min(minX, x);
            maxX = Math.Max(maxX, x);
            minY = Math.Min(minY, y);
            maxY = Math.Max(maxY, y);
        }

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        return (width, height);
    }

    public static T GetOrAddComponent<T>(GameObject obj) where T : Component
    {
        var comp = obj.GetComponent<T>();
        if (comp == null)
        {
            comp = obj.AddComponent<T>();
        }
        return comp;
    }
}

public static class ExtensionUtils
{
    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
    {
        return Utils.GetOrAddComponent<T>(obj);        
    } 

    public static string GetLanguage(this string str, params object[] args)
    {
        if (args == null || args.Length == 0)
            return Utils.GetLanguage(str);
        else
            return string.Format(Utils.GetLanguage(str), args);
    }

    public static T Get<T>(this UnityEngine.Object obj)
    {
        var comp = obj.GetComponent<T>();
        return comp;
    }
}

public static class Debug
{
    public static void Log(string msg)
    {
        UnityEngine.Debug.Log(msg);
    }

    public static void LogWarning(string msg)
    {
        UnityEngine.Debug.LogWarning(msg);
    }

    public static void LogError(string msg)
    {
        UnityEngine.Debug.LogError(msg);
    }
}
