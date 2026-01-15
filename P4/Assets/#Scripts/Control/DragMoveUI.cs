using UnityEngine;
using UnityEngine.EventSystems;

public class MapDragController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drag Settings")]
    [SerializeField] private float _dragSpeed = 1.0f;
    [SerializeField] private float _deceleration = 5f;
    [SerializeField] private RectTransform _viewport;
    [SerializeField] private RectTransform _mapRect;
    private Vector2 lastDragPosition;
    private Vector2 velocity;
    private bool isDragging;

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        velocity = Vector2.zero;
        lastDragPosition = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - lastDragPosition;
        lastDragPosition = eventData.position;

        Vector2 move = delta * _dragSpeed;
        _mapRect.anchoredPosition += move;

        if (Time.deltaTime > 0f)
            velocity = move / Time.deltaTime;
        else
            velocity = Vector2.zero;

        ClampToViewport();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    void Update()
    {
        if (!isDragging && velocity.magnitude > 0.1f)
        {
            _mapRect.anchoredPosition += velocity * Time.deltaTime;
            velocity = Vector2.Lerp(velocity, Vector2.zero, _deceleration * Time.deltaTime);

            ClampToViewport();
        }
    }

    private void ClampToViewport()
    {
        if (_viewport == null || _mapRect == null) return;

        Vector2 mapSize = _mapRect.rect.size;
        Vector2 viewSize = _viewport.rect.size;

        Vector2 mapPivot = _mapRect.pivot;
        Vector2 viewPivot = _viewport.pivot;

        Vector2 pos = _mapRect.anchoredPosition;

        float minX = -(mapSize.x * (1 - mapPivot.x)) + (viewSize.x * (1 - viewPivot.x));
        float maxX = (mapSize.x * mapPivot.x) - (viewSize.x * viewPivot.x);

        float minY = -(mapSize.y * (1 - mapPivot.y)) + (viewSize.y * (1 - viewPivot.y));
        float maxY = (mapSize.y * mapPivot.y) - (viewSize.y * viewPivot.y);

        pos.x = Mathf.Clamp(pos.x, -maxX, -minX);
        pos.y = Mathf.Clamp(pos.y, -maxY, -minY);

        _mapRect.anchoredPosition = pos;
    }
}