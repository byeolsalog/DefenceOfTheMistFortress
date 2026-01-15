using UnityEngine;

public class OneShotEffect : MonoBehaviour
{
    public void OnEndEffect()
    {
        Destroy(this.gameObject);
    }
}
