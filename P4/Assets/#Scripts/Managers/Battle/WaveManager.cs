using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    private static WaveManager _instance;
    public static WaveManager Instance => _instance;

    private float _battleTime = 0f;
    private int _currentWave = 0;
    private bool _isWaveProcessing = false;

    private Dictionary<int, WaveEntry> _waveEntries;
    private List<WaveEventEntry> _allWaveEvents;
    private MonsterTable _monsterTable;


    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        int diff = GameManager.Data.StageData.Diff;
        int stage = GameManager.Data.StageData.Stage;

        _waveEntries = GameManager.Table.GetTable<WaveTable>().Get(diff, stage);
        _allWaveEvents = GameManager.Table.GetTable<WaveEventTable>().Get(diff, stage); 
        _monsterTable = GameManager.Table.GetTable<MonsterTable>();

        if (_waveEntries == null || _allWaveEvents == null || _monsterTable == null)
        {
            Debug.LogError("웨이브 테이블 로드에 실패했습니다!");
            this.enabled = false;
            return;
        }

        StartBattle();
    }

    public void StartBattle()
    {        
        _battleTime = 0f;
        _currentWave = 1;
    }

    private void Update()
    {
        if (BattleManager.Instance != null && !BattleManager.Instance.IsBattleReady)
            return;

        if (_currentWave == 0) return;

        _battleTime += Time.deltaTime;

        if (_isWaveProcessing) return;
        if (_waveEntries.TryGetValue(_currentWave, out var nextWave))
        {
            if (_battleTime >= nextWave.START_TIME)
            {
                Debug.Log($"--- Wave {_currentWave} 시작! (Battle Time: {_battleTime}) ---");
                StartCoroutine(ProcessWave(_currentWave));
            }
        }
        else
        {
            if (!_isWaveProcessing)
            {
                Debug.Log("모든 웨이브 스폰 종료!");
                _currentWave = 0;
            }
        }
    }

    private IEnumerator ProcessWave(int waveIndex)
    {
        _isWaveProcessing = true;

        var eventsForThisWave = _allWaveEvents
            .Where(e => e.WAVE == waveIndex)
            .OrderBy(e => e.TIME)
            .ToList();

        float waveTime = 0f;
        while (eventsForThisWave.Count > 0)
        {
            waveTime += Time.deltaTime;

            var dueEvents = eventsForThisWave.Where(e => waveTime >= e.TIME).ToList();

            foreach (var evt in dueEvents)
            {
                StartCoroutine(SpawnEvent(evt));
                eventsForThisWave.Remove(evt);
            }

            yield return null;
        }

        _isWaveProcessing = false;
        _currentWave++;
    }
    private IEnumerator SpawnEvent(WaveEventEntry evt)
    {
        MonsterEntry monsterData = _monsterTable.Get(evt.MONSTER_ID);
        if (monsterData == null)
        {
            Debug.LogError($"Monster ID {evt.MONSTER_ID}를 찾을 수 없습니다.");
            yield break;
        }

        Vector3Int spawnCell = TileMapReader.Instance.GetSpawnCell(evt.SPAWN_ID);
        Vector3 spawnPos = TileMapReader.Instance.GetWorldPosFromIndexCell(spawnCell);

        if (!GameManager.Addressables.TryGet(monsterData.PREFAB_PATH, out Object obj))
            yield break;

        if (!(obj is GameObject)) yield break;

        GameObject prefab = obj as GameObject;
        if (prefab != null)
        {
            for (int i = 0; i < evt.COUNT; i++)
            {
                GameObject monsterObj = Instantiate(prefab, spawnPos, Quaternion.identity);
                Monster monster = monsterObj.GetComponent<Monster>();

                if (monster != null)
                {
                    monster.Init(monsterData, spawnCell);
                }
                else
                {
                    Debug.LogError($"프리팹 {monsterData.PREFAB_PATH}에 Monster 스크립트가 없습니다.");
                    Destroy(monsterObj);
                }

                if (evt.INTERVAL > 0)
                    yield return new WaitForSeconds(evt.INTERVAL);
            }
        }
        else
        {
            Debug.LogError($"몬스터 프리팹 로드 실패: {monsterData.PREFAB_PATH}");
        }
    }
}