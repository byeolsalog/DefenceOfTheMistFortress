using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tanker : Tower, IBlocker
{
    private int _blockCount = 0;
    public int GetBlockCapacity() { return _blockCount; }
    private List<Monster> _blockedMonsters = new List<Monster>();

    private float _attackCooldown = 0f;
    private bool _hasAttackedOnThisSwing = false;

    public override void Init(Vector3Int cell, TowerEntry towerData)
    {
        base.Init(cell, towerData);
        _blockCount = _data.BLOCK_COUNT;
    }

    public void TryBlockMonster(Monster monster)
    {
        if (monster.CanBeBlocked() && _blockedMonsters.Count < _blockCount)
        {
            _blockedMonsters.Add(monster);
            monster.Engage(this);
        }
    }

    public void RemoveBlockedMonster(Monster monster)
    {
        _blockedMonsters.Remove(monster);

        if (_currentTarget == monster)
            _currentTarget = null;

        var monstersInMyCell = BattleManager.Grid.GetMonstersInCell(_myCell);
        if (monstersInMyCell == null) return;

        Monster monsterToBlock = monstersInMyCell
            .FirstOrDefault(m => m.CanBeBlocked() && !_blockedMonsters.Contains(m));

        if (monsterToBlock != null)
        {
            _blockedMonsters.Add(monsterToBlock);
            monsterToBlock.Engage(this);
        }
    }

    protected override void Die()
    {
        foreach (Monster monster in _blockedMonsters.ToList())
        {
            if (monster != null) monster.Disengage();
        }

        _blockedMonsters.Clear();
        base.Die();
    }

    private void Update()
    {
        if (_data == null) return;

        if (_state == ETowerState.Attack)
        {
            if (_currentTarget != null)
                LookAtTarget(_currentTarget.transform.position);
            return;
        }

        if (_attackCooldown > 0f)
        {
            _attackCooldown -= Time.deltaTime;
            if (_attackCooldown < 0f) _attackCooldown = 0f;
        }

        if (_currentTarget == null || !_currentTarget.gameObject.activeInHierarchy || !IsTargetInRange(_currentTarget.CurrentCell))
        {
            FindNewTarget();
        }

        if (_currentTarget != null && _attackCooldown <= 0f)
        {
            LookAtTarget(_currentTarget.transform.position);
            Attack();
        }
    }

    private bool IsTargetInRange(Vector3Int monsterCell)
    {
        if (_data.ParsedAttackRange == null) return false;

        foreach (var relativePos in _data.ParsedAttackRange)
        {
            if (_myCell + relativePos == monsterCell)
                return true;
        }
        return false;
    }

    protected override void FindNewTarget()
    {
        _currentTarget = null;

        if (_data.ParsedAttackRange == null) return;

        if (_blockedMonsters.Count > 0)
        {
            _currentTarget = _blockedMonsters[0];
            return;
        }

        base.FindNewTarget();
    }

    private void Attack()
    {
        if (_currentTarget == null) return;

        _state = ETowerState.Attack;
        _attackCooldown = 1.0f / _data.ATTACK_SPEED;
        _hasAttackedOnThisSwing = false;

        if (_anim != null)
            _anim.SetTrigger("IsAttack");
        else
        {
            OnAttackEvent();
            OnAttackFinished();
        }
    }

    public void OnAttackEvent()
    {
        if (_currentTarget == null) return;

        if (_hasAttackedOnThisSwing)
            return;

        _hasAttackedOnThisSwing = true;
        GameManager.Audio.PlaySFX(GetTowerData().ATTACK_SFX);
        _currentTarget.TakeDamage(_attack);
    }

    public void OnAttackFinished()
    {
        _state = ETowerState.Idle;
    }
}