using UnityEngine;

namespace Flicker.Core
{
    /// <summary>
    /// 泛型 MonoBehaviour 单例基类。
    /// 使用方式：public class GameManager : Singleton<GameManager> { }
    /// 特性：场景切换不销毁、自动创建、线程安全。
    /// </summary>
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _isQuitting = false;

        public static T Instance
        {
            get
            {
                if (_isQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance of {typeof(T)} already destroyed on application quit.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();

                        if (_instance == null)
                        {
                            var go = new GameObject($"[{typeof(T).Name}]");
                            _instance = go.AddComponent<T>();
                            DontDestroyOnLoad(go);
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
                OnSingletonInit();
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[Singleton] Duplicate {typeof(T).Name} destroyed on {gameObject.name}.");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 单例首次初始化时调用（替代 Awake）。
        /// </summary>
        protected virtual void OnSingletonInit() { }

        protected virtual void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
