using UnityEngine;

namespace Framework
{
    /// <summary>
    /// Unity 单例基类：场景里手摆一份时，重复实例会被销毁。
    /// 派生类在 Awake 里必须判断 <see cref="IsDuplicateInstance"/> 并立刻 return。
    /// </summary>
    public class UnitySingleTonMono<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var obj = new GameObject(typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                    DontDestroyOnLoad(obj);
                }

                return _instance;
            }
        }

        protected bool IsDuplicateInstance { get; private set; }

        protected virtual void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (_instance == null)
            {
                _instance = this as T;
                name = typeof(T).Name;
            }
            else
            {
                IsDuplicateInstance = true;
                DestroyImmediate(gameObject);
            }
        }
    }
}
