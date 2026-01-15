using UnityEngine;

public class UI_Base : MonoBehaviour
{
    private bool _isInit = false;
    [SerializeField] private EUIType _type = EUIType.Popup;
    public EUIType Type => _type;

    private void Awake()
    {
        if (_isInit) return;
        Init();
    }

    protected virtual void Init()
    {
        _isInit = true;
    }
}
