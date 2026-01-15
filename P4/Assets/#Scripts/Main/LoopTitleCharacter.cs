using UnityEngine;

public class LoopTitleCharacter : MonoBehaviour
{
    [SerializeField] private float _speed = 200f;
    [SerializeField] private float _leftX = -800f;
    [SerializeField] private float _rightX = 800f;
    [SerializeField] private RectTransform _rect;

    void Update()
    {
        _rect.anchoredPosition += Vector2.right * _speed * Time.deltaTime;

        if (_rect.anchoredPosition.x > _rightX)
        {
            Vector2 pos = _rect.anchoredPosition;
            pos.x = _leftX;
            _rect.anchoredPosition = pos;
        }
    }
}
