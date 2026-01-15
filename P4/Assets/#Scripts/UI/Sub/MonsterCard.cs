using UnityEngine;
using UnityEngine.UI;

public class MonsterCard : MonoBehaviour
{
    [SerializeField] private Image monsterImage;

    public void SetImage(Sprite sprite)
    {
        monsterImage.sprite = sprite;
        monsterImage.SetNativeSize();
    }
}
