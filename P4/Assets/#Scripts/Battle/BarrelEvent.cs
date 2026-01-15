using UnityEngine;

public class BarrelEvent : MonoBehaviour
{
    [SerializeField] private Barrel _barrel;

    public void OnBarrelAttack()
    {
        _barrel.OnAttackEvent();
    }
}
