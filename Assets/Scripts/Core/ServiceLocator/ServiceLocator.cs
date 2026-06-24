
using System;
using System.Collections.Generic;
using UnityEngine;


namespace Flicker.Core.Service
{
    public static class ServiceLocator
    {
        private static Dictionary<Type,object> services = new Dictionary<Type, object>();

        public static void RegisterService<T>(object service) where T : class
        {   
            if (services.ContainsKey(typeof(T)))
            {
                Debug.Log("[warning] 重复的服务注册！");
            }
            if (!services.TryAdd(typeof(T), service))
            {
                Debug.Log($"[Error] {typeof(T).ToString()} 注册失败！");
            }
        }
        public static T GetService<T>() where T : class
        {   
            if (services.TryGetValue(typeof(T),out var result))
            {
                return (T)result;
            }
            Debug.Log($"[Error] 不存在的服务获取：{typeof(T)}");
            return default(T);
        }
    }
}