using UnityEngine;
using System.IO;
using Newtonsoft.Json;
public class DataManager
{
    private OptionData _optionData;
    public OptionData OptionData
    {
        get
        {
            if (_optionData == null)
            {
                _optionData =LoadData<OptionData>(EFileDataType.Option);

                if(_optionData == null)
                    _optionData = new OptionData();
            }

            return _optionData;
        }
    }

    private StageData _stageData;
    public StageData StageData
    {
        get
        {
            if(_stageData == null)
            {
                _stageData = LoadData<StageData>(EFileDataType.Stage);

                if (_stageData == null)
                    _stageData = new();
            }            

            return _stageData;
        }
    }

    private string GetFilePath(EFileDataType type)
    {
        return Path.Combine(Application.persistentDataPath, type.ToString().ToLower() + ".json");
    }

    public void SaveData(EFileDataType type)
    {
        switch (type)
        {
            case EFileDataType.Option:
                SaveData(OptionData, type);
                break;

            case EFileDataType.Stage:

                if ((StageData.Diff > StageData.maxDiff) || (StageData.maxDiff == StageData.Diff && StageData.Stage >= StageData.maxStage))
                {
                    SaveData(StageData, type);
                }
                break;

            default:
                Debug.LogError($"[DataManager] 알 수 없는 데이터 타입입니다: {type}");
                break;
        }
    }

    private void SaveData<T>(T data, EFileDataType type)
    {
        string path = GetFilePath(type);
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(path, json);
        Debug.Log($"[DataManager] {type} 데이터가 저장되었습니다: {path}, 저장된 데이터: {json}");
    }

    private T LoadData<T>(EFileDataType type) where T : new()
    {
        string path = GetFilePath(type);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var loadData = JsonConvert.DeserializeObject<T>(json);
            if(loadData == null)
                return new T();

            return loadData;
        }
        else
        {
            Debug.LogWarning($"[DataManager] {type} 데이터 파일이 존재하지 않습니다: {path}");
            return new T();
        }
    }
}

[System.Serializable]
public class OptionData
{
    public float bgmVolume = 1.0f;
    public float sfxVolume = 1.0f;
}

// 파일로 저장할 때는 최대치만 저장.
[System.Serializable]
public class StageData
{
    private int _diff = 1;
    public int Diff => _diff;
    private int _stage = 0;
    public int Stage => _stage;

    public int maxDiff = 1;
    public int maxStage = 0;

    public void SetStageData(int diff, int stage)
    {
        _diff = diff;
        _stage = stage;
    }

    public void SetMaxData()
    {
        maxDiff = _diff;
        maxStage = _stage;
    }
}
