using System.Collections;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] protected float _maxHealth;
    [SerializeField] protected float _attack;
    [SerializeField] protected float _defence;
    [SerializeField] protected float _speed;

    [SerializeField] protected Animator _anim;
    [SerializeField] private AnimationClip _attackAnimClip;
    [SerializeField] protected SpriteRenderer _unit;

    private static readonly int HitFlashId = Shader.PropertyToID("_HitFlash");

    private MaterialPropertyBlock _mpb;
    private float flashDuration = 0.08f;
    private Coroutine _flashCoroutine;

    protected float _currentHealth;
    private float _baseScaleX;
    protected bool _isDead => _currentHealth <= 0;

    public float CurrentHealth => _currentHealth;
    public float ATK => _attack;
    public float DEF => _defence;
    public float SPD => _speed;

    public int Id { get; private set; }

    protected virtual void Awake() { _baseScaleX = Mathf.Abs(transform.localScale.x); _mpb = new(); }    

    protected virtual void SetAnimAttackSpeed() 
    {
        if (_anim == null) return;
        float attackDuration = 1.0f / _speed;
        float clipLength = _attackAnimClip.length;
        float speedMuliplier = clipLength / attackDuration;
        _anim.speed = speedMuliplier;
    }

    public virtual bool TakeDamage(float damage)
    {
        if(_isDead) return true;

        float finalDamage = Mathf.Max(0, damage - _defence);
        _currentHealth -= finalDamage;        
        
        if (_currentHealth <= 0)
        {
            _currentHealth = 0;

            if (_flashCoroutine != null)
                StopCoroutine(_flashCoroutine);

            Die();
            return true;
        }

        HitFlash();

        return false;
    }

    private void HitFlash()
    {
        if(_flashCoroutine != null)
            StopCoroutine(_flashCoroutine);

        _flashCoroutine = StartCoroutine(CoHitFlash());
    }

    private IEnumerator CoHitFlash()
    {
        _unit.GetPropertyBlock(_mpb);
        _mpb.SetFloat(HitFlashId, 1f);
        _unit.SetPropertyBlock(_mpb);

        yield return new WaitForSeconds(flashDuration);

        _unit.GetPropertyBlock(_mpb);
        _mpb.SetFloat(HitFlashId, 0f);
        _unit.SetPropertyBlock(_mpb);

        _flashCoroutine = null;
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    public void LookAtTarget(Vector3 targetPosition)
    {
        bool isTargetOnLeft = targetPosition.x < _unit.transform.position.x;
        Vector3 currentScale = _unit.transform.localScale;

        if(isTargetOnLeft) currentScale.x = -_baseScaleX;
        else currentScale.x = _baseScaleX;

        _unit.transform.localScale = currentScale;
    }
}