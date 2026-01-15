using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Barrel : Tower
{
    private bool _hasExploded = false;

    private void Update()
    {
        if (_hasExploded) return;

        if (IsMonsterInRange())
        {
            Attack();
        }
    }

    private bool IsMonsterInRange()
    {
        if (_data.ParsedAttackRange == null) return false;

        foreach (var relativePos in _data.ParsedAttackRange)
        {
            Vector3Int targetCell = _myCell + relativePos;
            var monsters = BattleManager.Grid.GetMonstersInCell(targetCell);

            if (monsters != null && monsters.Count > 0)
            {
                foreach (var monster in monsters)
                {
                    if (monster != null && monster.gameObject.activeInHierarchy)
                        return true;
                }
            }
        }

        return false;
    }

    private void Attack()
    {
        if (_hasExploded) return;
        _hasExploded = true;

        if (_anim != null)
        {
            _anim.SetTrigger("IsAttack");
        }
        else
        {
            OnAttackEvent();
        }
    }

    public void OnAttackEvent()
    {
        if (_data.ParsedAttackRange == null) return;

        HashSet<Monster> damagedMonsters = new HashSet<Monster>();

        foreach (var relativePos in _data.ParsedAttackRange)
        {
            Vector3Int targetCell = _myCell + relativePos;
            var monsters = BattleManager.Grid.GetMonstersInCell(targetCell);

            if (monsters == null) continue;

            for (int i = 0; i < monsters.Count; i++)
            {
                if (monsters[i] == null) continue;
                if (damagedMonsters.Contains(monsters[i])) continue;

                bool isDead = monsters[i].TakeDamage(_attack);
                if (isDead) continue;
                damagedMonsters.Add(monsters[i]);
            }
        }


        if (GameManager.Addressables.TryGet<UnityEngine.Object>("Prefabs_Battle/Tower/Effect/Explosion.prefab", out Object exp))
        {
            var explosion = GameObject.Instantiate(exp).GameObject();
            explosion.transform.position = this.transform.position;

            var attackRange = Utils.GetAttackRangeSize(_data.ATTACK_RANGE);
            explosion.transform.localScale = new Vector3(attackRange.width, attackRange.height, 1);
        }
        GameManager.Audio.PlaySFX(GetTowerData().ATTACK_SFX);

        Die();
    }
}
