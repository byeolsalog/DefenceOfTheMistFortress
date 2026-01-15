using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public interface IBlocker
{
    void TryBlockMonster(Monster monster);
    void RemoveBlockedMonster(Monster monster);
    int GetBlockCapacity();
}

public class Tower : Unit
{    
    protected TowerEntry _data;
    protected Vector3Int _myCell;
    protected ETowerState _state = ETowerState.Idle;
    protected Monster _currentTarget = null;

    public virtual void Init(Vector3Int cell, TowerEntry towerData)
    {
        _myCell = cell;
        _data = towerData;

        _maxHealth = _data.HP;
        _currentHealth = _maxHealth;
        _speed = _data.ATTACK_SPEED;
        _attack = _data.ATTACK;
        _defence = _data.DEFENCE;

        BattleManager.Grid.OnTowerPlaced(this, _myCell);
        LookAtTarget(TileMapReader.Instance.GetFirstSpawnWorldPos());
        SetAnimAttackSpeed();
    }

    public TowerEntry GetTowerData()
    {
        return _data;
    }

    public void Retreat()
    {
        Die();
    }

    protected override void Die()
    {
        BattleManager.Grid.OnTowerRemoved(_myCell);
        base.Die();
    }


    protected virtual void FindNewTarget()
    {
        _currentTarget = null;
        if (_data.ParsedAttackRange == null || _data.ParsedAttackRange.Count <= 0) return;

        foreach (var relativePos in _data.ParsedAttackRange)
        {
            Vector3Int targetCell = _myCell + relativePos;
            List<Monster> monstersInCell = BattleManager.Grid.GetMonstersInCell(targetCell);

            if (monstersInCell != null && monstersInCell.Count > 0)
            {
                foreach (var monster in monstersInCell)
                {
                    if (monster != null && monster.gameObject.activeInHierarchy)
                    {
                        _currentTarget = monster;
                        return;
                    }
                }
            }
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (_data == null || _data.ParsedAttackRange == null) return;

        Gizmos.color = Color.black;
        foreach (var relativePos in _data.ParsedAttackRange)
        {
            Vector3 worldPos = new Vector3(transform.position.x + relativePos.x, transform.position.y + relativePos.y, 0);
            Gizmos.DrawWireCube(worldPos, Vector3.one * 0.9f);
        }
    }
}