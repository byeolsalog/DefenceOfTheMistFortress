using UnityEditor;
using UnityEngine;

public class MapStage : MonoBehaviour
{
    private void Start()
    {
        GameManager.Audio.StopBGM();
        GameManager.Audio.PlayBGM(EBGM.LobbyBGM);
    }

    public void OnClickOption()
    {
        GameManager.Addressables.TryGet<UnityEngine.Object>("Prefabs_Common/PopupPrefabs/Option.prefab", out var prefab);
        var popup = prefab.Get<OptionUI>();
        GameManager.UI.ShowUI(popup);
    }

    public void OnClickQuitGame()
    {
        GameManager.Addressables.TryGet<UnityEngine.Object>("Prefabs_Common/PopupPrefabs/PopupUI.prefab", out var prefab);
        var popup = GameManager.UI.ShowUI(prefab.Get<NoticeUI>()) as NoticeUI;
        popup.SetString(new() { "게임을 종료하시겠습니까?".GetLanguage(), "예".GetLanguage(), "아니요".GetLanguage() });
        popup.SetCallback(new() { () =>
                {
                    GameManager.UI.CloseTopUI();
        #if UNITY_EDITOR
                    EditorApplication.isPlaying = false;
        #else
                    QuitApplication();
        #endif            
                },
                () =>
                {
                    GameManager.UI.CloseTopUI();
                }});
    }

    private void QuitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
