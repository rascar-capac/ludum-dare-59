using UnityEngine;

public class Singleton<TInstance> : MonoBehaviour where TInstance : Singleton<TInstance>
{
    private static TInstance _instance;

    [SerializeField] private bool _destroyOnLoad = false;

    public static TInstance Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<TInstance>();
            }

            return _instance;
        }
    }

    public static bool HasInstance => _instance != null;

    public static bool TryGetInstance(out TInstance instance)
    {
        instance = Instance;

        return instance != null;
    }

    protected virtual void Awake()
    {
        if (!_destroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        if (_instance == null)
        {
            _instance = this as TInstance;
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"There are two instances of {_instance.name}. The latest one will be destroyed.", _instance);

            Destroy(this);
        }
    }
}
