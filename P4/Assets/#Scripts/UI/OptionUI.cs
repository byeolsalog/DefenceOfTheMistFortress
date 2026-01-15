using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionUI : UI_Base
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _bgmText;
    [SerializeField] private TextMeshProUGUI _bgmPercentText;
    [SerializeField] private TextMeshProUGUI _soundEffectText;
    [SerializeField] private TextMeshProUGUI _soundEffectPercentText;
    [SerializeField] private TextMeshProUGUI _giveupText;
    [SerializeField] private TextMeshProUGUI _closeText;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _soundEffectSlider;
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _giveupButton;


    protected override void Init()
    {
        base.Init();
        RefreshUI();

        _closeButton.onClick.RemoveAllListeners();
        _giveupButton?.onClick.RemoveAllListeners();

        _closeButton.onClick.AddListener(UIClose);
        _giveupButton?.onClick.AddListener(GiveUp);
    }

    private void OnDisable()
    {
        GameManager.Data.SaveData(EFileDataType.Option);
    }

    private void RefreshUI()
    {
        var optionData = GameManager.Data.OptionData;
        _titleText.text = "Option".GetLanguage();
        _bgmText.text = "BGM".GetLanguage();
        _soundEffectText.text = "Sound Effect".GetLanguage();
        _bgmSlider.value = optionData.bgmVolume;
        _soundEffectSlider.value = optionData.sfxVolume;
        _bgmPercentText.text = $"{(int)(optionData.bgmVolume * 100)}%";
        _soundEffectPercentText.text = $"{(int)(optionData.sfxVolume * 100)}%";
        _closeText.text = "Close".GetLanguage();

        _bgmSlider.onValueChanged.RemoveAllListeners();
        _soundEffectSlider.onValueChanged.RemoveAllListeners();

        _bgmSlider.onValueChanged.AddListener((value) => 
        {
            optionData.bgmVolume = value; _bgmPercentText.text = $"{(int)(value * 100)}%"; 
            GameManager.Audio.SetBGMVolume(value);
        });
        _soundEffectSlider.onValueChanged.AddListener((value) => 
        {
            optionData.sfxVolume = value; _soundEffectPercentText.text = $"{(int)(value * 100)}%";
            GameManager.Audio.SetSFXVolume(value);
        });

        switch (GameManager.Scene.CurrentScene)
        {
            case EGameScene.Battle:
                _giveupButton.gameObject.SetActive(true);
                _giveupText.text = "Give Up".GetLanguage();
                break;

            default:
                _giveupButton?.gameObject.SetActive(false);
                break;
        }
    }

    public void UIClose()
    {
        GameManager.UI.CloseUI(this);
    }

    public void GiveUp()
    {
        GameManager.Addressables.TryGet<UnityEngine.Object>("Prefabs_Common/PopupPrefabs/PopupUI.prefab", out var prefab);
        var popup = GameManager.UI.ShowUI(prefab.Get<NoticeUI>()) as NoticeUI;
        popup.SetString(new() { "giveup_battle".GetLanguage(), "yes".GetLanguage(), "no".GetLanguage()} );
        popup.SetCallback(new()
        {
            () =>
            {
                if (BattleManager.Instance == null)
                {
                    GameManager.Scene.LoadScene(EGameScene.Robby);
                }
                else
                {
                    BattleManager.Instance.GameFail();
                }
            },
            () => popup.OnClickClose()
        });
    }
}
