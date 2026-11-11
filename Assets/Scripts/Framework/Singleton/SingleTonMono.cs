using UnityEngine;

namespace Framework
{
    /// <summary>
    /// 简单 MonoBehaviour 单例（全局唯一，跨场景常驻）。
    /// </summary>
    public class SingleTonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static readonly object Locker = new object();
        private static bool _applicationIsQuitting;

        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance of {typeof(T)} already destroyed on quit.");
                    return null;
                }

                lock (Locker)
                {
                    if (_instance == null)
                    {
                        _instance = FindAnyObjectByType<T>();
                        if (_instance == null)
                        {
                            var obj = new GameObject(typeof(T).Name);
                            _instance = obj.AddComponent<T>();
                            DontDestroyOnLoad(obj);
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}
