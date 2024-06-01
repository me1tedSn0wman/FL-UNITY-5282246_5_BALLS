using UnityEngine;

public class Soliton<T> : MonoBehaviour where T: MonoBehaviour
{
    private static T _instance;
    public static bool instanceExists
    {
        get { return _instance != null; }
    }

    private static object _lock = new object();

    private static bool applicationIsQuitting = false;

    public static T Instance
    {
        get
        {
            
            if (applicationIsQuitting)
            {
                Debug.Log("Instance " + typeof(T) + "already destroyed by quiiting");
                return null;
            }
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = (T)FindObjectOfType(typeof(T));

                    if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
                        Debug.LogError("more than 1 intsance of " + typeof(T));
                        return _instance;
                    }

                    if (_instance == null)
                    {
                        GameObject singleton = new GameObject();
                        _instance = singleton.AddComponent<T>();
                        singleton.name = "(Soliton) " + typeof(T).ToString();
                        Debug.Log(
                            "[Singleton] An instance of " + typeof(T) +
                            " is needed in the scene, so '" + singleton +
                            "' was created.");
                    }
                }
//                Debug.Log(_instance.gameObject.name + "_Called");
                return _instance;
            }
        }
    }

    public virtual void Awake()
    {
        if (_instance != null) {
//            Debug.Log(_instance.gameObject.name + "destr");
            Destroy(gameObject);
            return;
        }
        InitObj();
        DontDestroyOnLoad(gameObject);
    }

    public virtual void InitObj() {
        _instance = (T)FindObjectOfType(typeof(T));
        if (FindObjectsOfType(typeof(T)).Length > 1)
        {
            Debug.LogError("more than 1 intsance of " + typeof(T));
        }
    }
}
