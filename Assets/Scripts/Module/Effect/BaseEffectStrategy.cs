using Flicker.Core;
using UnityEngine;

namespace Flicker.Module
{
    /// <summary>
    /// 效果策略基类，供修饰管线直接遍历
    /// </summary>
    public class BaseEffectStrategy
    {
        /// <summary>
        /// 效果的对象
        /// </summary>
        protected GameObject gameObject;
        /// <summary>
        /// 来源buff
        /// </summary>
        protected BaseBuffStrategy baseBuffStrategy;
        /// <summary>
        /// 持续性修饰，每次读取数据都会应用
        /// </summary>
        /// <param name="ModifierSet">迭代数据集</param>
        public virtual void Apply(ModifierSet ModifierSet){}
        /// <summary>
        /// 注册，当效果被加至效果列表时被调用，用于绑定事件
        /// </summary>
        /// <param name="miniEventBus">迷你事件总线</param>
        public virtual void Register(MiniEventBus miniEventBus){}
        /// <summary>
        /// 取消注册，当效果被移除效果列表时调用，用于解绑事件
        /// </summary>
        /// <param name="miniEventBus">迷你事件总线</param>
        public virtual void Unregister(MiniEventBus miniEventBus){}

        public void BindGameObject(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }
        public void BindBaseBuffStrategy(BaseBuffStrategy baseBuffStrategy)
        {
            this.baseBuffStrategy = baseBuffStrategy;
        }
    }
}