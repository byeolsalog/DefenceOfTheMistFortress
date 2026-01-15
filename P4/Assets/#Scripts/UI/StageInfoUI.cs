using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.Collections;

public class StageInfoUI : UI_Base
{
    [SerializeField] private TextMeshProUGUI _stageNameText;
    [SerializeField] private TextMeshProUGUI _stageMapText;
    [SerializeField] private TextMeshProUGUI _stageMonsterText;
    [SerializeField] private TextMeshProUGUI _stageStartText;
    [SerializeField] private Transform _monsterListParent;
    [SerializeField] private Image _mapImage;

    private const string MONSTER_ITEM_PATH = "Prefabs_Common/MonsterCard.prefab";
    private int _diff = 0, _stage = 0;

    public void SetData(int diff, int stage)
    {
        _diff = diff; _stage = stage;

        foreach (Transform child in _monsterListParent)        
            Destroy(child.gameObject);
        
        _stageNameText.text = "stage_num".GetLanguage(diff, stage);
        _stageMapText.text = "map_data".GetLanguage();
        _stageMonsterText.text = "monster_list".GetLanguage();
        _stageStartText.text = "entrance".GetLanguage();

        var table = GameManager.Table.GetTable<StageTable>().Get(diff, stage);
        if(GameManager.Addressables.TryGet(table.MAP, out Sprite sprite))
        {
            _mapImage.gameObject.SetActive(true);
            _mapImage.sprite = sprite;
        }
        else
        {
            _mapImage.gameObject.SetActive(false);
        }

        if(table.MONSTERS != null || table.MONSTERS.Count > 0)
        {
            var monsterTable = GameManager.Table.GetTable<MonsterTable>();
            foreach (var monsterId in table.MONSTERS)
            {
                var monsterData = monsterTable.Get(monsterId);
                if (monsterData == null)
                    continue;

                if (!GameManager.Addressables.TryGet<UnityEngine.Object>(MONSTER_ITEM_PATH, out var prefab))
                    continue;

                if (!GameManager.Addressables.TryGet<Sprite>(monsterData.SPRITE_PATH, out var monsterSprite))
                    continue;

                GameObject.Instantiate(prefab, _monsterListParent).GetComponent<MonsterCard>().SetImage(monsterSprite);
            }
        }
        
    }

    private bool _isProgress = false;

    public void OnClickStageStartButton()
    {
        if (_isProgress) return;

        StartCoroutine(CoStageStart());
    }

    private IEnumerator CoStageStart()
    {
        GameManager.Scene.LoadBattleScene(_diff, _stage);

        yield return new WaitForSeconds(1f);

        _isProgress = false;
    }

    public void OnClickStageInfoCloseButton()
    {
        GameManager.UI.CloseUI(this);
    }
}
