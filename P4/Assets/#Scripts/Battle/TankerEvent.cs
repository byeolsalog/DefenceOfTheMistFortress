using UnityEngine;

public class TankerEvent : MonoBehaviour
{
    [SerializeField] private Tanker _tanker;

    public void OnTankerAttack()
    {
        _tanker.OnAttackEvent();
    }

    public void OnTankerAttackFinished()
    {
        _tanker.OnAttackFinished();
    }
}
