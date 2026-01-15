using TMPro;
using UnityEngine;

public class StageButton : MonoBehaviour
{
    [SerializeField] private StageInfoUI _stageInfo;
    [SerializeField] private TextMeshProUGUI _stageName;

    private int diff;
    private int stage;

    public void SetData(int diff, int stage)
    {
        this.diff = diff;
        this.stage = stage;
        _stageName.text = $"{diff}-{stage}".GetLanguage();
    }

    public void OnClickStageButton()
    {
        _stageInfo.SetData(diff, stage);
        GameManager.UI.ShowUI(_stageInfo);
    }
}
