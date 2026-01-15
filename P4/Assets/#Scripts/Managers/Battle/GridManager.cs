using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridManager
{
    private Dictionary<Vector3Int, List<Monster>> _monsterGrid = new Dictionary<Vector3Int, List<Monster>>();
    private Dictionary<Vector3Int, Unit> _towerGrid = new Dictionary<Vector3Int, Unit>();

    #region 몬스터가 호출하는 함수

    public void OnMonsterSpawn(Monster monster, Vector3Int cell)
    {
        OnMonsterMove(monster, cell, cell);
    }

    public void OnMonsterMove(Monster monster, Vector3Int oldCell, Vector3Int newCell)
    {
        if (oldCell != newCell && _monsterGrid.ContainsKey(oldCell))
        {
            _monsterGrid[oldCell].Remove(monster);
        }

        if (!_monsterGrid.ContainsKey(newCell))
        {
            _monsterGrid[newCell] = new List<Monster>();
        }
        _monsterGrid[newCell].Add(monster);
    }

    public void OnMonsterDie(Monster monster, Vector3Int cell)
    {
        if (_monsterGrid.ContainsKey(cell))
        {
            _monsterGrid[cell].Remove(monster);
        }
    }

    #endregion

    #region 타워가 호출하는 함수
    public void OnTowerPlaced(Unit tower, Vector3Int cell)
    {
        _towerGrid[cell] = tower;
        BattleManager.Instance.CurrentPlacementCount++;

        if (tower is Tanker tanker)
        {
            var monstersInCell = GetMonstersInCell(cell);
            if (monstersInCell != null)
            {
                foreach (var monster in monstersInCell.ToList())
                {
                    if(monster.CurrentCell == cell)
                        tanker.TryBlockMonster(monster);
                }
            }
        }
    }

    public Vector3Int GetTowerCell(Unit tower)
    {
        foreach (var kvp in _towerGrid)
        {
            if (kvp.Value == tower)
                return kvp.Key;
        }

        return Vector3Int.zero;
    }

    public void OnTowerRemoved(Vector3Int cell)
    {
        if (_towerGrid.ContainsKey(cell))
        {
            _towerGrid.Remove(cell);
            BattleManager.Instance.CurrentPlacementCount--;
        }
    }

    #endregion

    public List<Monster> GetMonstersInCell(Vector3Int cell)
    {
        _monsterGrid.TryGetValue(cell, out var monsterList);
        return monsterList;
    }

    public Unit GetTowerInCell(Vector3Int cell)
    {
        _towerGrid.TryGetValue(cell, out var tower);
        return tower;
    }
}