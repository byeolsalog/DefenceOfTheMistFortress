using UnityEngine;

public class Monster : Unit
{
    private static readonly int _hashAttack = Animator.StringToHash("IsAttack");
    private static readonly int _hashWalk = Animator.StringToHash("IsWalk");

    private EMonsterState _currentState = EMonsterState.Moving;
    private Unit _targetTanker;
    protected MonsterEntry _data;
    private Vector3Int _currentCell;
    private Vector3Int _nextCell;
    private Vector3 _targetWorldPos;
    private float _attackCooldown = 0f;

    public Vector3Int CurrentCell => _currentCell;

    public void Init(MonsterEntry data, Vector3Int startCell)
    {
        _data = data;

        _maxHealth = _data.HP;
        _currentHealth = _maxHealth;
        _speed = _data.SPEED;
        _attack = _data.ATTACK;
        _defence = _data.DEFENCE;

        _currentCell = TileMapReader.Instance.GetNextCell(startCell);
        UpdateNextTarget();
        SetAnimAttackSpeed();
        transform.position = TileMapReader.Instance.GetWorldPosFromIndexCell(_currentCell);
        BattleManager.Grid.OnMonsterSpawn(this, _currentCell);
        BattleManager.Instance.CurrentMonsterCount++;
    }

    private void Update()
    {
        if (_currentState == EMonsterState.Moving)
        {
            Unit unitInNextCell = BattleManager.Grid.GetTowerInCell(_nextCell);
            if(unitInNextCell != null && unitInNextCell is Tanker tanker)
            {
                tanker.TryBlockMonster(this);

                if(_currentState != EMonsterState.Engaged)
                {
                    MoveToTarget();
                }
            }
            else
            {
                MoveToTarget();
            }                
        }

        if (_currentState == EMonsterState.Engaged)
        {
            if (_targetTanker == null)
            {
                Disengage();
                return;
            }

            LookAtTarget(_targetTanker.transform.position);
            _attackCooldown -= Time.deltaTime;
            if (_attackCooldown <= 0f)
            {
                Attack();
                _attackCooldown = 1.5f;
            }
        }
    }

    public float GetRadius()
    {
        return 0.5f;
    }

    protected void Attack()
    {
        if (_targetTanker == null) return;
        _anim.SetTrigger(_hashAttack);
        _targetTanker.TakeDamage(_attack);
    }

    protected virtual void MoveToTarget()
    {
        LookAtTarget(_targetWorldPos);
        _anim.SetBool(_hashWalk, true);
        transform.position = Vector3.MoveTowards(
            transform.position,
            _targetWorldPos,
            _speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, _targetWorldPos) < 0.01f)
        {
            var oldCell = _currentCell;
            _currentCell = _nextCell;
            BattleManager.Grid.OnMonsterMove(this, oldCell, _currentCell);
            UpdateNextTarget();
            if (_currentCell == _nextCell)
            {
                ReachGoal();
            }
        }
    }

    private void UpdateNextTarget()
    {
        _nextCell = TileMapReader.Instance.GetNextCell(_currentCell);
        _targetWorldPos = TileMapReader.Instance.GetWorldPosFromIndexCell(_nextCell);
    }

    private void ReachGoal()
    {
        Debug.Log("몬스터가 목표 지점에 도달했습니다!");
        BattleManager.Instance.LifeCount--;
        BattleManager.Instance.CurrentMonsterCount--;
        Destroy(gameObject);
    }

    protected override void Die()
    {
        BattleManager.Grid.OnMonsterDie(this, _currentCell);

        if (_currentState == EMonsterState.Engaged && _targetTanker != null)
        {
            var tankerScript = _targetTanker.GetComponent<Tanker>();
            if (tankerScript != null)
            {
                tankerScript.RemoveBlockedMonster(this);
            }
        }

        BattleManager.Instance.Cost += _data.REWARD;
        BattleManager.Instance.CurrentMonsterCount--;
        BattleManager.Instance.KillCount++;
        base.Die();
    }

    public virtual void Engage(Unit tanker)
    {
        _anim.SetBool(_hashWalk, false);
        _currentState = EMonsterState.Engaged;
        _targetTanker = tanker;
    }

    public void Disengage()
    {
        _currentState = EMonsterState.Moving;
        _targetTanker = null;
    }

    public bool CanBeBlocked()
    {
        return _currentState == EMonsterState.Moving;
    }
}