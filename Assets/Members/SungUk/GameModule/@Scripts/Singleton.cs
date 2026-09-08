using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = (T)GameObject.FindAnyObjectByType(typeof(T));

                if(_instance == null)
                {
                    GameObject obj = new GameObject();
                    T t = obj.AddComponent<T>();
                    t.name = $"@{typeof(T).Name}";

                    _instance = t;
                }

                DontDestroyOnLoad(_instance.gameObject);
            }

            return _instance;
        }
    }
}
