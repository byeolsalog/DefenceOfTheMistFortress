using UnityEngine;

public class CameraZoomController : MonoBehaviour
{
    private static CameraZoomController _instance;
    public static CameraZoomController Instance => _instance;

    [Header("Target Tiles (Map Size)")]
    public float _tilesX;
    public float _tilesY;

    private const float _cellSizeX = 1f;
    private const float _cellSizeY = 1f;

    [Header("Zoom Settings")]
    public float _normalZoom = 5f;
    public float _placementZoom = 3f;
    public float _zoomSpeed = 20f;

    [Header("Move Settings")]
    public float _moveSpeed = 20f;

    [SerializeField] private Camera _cam;

    private Vector3 _targetPos;
    private float _targetZoom;
    private bool _isFocusing = false;

    private Vector3 _originPos;

    private int _lastW, _lastH;
    private float _lastAspect;
    private float _lastOrtho;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;

        _cam.orthographic = true;

        Apply();

        _targetPos = transform.position;
        _targetZoom = _normalZoom;
        _originPos = transform.position;
    }

    private void Update()
    {
        _cam.orthographicSize = Mathf.Lerp(_cam.orthographicSize, _targetZoom, Time.deltaTime * _zoomSpeed);
        transform.position = Vector3.Lerp(transform.position, _targetPos, Time.deltaTime * _moveSpeed);
        if (_isFocusing)
            return;

        if (Screen.width != _lastW ||
            Screen.height != _lastH ||
            Mathf.Abs(_cam.orthographicSize - _lastOrtho) > 0.0001f ||
            Mathf.Abs(_cam.aspect - _lastAspect) > 0.0001f)
        {
            Apply();
        }
    }

    public void Apply()
    {
        _lastW = Screen.width;
        _lastH = Screen.height;
        _lastAspect = _cam.aspect;

        float worldWidth = _tilesX * _cellSizeX;
        float worldHeight = _tilesY * _cellSizeY;
        float sizeByHeight = worldHeight * 0.5f;
        float sizeByWidth = (worldWidth / _cam.aspect) * 0.5f;
        _cam.orthographicSize = Mathf.Max(sizeByHeight, sizeByWidth);

        _lastOrtho = _cam.orthographicSize;

        const float pixelsPerUnit = 32f;
        float step = 1f / pixelsPerUnit;

        Vector3 p = _cam.transform.position;
        p.x = Mathf.Round(p.x / step) * step;
        p.y = Mathf.Round(p.y / step) * step;
        _cam.transform.position = p;
    }

    public void FocusOnPosition(Vector3 worldPos)
    {
        _isFocusing = true;

        _targetPos = new Vector3(worldPos.x, worldPos.y, transform.position.z);
        _targetZoom = _placementZoom;
    }

    public void ResetCamera()
    {
        _targetPos = _originPos;
        _targetZoom = _normalZoom;
        _isFocusing = false;
    }
}