 using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            Init();
            return _instance;
        }
    }

    [SerializeField] private TableRegistrySO _tableRegistry;
    [SerializeField] private Canvas _popupParent;

    private LoginManager _login;
    private AddressablesManager _addressables;
    private SceneManager _scene;
    private UIManager _ui;
    private DataManager _data;
    private NetManager _net;
    private TableManager _table;
    private AudioManager _audio;
    public static LoginManager Login => Instance.GetOrCreateManager(ref Instance._login);
    public static AddressablesManager Addressables => Instance.GetOrCreateManager(ref Instance._addressables);
    public static SceneManager Scene => Instance.GetOrCreateManager(ref Instance._scene);
    public static UIManager UI => Instance.GetOrCreateManager(ref Instance._ui);
    public static DataManager Data => Instance.GetOrCreateManager(ref Instance._data);
    public static NetManager Net => Instance.GetOrCreateManager(ref Instance._net);

    public static TableManager Table
    {
        get
        {
            if(Instance._table == null)
            {
                if (Instance._tableRegistry == null)
                    Debug.LogError("[GameManager] TableRegistry가 할당되지 않음.");

                Instance._table = new TableManager(Instance._tableRegistry);
            }
            return Instance._table;
        }
    }
    public static AudioManager Audio => Instance.GetOrCreateManager(ref Instance._audio);

    private T GetOrCreateManager<T>(ref T manager) where T : class, new()
    {
        if (manager == null)
            manager = new T();

        return manager;
    }

    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        Audio.Init();
    }

    static void Init()
    {
        if (_instance == null)
        {
            GameObject go = GameObject.Find("GameManager");
            if (go == null)
            {
                go = new GameObject { name = "GameManager" };
                go.AddComponent<GameManager>();
            }

            DontDestroyOnLoad(go);
            _instance = go.GetComponent<GameManager>();
        }
    }

    public Canvas GetPopupParent()
    {
        return _popupParent;
    }

    private void OnApplicationQuit()
    {
        //Net.Disconnect();
    }
}