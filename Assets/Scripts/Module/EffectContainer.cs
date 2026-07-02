using System.Collections.Generic;
using Flicker.Core;
using UnityEngine;

namespace Flicker.Module
{
    /// <summary>
    /// 挂载于游戏对象上，作为效果容器，供修饰管线调用
    /// </summary>
    public class EffectContainer: MonoBehaviour
    {
        private List<BaseEffectStrategy> effects = new();
        public List<BaseEffectStrategy> GetEffectStrategies()
        {
            List<BaseEffectStrategy> result = new();
            foreach (var i in effects)
            {
                result.Add(i);
            }
            return result;
        }
        public void AddEffect(BaseEffectStrategy baseEffectStrategy)
        {
            effects.Add(baseEffectStrategy);
            baseEffectStrategy.BindGameObject(gameObject);
            baseEffectStrategy.Register(gameObject.GetComponent<MiniEventBus>());
        }
        public void RemoveEffect(BaseEffectStrategy baseEffectStrategy)
        {
            effects.Remove(baseEffectStrategy);
            baseEffectStrategy.Unregister(gameObject.GetComponent<MiniEventBus>());
        }
    }
}