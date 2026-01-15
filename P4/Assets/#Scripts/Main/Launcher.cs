using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Launcher : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _loginButtonsParent;
    [SerializeField] private GameObject _CCDParent;
    [SerializeField] private TextMeshProUGUI _versionText;
    [SerializeField] private TextMeshProUGUI _addressableDataText;
    [SerializeField] private Image _progress;
    [SerializeField] private TextMeshProUGUI _statusMessage;

    [Header("Popup")]
    [SerializeField] private NoticeUI _oneButtonPopup;
    [SerializeField] private NoticeUI _twoButtonPopup;
    [SerializeField] private OptionUI _optionUI;

    private Task<bool> _addressablesDownloadTask;
    private bool _isReady = false;
    private async void Start()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            ShowStatus("인터넷 연결 없음. 앱을 종료합니다.");
            QuitApplication();
            return;
        }

        //_loginButtonsParent.SetActive(false);
        _CCDParent.SetActive(false);
        SubscribeToEvents();

        _versionText.text = $"v{Application.version}";
        //var versionResult = await VersionCheck.CheckVersionAsync();
        //if (versionResult == VersionCheckResult.Failed)
        //{
        //    ShowStatus("버전 체크 실패.");
        //    QuitApplication();
        //    return;
        //}
        //else if (versionResult == VersionCheckResult.ForceUpdate)
        //{
        //    ShowStatus("버전 업데이트 필요.");
        //    //TODO: 마켓 이동 ?
        //    return;
        //}

        //_versionText.text = $"v{VersionCheck.LatestVersion}";

        //bool firebaseInitSuccess = await GameManager.Login.InitializeAsync();
        //if (!firebaseInitSuccess)
        //{
        //    ShowStatus("Firebase 초기화 실패.");
        //    QuitApplication();
        //    return;
        //}
        _isReady = false;
        _statusMessage.text = "리소스 버전 체크 중...";
        await GameManager.Addressables.InitAddressables();
        _addressablesDownloadTask = GameManager.Addressables.DownloadAllDependenciesAsync();
        await _addressablesDownloadTask;

        _statusMessage.text = "테이블 데이터 로드 중...";
        await GameManager.Table.LoadAllTablesAsync();
        await GameManager.Addressables.PreloadByLabelAsync<Sprite>(EAddressablesLabel.Sprite.ToString());
        await GameManager.Addressables.PreloadByLabelAsync<UnityEngine.Object>(EAddressablesLabel.Prefab_Common.ToString());
        var bgmTask = await GameManager.Addressables.PreloadByLabelAsync<AudioClip>(EAddressablesLabel.BGM.ToString());
        GameManager.Audio.LoadBGMs(bgmTask);

        //_loginButtonsParent.SetActive(!GameManager.Login.IsUserSignedIn());
        _statusMessage.text = "Press Start";
        _isReady = true;
    }

    #region Button Click Handlers
    public async void OnClick_SignInAnonymously()
    {
        _loginButtonsParent.SetActive(false);
        ShowStatus("익명 로그인 중...");
        var loginTask = GameManager.Login.SignInAnonymouslyAsync();
        await ProcessLoginAndDownload(loginTask);
    }

    public async void OnClick_SignInWithGoogle()
    {
        _loginButtonsParent.SetActive(false);
        ShowStatus("Google 로그인 중...");
        var loginTask = GameManager.Login.SignInWithGpgsAsync();
        await ProcessLoginAndDownload(loginTask);
    }

    private async Task ProcessLoginAndDownload(Task<bool> loginTask)
    {
        await Task.WhenAll(_addressablesDownloadTask, loginTask);
        bool downloadSuccess = await _addressablesDownloadTask;
        bool loginSuccess = await loginTask;


        if (downloadSuccess && loginSuccess)
        {
            ShowStatus("로그인 및 다운로드 완료. 로비로 이동합니다...");

            //await GameManager.Net.ConnectToGameServer();
            ProceedToLobby();
        }
        else
        {
            ShowStatus($"실패! Download: {downloadSuccess}, Login: {loginSuccess}");
            _loginButtonsParent.SetActive(true);
        }
    }
    #endregion

    private async void ProceedToLobby()
    {
        await Task.Delay(1000);
        Debug.Log("모든 준비 완료. 로비 씬으로 이동합니다.");
        GameManager.Scene.LoadScene(EGameScene.Robby);
    }

    #region Event Handlers
    private void SubscribeToEvents()
    {
        GameManager.Addressables.OnDownloadStarted += HandleDownloadStarted;
        GameManager.Addressables.OnProgressUpdated += HandleProgressUpdated;
        GameManager.Addressables.OnDownloadCompleted += HandleDownloadCompleted;
    }

    private void UnsubscribeFromEvents()
    {
        GameManager.Addressables.OnDownloadStarted -= HandleDownloadStarted;
        GameManager.Addressables.OnProgressUpdated -= HandleProgressUpdated;
        GameManager.Addressables.OnDownloadCompleted -= HandleDownloadCompleted;
    }

    private void HandleDownloadStarted(long totalSize)
    {
        _CCDParent.SetActive(true);
        _progress.fillAmount = 0f;
        _addressableDataText.text = $"0.00 MB / {totalSize / (1024f * 1024f):F2} MB";
        ShowStatus("데이터 다운로드 시작...");
    }

    private void HandleProgressUpdated(float percent, long downloadedBytes, long totalSize)
    {
        _progress.fillAmount = percent;
        _addressableDataText.text = $"{downloadedBytes / (1024f * 1024f):F2} MB / {totalSize / (1024f * 1024f):F2} MB";
    }

    private void HandleDownloadCompleted(bool success)
    {
        _CCDParent.SetActive(false);
        var popup = GameManager.UI.ShowUI(_oneButtonPopup) as NoticeUI;
        popup.SetString(new() { success ? "리소스 다운로드를 완료했습니다.\n게임을 재시작합니다.".GetLanguage() : "리소스 다운로드에 실패했습니다.".GetLanguage(), "확인" });
        popup.SetCallback(new() { () =>
        {
            if(success)
            {
                GameManager.Scene.LoadScene(EGameScene.Login);
            }
            else
            {
                Application.Quit();
            }
        } });
    }
    #endregion

    public void OnClickClearCache()
    {
        var popup = GameManager.UI.ShowUI(_twoButtonPopup) as NoticeUI;
        popup.SetString(new() { "정말로 게임 데이터를 삭제하시겠습니까?".GetLanguage(), "예".GetLanguage(), "아니요".GetLanguage() });
        popup.SetCallback(new() { () =>
        {
            Debug.Log("어드레서블 캐시 삭제 시작...");
            GameManager.Addressables.ClearCache();
            GameManager.UI.CloseTopUI();

            bool success = Caching.ClearCache();
            if (success)
            {
                Debug.Log("Unity Caching.ClearCache() 성공");                
            }
            else
            {
                Debug.Log("Unity Caching.ClearCache() 실패");
            }

            GameManager.Scene.LoadScene(EGameScene.Login);
        },
        () => 
        {
            GameManager.UI.CloseTopUI();
        }});
    }

    public void OnClickShowOption()
    {        
        GameManager.UI.ShowUI(_optionUI);
    }

    public void OnClickChangeAccount()
    {
        var popup = GameManager.UI.ShowUI(_twoButtonPopup) as NoticeUI;
        popup.SetString(new() { "계정 변경을 하시겠습니까?\n(현재 익명 로그인 상태라면 계정이 초기화됩니다)".GetLanguage(), "예".GetLanguage(), "아니요".GetLanguage() });
        popup.SetCallback(new() { () =>
        {
            GameManager.Login.SignOut();
            GameManager.UI.CloseTopUI();
            _loginButtonsParent.SetActive(true);
        }});
    }

    public void OnClickQuitGame()
    {
        var popup = GameManager.UI.ShowUI(_twoButtonPopup) as NoticeUI;
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

    public void OnClickStartGame()
    {
        if (!_isReady) return;
        ProceedToLobby();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void QuitApplication()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void ShowStatus(string message)
    {
        if (_statusMessage != null)
        {
            _statusMessage.text = message;
        }
        Debug.Log(message);
    }
}