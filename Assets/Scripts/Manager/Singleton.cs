using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    public static T Instance;
    protected virtual void Awake()
    {
        if (Instance == null)
            Instance = this as T;
    }
}

public class Persistence<T> : Singleton<T> where T : MonoBehaviour
{
    protected override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // A valid instance already exists from a previous scene → destroy this duplicate
            UnityEngine.Object.Destroy(gameObject);
            return;
        }
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
}
