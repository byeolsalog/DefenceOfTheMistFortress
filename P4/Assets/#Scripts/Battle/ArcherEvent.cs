using Unity.VisualScripting;
using UnityEngine;

public class ArcherEvent : MonoBehaviour
{
    [SerializeField] private Archer _archer;

    public void OnArcherAttackStartEvent()
    {
        _archer.OnAttackEvent();
    }

    public void OnArcherAttackEndEvent()
    {
        _archer.OnAttackFinished();
    }
}
