using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface ITable
{
    void TableLoad(string text, int diff = -1, int stage = -1);
}

public interface IFieldTable : ITable
{
    public bool IsLoadFieldData(int diff, int stage);
    public string GetPath(int diff, int stage);
}


public class TableManager
{
    private readonly Dictionary<Type, ITable> _tableDatas = new();
    private bool _isLoaded = false;
    private readonly TableRegistrySO _registry;

    public TableManager(TableRegistrySO registry)
    {
        _registry = registry;
        _registry.Init();
    }

    public async Task<bool> LoadAllTablesAsync()
    {
        if(_isLoaded) return true;

        try
        {
            var textAssets = await GameManager.Addressables.PreloadByLabelAsync<TextAsset>(EAddressablesLabel.Table.ToString());
            foreach (var asset in textAssets)
            {
                if (asset == null) continue;
                
                ITable table = _registry.CreateTableInstance(asset.name);
                if (table == null) continue;

                table.TableLoad(asset.text);
                _tableDatas[table.GetType()] = table;

                Debug.Log($"[TableManager] {asset.name} 로드 완료");
            }
            _isLoaded = true;
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[TableManager] 로드 중 오류 : {e.Message}");
            return false;
        }
    }

    public async Task<bool> LoadFieldTableAsync<T>(int diff, int stage) where T : class, IFieldTable, new()
    {
        try
        {
            if(_tableDatas.TryGetValue(typeof(T), out var existing))
            {
                var fieldTable = existing as T;
                if(fieldTable == null)
                {
                    Debug.LogError($"[TableManager] 캐시에 저장된 테이블 타입이 일치하지 않습니다. input: {typeof(T).Name}, output: {existing?.GetType().Name}");
                    return false;
                }

                if (fieldTable.IsLoadFieldData(diff, stage))
                    return true;

                var path = fieldTable.GetPath(diff, stage);
                var text = await GameManager.Addressables.LoadAssetAsync<TextAsset>(path);
                if(text == null || string.IsNullOrEmpty(text.text))
                {
                    Debug.LogError($"[TableManager] Field Table 에셋이 비어있습니다. type: {typeof(T).Name}, path: {path}");
                    return false;
                }

                fieldTable.TableLoad(text.text, diff, stage);     
            }
            else
            {
                var fieldTable = new T();
                var path = fieldTable.GetPath(diff, stage);
                var text = await GameManager.Addressables.LoadAssetAsync<TextAsset>(path);
                if(text == null || string.IsNullOrEmpty(text.text))
                {
                    Debug.LogError($"[TableManager] Field Table 에셋이 비어있습니다. type: {typeof(T).Name}, path: {path}");
                    return false;
                }

                fieldTable.TableLoad(text.text, diff, stage);
                _tableDatas[typeof(T)] = fieldTable;
            }

            return true;
        }
        catch(Exception ex)
        {
            Debug.LogError($"[TableManager] Field Table 로드 중 오류: type: {typeof(T).Name}, diff: {diff}, stage: {stage}, message: {ex.Message}");
            return false;
        }
    }
    public T GetTable<T>() where T : class, ITable
    {
        if(_tableDatas.TryGetValue(typeof(T), out var table))
        {
            return table as T;
        }
        Debug.LogWarning($"[TableManager] 테이블을 찾을 수 없음: {typeof(T).Name}");
        return null;
    }
}