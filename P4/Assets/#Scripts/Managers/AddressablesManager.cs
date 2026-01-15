using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class AddressCacheData
{
    public object _asset;
    public System.Type _assetType;
}

public class AddressablesManager
{
    private bool _isInitialized = false;
    public List<string> _labels = new List<string>() { "Scene", "Prefab_Common", "Prefab_Battle", "Table", "Sprite", "Tile", "FieldTable", "BGM", "SFX" };
    public Dictionary<string, AddressCacheData> _cacheDatas = new Dictionary<string, AddressCacheData>();
    public event Action<long> OnDownloadStarted;
    public event Action<float, long, long> OnProgressUpdated;
    public event Action<bool> OnDownloadCompleted;

    private readonly Dictionary<string, HashSet<string>> _labelToAddresses = new();

    private readonly Dictionary<string, SemaphoreSlim> _locks = new Dictionary<string, SemaphoreSlim>();
    private SemaphoreSlim GetLock(string address)
    {
        if( !_locks.TryGetValue(address, out var sem))
        {
            sem = new SemaphoreSlim(1, 1);
            _locks[address] = sem;
        }

        return sem;
    }

    public async Task InitAddressables()
    {
        if (_isInitialized)
        {
            Debug.Log("Addressables 이미 초기화됨");
            return;
        }

        var handle = Addressables.InitializeAsync();
        handle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("Addressables 초기화 성공");
                _isInitialized = true;
            }
            else
            {
                Debug.LogError($"Addressables 초기화 실패: {op.OperationException}");
            }
        };

        await handle.Task;        
    }

    public async Task<bool> DownloadAllDependenciesAsync()
    {
        bool success = false;
        try
        {
            long totalDownloadSize = await GetTotalDownloadSize(_labels);

            if (totalDownloadSize <= 0)
            {
                Debug.Log("다운로드할 에셋이 없습니다. (이미 캐시됨)");
                return true;
            }

            OnDownloadStarted?.Invoke(totalDownloadSize);

            var downloadHandle = Addressables.DownloadDependenciesAsync(_labels, Addressables.MergeMode.Union, true);
            downloadHandle.Completed += (op) =>
            {
                success = op.Status == AsyncOperationStatus.Succeeded;
                if (success) Debug.Log("모든 에셋 다운로드 성공");
                else Debug.LogError($"에셋 다운로드 실패: {op.OperationException}");

                OnDownloadCompleted?.Invoke(success);
            };

            while (!downloadHandle.IsDone)
            {
                float percent = downloadHandle.PercentComplete;
                long downloadedBytes = (long)(totalDownloadSize * percent);
                OnProgressUpdated?.Invoke(percent, downloadedBytes, totalDownloadSize);
                await Task.Yield();
            }

            Debug.Log($"결과 : {success}");
            return success;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            return false;
        }
    }

    private async Task<long> GetTotalDownloadSize(List<string> labelsToCheck)
    {
        var sizeHandle = Addressables.GetDownloadSizeAsync(labelsToCheck);
        long downloadSize = -1;

        sizeHandle.Completed += (op) =>
        {
            if (op.Status == AsyncOperationStatus.Succeeded)
            {
                downloadSize = op.Result;
            }
            else
            {
                Debug.LogError($"다운로드 크기 확인 실패: {op.OperationException}");
            }
            try
            {
                Addressables.Release(op);
            }
            catch (Exception e)
            {
                Debug.LogError($"어드레서블 release 실패 : {e.Message}");
            }            
        };

        await sizeHandle.Task;
        return downloadSize;
    }

    public async Task<T> LoadAssetAsync<T>(string address)
    {
        var sem = GetLock(address);
        await sem.WaitAsync();
        try
        {
            if (_cacheDatas.TryGetValue(address, out var data))
            {
                if (data._assetType != typeof(T))
                    throw new InvalidCastException($"{address}는 {data._assetType.Name}으로 캐시됨. 요청 타입: {typeof(T).Name}");

                _cacheDatas[address] = data;
                return (T)data._asset;
            }

            var handle = Addressables.LoadAssetAsync<T>(address);
            var asset = await handle.Task;

            _cacheDatas[address] = new() { _asset = asset!, _assetType = typeof(T)};
            return asset!;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Addressable] {address}/{ex.Message}");
            return default;
        }
        finally
        {
            sem.Release();
        }
    }

    public async Task<IReadOnlyList<T>> PreloadByLabelAsync<T>(string label, Action<int, int> onProgress = null)
    {
        var locHandle = Addressables.LoadResourceLocationsAsync(label, typeof(T));
        IList<IResourceLocation> locations = null;

        try
        {
            locations = await locHandle.Task;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Addressables] label: {label} / {ex.Message}");
            Addressables.Release(locHandle);
            return Array.Empty<T>();
        }

        if (locations == null || locations.Count == 0)
        {
            Addressables.Release(locHandle);
            return Array.Empty<T>();
        }

        var uniqueAddresses = new HashSet<string>();
        foreach (var loc in locations)
            uniqueAddresses.Add(loc.PrimaryKey);

        var results = new List<T>(uniqueAddresses.Count);
        int total = uniqueAddresses.Count;
        int idx = 0;

        foreach (var addr in uniqueAddresses)
        {
            try
            {
                var asset = await LoadAssetAsync<T>(addr);
                results.Add(asset);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Addressables] 실패: {addr} / {ex.Message}");
            }

            idx++;
            onProgress?.Invoke(idx, total);
        }

        if (!_labelToAddresses.TryGetValue(label, out var set))
        {
            set = new HashSet<string>();
            _labelToAddresses[label] = set;
        }

        foreach (var a in uniqueAddresses)
            set.Add(a);

        Addressables.Release(locHandle);

        Debug.Log($"[Addressables] {typeof(T).Name} load complete");
        return results;
    }

    public bool TryGet<T>(string address, out T asset)
    {
        if(_cacheDatas.TryGetValue(address,out var data) && data._assetType == typeof(T))
        {
            asset = (T)data._asset;
            return true;
        }

        asset = default;
        return false;
    }

    public void ClearCache()
    {
        _cacheDatas.Clear();
        Addressables.ClearDependencyCacheAsync(_labels, true);
    }
}