using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MapBG : MonoBehaviour
{
    [SerializeField] private List<StageButton> _stageButtons;

    private void Start()
    {
        SetStageButtons();
    }

    private void SetStageButtons()
    {
        var stageTable = GameManager.Table.GetTable<StageTable>();
        if (stageTable == null) return;

        var allStages = stageTable.Get()
            .SelectMany(diffPair => diffPair.Value.Select(stagePair => (Difficulty: diffPair.Key, Stage: stagePair.Key)))
            .ToList();

        if (allStages.Count > _stageButtons.Count)
        {
            Debug.LogError($"[StageManager] StageButton 갯수 부족. 필요: {allStages.Count}, 현재: {_stageButtons.Count}");
            return;
        }

        var maxStageData = GameManager.Data.StageData;

        for (int i = 0; i < _stageButtons.Count; i++)
        {
            var button = _stageButtons[i];

            if (i < allStages.Count)
            {
                var stageData = allStages[i];

                if(i > 0 && (stageData.Difficulty > maxStageData.maxDiff || (stageData.Difficulty <= maxStageData.maxDiff) && (i > maxStageData.maxStage)))
                {
                    button.gameObject.SetActive(false);
                    continue;
                }

                button.gameObject.SetActive(true);
                button.SetData(stageData.Difficulty, stageData.Stage);
            }
            else
            {
                button.gameObject.SetActive(false);
            }
        }

        Debug.Log($"[StageManager] StageButton {allStages.Count}개 세팅 완료");
    }
}