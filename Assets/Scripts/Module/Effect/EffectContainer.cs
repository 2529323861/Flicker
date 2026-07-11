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
        /// <summary>
        /// 游戏角色的迷你事件总线
        /// </summary>
        private MiniEventBus miniEventBus;
        /// <summary>
        /// 游戏角色的buff容器
        /// </summary>
        private BuffContainer buffContainer;
        private List<BaseEffectStrategy> effects = new();
        void Awake()
        {
            miniEventBus = GetComponent<MiniEventBus>();
            buffContainer = GetComponent<BuffContainer>();
        }
        public List<BaseEffectStrategy> GetEffectStrategies()
        {
            List<BaseEffectStrategy> result = new();
            foreach (var i in effects)
            {
                result.Add(i);
            }
            return result;
        }
        public void AddEffect(BaseEffectStrategy baseEffectStrategy, BaseBuffStrategy sourceBuff=null)
        {
            effects.Add(baseEffectStrategy);
            baseEffectStrategy.BindGameObject(gameObject);
            baseEffectStrategy.BindBaseBuffStrategy(sourceBuff);
            baseEffectStrategy.BindMiniEventBus(miniEventBus);
            baseEffectStrategy.BindBuffContainer(buffContainer);
            baseEffectStrategy.OnAdd();
        }
        public void RemoveEffect(BaseEffectStrategy baseEffectStrategy)
        {
            effects.Remove(baseEffectStrategy);
            baseEffectStrategy.OnRemove();
        }
    }
}