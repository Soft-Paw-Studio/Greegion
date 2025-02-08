using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
{
    private static T instance;
    private static readonly object Lock = new object();
    private static bool isQuitting;

    public static T Instance
    {
        get
        {
            if (isQuitting)
            {
                Debug.LogWarning($"[Singleton] Instance of {typeof(T)} already destroyed due to application quitting.");
                return null;
            }

            lock (Lock)
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>();

                    if (instance == null)
                    {
                        var go = new GameObject($"[Singleton] {typeof(T)}");
                        instance = go.AddComponent<T>();
                    }
                }
                return instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = (T)this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Debug.LogWarning($"[Singleton] Multiple instances of {typeof(T)} detected. Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    public virtual void OnApplicationQuit()
    {
        isQuitting = true;
    }
}