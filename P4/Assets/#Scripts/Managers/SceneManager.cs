using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
public class SceneManager
{
    private readonly Dictionary<EGameScene, string> sceneNames = new Dictionary<EGameScene, string>
    {
        { EGameScene.Login, "Login" },
        { EGameScene.Robby, "Lobby" },
        { EGameScene.Battle, "Battle" },
    };
    private EGameScene _currentScene = EGameScene.Login;
    public EGameScene CurrentScene => _currentScene;
    private AsyncOperationHandle<SceneInstance> _sceneHandle;

    private const string LoadingUIPath = "LoadingUI";

    public async void LoadScene(EGameScene scene)
    {
        if (sceneNames.TryGetValue(scene, out string sceneName))
        {
            Debug.Log($"[SceneManager] {scene} 씬으로 이동 중...");
            LoadingUI loadingUI = ShowLoadingUI();

            var op = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            while (!op.isDone)
            {
                float progress = Mathf.Clamp01(op.progress / 0.9f);

                if (loadingUI != null)
                    loadingUI.SetProgress(progress);

                if (op.progress >= 0.9f)
                {
                    op.allowSceneActivation = true;
                }

                await Task.Yield();
            }

            _currentScene = scene;
            if (loadingUI != null)
                GameManager.UI.CloseUI(loadingUI);
        }
        else
        {
            Debug.LogError($"[SceneManager] 씬 이름이 등록되지 않았습니다: {scene}");
        }
    }

    public async void LoadBattleScene(int diff, int stage)
    {
        LoadingUI loadingUI = ShowLoadingUI();
        if (loadingUI != null) loadingUI.SetProgress(0f);

        string scene = Define.SceneName(diff, stage);
        GameManager.Data.StageData.SetStageData(diff, stage);

        var titleTask = GameManager.Addressables.PreloadByLabelAsync<UnityEngine.Object>(EAddressablesLabel.Tile.ToString());
        var battleTask = GameManager.Addressables.PreloadByLabelAsync<UnityEngine.Object>(EAddressablesLabel.Prefab_Battle.ToString());
        var sfxTask = GameManager.Addressables.PreloadByLabelAsync<AudioClip>(EAddressablesLabel.SFX.ToString());
        var fieldTask = GameManager.Table.LoadFieldTableAsync<FieldTable>(diff, stage);
        var fieldSpawnTask = GameManager.Table.LoadFieldTableAsync<FieldSpawnTable>(diff, stage);

        await Task.WhenAll(titleTask, battleTask, sfxTask, fieldTask, fieldSpawnTask);
        var sfxClips = await sfxTask;
        GameManager.Audio.LoadSFXs(sfxClips);
        if (loadingUI != null) loadingUI.SetProgress(0.3f);

        var battleSceneOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneNames[EGameScene.Battle], LoadSceneMode.Single);
        battleSceneOperation.allowSceneActivation = false;

        while (!battleSceneOperation.isDone)
        {
            float progress = 0.3f + (battleSceneOperation.progress / 0.9f) * 0.3f;
            if (loadingUI != null) loadingUI.SetProgress(progress);

            if (battleSceneOperation.progress >= 0.9f)
                battleSceneOperation.allowSceneActivation = true;

            await Task.Yield();
        }

        _sceneHandle = Addressables.LoadSceneAsync(scene, LoadSceneMode.Additive);

        while (!_sceneHandle.IsDone)
        {
            float progress = 0.6f + _sceneHandle.PercentComplete * 0.4f;
            if (loadingUI != null) loadingUI.SetProgress(progress);

            await Task.Yield();
        }

        if (_sceneHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log($"씬 로드 완료: {scene}");
            _currentScene = EGameScene.Battle;
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(_sceneHandle.Result.Scene);

            if (loadingUI != null)
            {
                loadingUI.SetProgress(1.0f);
                await Task.Delay(500);
                GameManager.UI.CloseUI(loadingUI);
            }
        }
        else
        {
            Debug.LogError($"어드레서블 씬 로드 실패: {scene}");
            if (loadingUI != null) GameManager.UI.CloseUI(loadingUI);
        }
    }


    private LoadingUI ShowLoadingUI()
    {
        GameObject go = Resources.Load<GameObject>(LoadingUIPath);
        if (go == null)
        {
            Debug.LogError($"로딩 UI 프리팹을 찾을 수 없습니다. 경로를 확인하세요: {LoadingUIPath}");
            return null;
        }

        UI_Base uiBase = go.GetComponent<UI_Base>();
        if (uiBase == null)
        {
            Debug.LogError($"로딩 UI 프리팹에 UI_Base(또는 LoadingUI) 컴포넌트가 없습니다.");
            return null;
        }

        return GameManager.UI.ShowUI(uiBase) as LoadingUI;
    }
}