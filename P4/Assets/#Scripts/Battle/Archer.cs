using System.Collections;
using UnityEngine;

public class Archer : Tower
{
    private float _attackCooldown = 0f;

    [SerializeField] private GameObject _projectile;
    private Vector3 _projectileBaseLocalPos;
    private Quaternion _projectileBaseLocalRot;

    private static readonly int _hashAttack = Animator.StringToHash("IsAttack");
    private bool _hasFired = false;

    public override void Init(Vector3Int cell, TowerEntry towerData)
    {
        base.Init(cell, towerData);

        if (_projectile != null)
        {
            _projectileBaseLocalPos = _projectile.transform.localPosition;
            _projectileBaseLocalRot = _projectile.transform.localRotation;
            _projectile.SetActive(false);
        }
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
            if (_currentTarget != null)
                Debug.Log($"Archer 새 타깃 : {_currentTarget.name}");
        }

        if (_currentTarget != null && _attackCooldown <= 0f)
        {
            LookAtTarget(_currentTarget.transform.position);
            Debug.Log($"Archer 공격: {_currentTarget.name}");
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

    private void Attack()
    {
        if (_currentTarget == null) return;
        _state = ETowerState.Attack;
        _attackCooldown = 1.0f / _data.ATTACK_SPEED;
        _hasFired = false;

        if (_anim != null)
        {
            _anim.SetTrigger(_hashAttack);
        }
        else
        {
            OnAttackEvent();
            OnAttackFinished();
        }
    }

    public void OnAttackEvent()
    {
        if (_currentTarget == null || _projectile == null) return;

        if (_hasFired)
            return;

        _hasFired = true;

        GameManager.Audio.PlaySFX(GetTowerData().ATTACK_SFX);
        StartCoroutine(CoFireProjectile(_currentTarget));
    }

    public void OnAttackFinished()
    {
        _state = ETowerState.Idle;
    }

    private IEnumerator CoFireProjectile(Monster target)
    {
        if (target == null) yield break;

        Monster firingTarget = target;

        Vector3 lastPosition = firingTarget.transform.position;
        Vector3Int targetCell = firingTarget.CurrentCell;

        _projectile.transform.localPosition = _projectileBaseLocalPos;
        _projectile.transform.localRotation = _projectileBaseLocalRot;
        _projectile.SetActive(true);

        int distance = Mathf.Abs(_myCell.x - targetCell.x) + Mathf.Abs(_myCell.y - targetCell.y);
        float travelTime = Mathf.Max(0.1f, distance * 0.1f);
        float timer = 0f;
        Vector3 startLocalPos = _projectile.transform.localPosition;

        float monsterRadius = firingTarget.GetRadius();

        while (timer < travelTime)
        {
            timer += Time.deltaTime;
            float t = timer / travelTime;

            if (firingTarget != null && firingTarget.gameObject.activeInHierarchy)
            {
                lastPosition = firingTarget.transform.position;
            }

            Vector3 targetLocalPos = transform.InverseTransformPoint(lastPosition);
            _projectile.transform.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, t);

            float currentDistanceToTarget = Vector3.Distance(_projectile.transform.position, lastPosition);
            if (currentDistanceToTarget < monsterRadius)
            {
                _projectile.SetActive(false);
                _projectile.transform.localPosition = _projectileBaseLocalPos;
                _projectile.transform.localRotation = _projectileBaseLocalRot;

                if (firingTarget != null && firingTarget.gameObject.activeInHierarchy)
                {
                    Debug.Log($"{gameObject.name}의 화살이 {firingTarget.name}에게 {_attack}의 데미지");
                    firingTarget.TakeDamage(_attack);
                }

                yield break;
            }

            Vector3 directionToTarget = lastPosition - _projectile.transform.position;
            directionToTarget.z = 0f;
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            _projectile.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }

        _projectile.SetActive(false);
        _projectile.transform.localPosition = _projectileBaseLocalPos;
        _projectile.transform.localRotation = _projectileBaseLocalRot;

        if (firingTarget != null && firingTarget.gameObject.activeInHierarchy)
        {
            firingTarget.TakeDamage(_attack);
        }
    }
}