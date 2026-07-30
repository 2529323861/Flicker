using System;
using System.Collections.Generic;
using UnityEngine;

namespace Flicker.Core
{
    /// <summary>
    /// 基于泛型的类型安全事件总线。
    /// 模块间解耦通信核心，支持订阅/发布/取消订阅。
    /// 
    /// 使用方式：
    ///   EventBus.Subscribe&lt;SaveTriggerEvent&gt;(OnSaveTrigger);
    ///   EventBus.Publish(new SaveTriggerEvent());
    ///   EventBus.Unsubscribe&lt;SaveTriggerEvent&gt;(OnSaveTrigger);
    /// </summary>
    public static class EventBus
    {
        // 泛型订阅存储（泛型 Publish<T> 走快路径，零反射）
        private static readonly Dictionary<Type, Delegate> _handlers = new();
        // object 版本缓存（非泛型 Publish(Type,object) 走快路径，零反射）
        private static readonly Dictionary<Type, Action<object>> _objectHandlers = new();

        /// <summary>
        /// 订阅事件。
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);

            // 泛型入口（Publish<T> 用）
            if (_handlers.TryGetValue(type, out var existing))
                _handlers[type] = Delegate.Combine(existing, handler);
            else
                _handlers[type] = handler;

            // object 入口（Publish(Type,object) 用，Subscribe 时预包装，零反射）
            Action<object> boxed = (object evt) => handler((T)evt);
            if (_objectHandlers.TryGetValue(type, out var existingObj))
                _objectHandlers[type] = (Action<object>)Delegate.Combine(existingObj, boxed);
            else
                _objectHandlers[type] = boxed;
        }

        /// <summary>
        /// 取消订阅事件。
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            var type = typeof(T);

            // 泛型入口
            if (_handlers.TryGetValue(type, out var existing))
            {
                var result = Delegate.Remove(existing, handler);
                if (result == null) _handlers.Remove(type);
                else _handlers[type] = result;
            }

            // object 入口：从剩余 handlers 重建
            if (_handlers.TryGetValue(type, out var remaining) && remaining != null)
            {
                var list = remaining.GetInvocationList();
                Action<object> rebuilt = null;
                foreach (var d in list)
                {
                    var h = (Action<T>)d;
                    rebuilt += (object evt) => h((T)evt);
                }
                _objectHandlers[type] = rebuilt;
            }
            else
            {
                _objectHandlers.Remove(type);
            }
        }

        /// <summary>
        /// 发布事件，通知所有订阅者。
        /// </summary>
        public static void Publish<T>(T evt) where T : struct
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
        /// 运行时类型发布，用于反序列化后动态分发（零反射）。
        /// </summary>
        public static void Publish(Type type, object evt)
        {
            if (_objectHandlers.TryGetValue(type, out var handler))
            {
                try
                {
                    handler(evt);
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
        public static void Clear()
        {
            _handlers.Clear();
            _objectHandlers.Clear();
        }

        /// <summary>
        /// 清除指定类型的所有订阅。
        /// </summary>
        public static void Clear<T>() where T : struct
        {
            var type = typeof(T);
            _handlers.Remove(type);
            _objectHandlers.Remove(type);
        }
    }
}
