using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingUI : UI_Base
{
    [Header("Components")]
    [SerializeField] private Slider _progressBar;
    [SerializeField] private TextMeshProUGUI _progressText;
    [SerializeField] private TextMeshProUGUI _loadingText;

    protected override void Init()
    {
        base.Init();
        SetProgress(0f);
        _loadingText.text = "Loading";
        StartCoroutine(CoLoadingAnim());
    }
     
    public void SetProgress(float value)
    {
        Debug.Log($"ÁøÇàµµ : {value}");
        if (_progressBar != null)
            _progressBar.value = value;

        if (_progressText != null)
            _progressText.text = $"{(value * 100):F0}%";
    }

    private IEnumerator CoLoadingAnim()
    {
        int dotCount = 0;

        while (true)
        {
            dotCount = (dotCount + 1) % 4;
            _loadingText.text = "Loading" + new string('.', dotCount);
            yield return new WaitForSeconds(0.5f);
        }
    }
}