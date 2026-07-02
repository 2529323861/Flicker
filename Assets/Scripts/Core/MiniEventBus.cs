using System;
using System.Collections.Generic;
using UnityEngine;

namespace Flicker.Core
{
    /// <summary>
    /// 非静态事件总线，用于在游戏对象内实例化自治，去中心化
    /// </summary>
    public class MiniEventBus: MonoBehaviour
    {
        private readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();

        /// <summary>
        /// 订阅事件。
        /// </summary>
        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
            {
                _handlers[type] = Delegate.Combine(existing, handler);
            }
            else
            {
                _handlers[type] = handler;
            }
        }

        /// <summary>
        /// 取消订阅事件。
        /// </summary>
        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
            {
                var result = Delegate.Remove(existing, handler);
                if (result == null)
                    _handlers.Remove(type);
                else
                    _handlers[type] = result;
            }
        }

        /// <summary>
        /// 发布事件，通知所有订阅者。
        /// </summary>
        public void Publish<T>(T evt) where T : struct
        {
            var type = typeof(T);
            if (_handlers.TryGetValue(type, out var existing))
            {
                try
                {
                    (existing as Action<T>)?.Invoke(evt);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventBus] Error handling event {type.Name}: {e}");
                }
            }
        }

        /// <summary>
        /// 清除所有事件订阅（场景切换或重置时调用）。
        /// </summary>
        public void Clear()
        {
            _handlers.Clear();
        }

        /// <summary>
        /// 清除指定类型的所有订阅。
        /// </summary>
        public void Clear<T>() where T : struct
        {
            _handlers.Remove(typeof(T));
        }
    }
}
