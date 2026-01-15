using UnityEngine;

public class ClickableTileItem : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private GameObject _rangeSlot;
    private Color _originalColor;

    public ETileType _type;

    private void Awake()
    {
        if (_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
        }
    }

    public void SetHighlightColor(Color color)
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = color;
        }
    }

    public void ResetColor()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _originalColor;
        }
    }

    public void SetActiveRangeSlot(bool isActive)
    {
        _rangeSlot.SetActive(isActive);
    }

    private void OnDrawGizmos()
    {
        switch (_type)
        {
            case ETileType.Grass:
                Gizmos.color = Color.green;
                break;
            case ETileType.Road:
                Gizmos.color = Color.blue;
                break;
            case ETileType.Rock:
            case ETileType.Water:
                Gizmos.color = Color.red;
                break;
            case ETileType.Spawn:
                Gizmos.color = Color.black;
                break;
            case ETileType.Goal:
                Gizmos.color = Color.cyan;
                break;
            default:
                Gizmos.color = Color.gray;
                break;
        }

        
        Gizmos.DrawWireCube(this.transform.position, Vector3.one * 0.9f);        
    }
}