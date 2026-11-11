using UnityEngine;

/*
 * MonoSingletonT 版本: V1.0.0，设计思路：
 * 1. 全局只实例化一个对象，方便管理
 * 2. 使用饿汉式实现，调用时才进行实例化
 * 3. 使用泛型实现单例，使其通用
 * 4. 实现DontDestroyOnLoad或场景直接Awake两种方式的单例
 */
namespace IMGUIDebugger
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;

                GameObject gameObject = new GameObject(typeof(T).Name);

                DontDestroyOnLoad(gameObject);

                T temT = gameObject.AddComponent<T>();

                _instance = temT;

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
            else
            {
                Debug.LogError(
                    $"typeName:{typeof(T).Name} is Already Exist");
            }
        }

        protected virtual void Destroy()
        {
            _instance = null;
        }

        public static void Dispose()
        {
            UnityEngine.Object.Destroy(_instance.gameObject);
        }
    }
}