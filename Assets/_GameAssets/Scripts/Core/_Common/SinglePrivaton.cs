using UnityEngine;

public class SinglePrivaton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T Instance
    {
        get
        {
            if (_instance is null)
            {
                _instance = FindObjectOfType<T>();
            }

            if (_instance is null)
            {
                var obj = new GameObject(typeof(T).Name, typeof(DontDestroyOnLoad));
                _instance = obj.GetOrAddComponent<T>();
            }
            return _instance;
        }
    }
}