using System;
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
        public GameObject gameObject;
        /// <summary>
        /// 来源buff
        /// </summary>
        public BaseBuffStrategy baseBuffStrategy;
        /// <summary>
        /// buff容器的绑定
        /// </summary>
        public BuffContainer buffContainer;
        /// <summary>
        /// 游戏角色的事件总线
        /// </summary>
        public MiniEventBus miniEventBus;
        public  Action<BaseEffectStrategy,ModifierSet> apply;
        public  Action<BaseEffectStrategy> onAdd;
        public  Action<BaseEffectStrategy> onRemove;
        /// <summary>
        /// 持续性修饰，每次读取数据都会应用
        /// </summary>
        /// <param name="ModifierSet">迭代数据集</param>
        public virtual void Apply(ModifierSet modifierSet)
        {
            apply?.Invoke(this,modifierSet);
        }
        /// <summary>
        /// 添加钩子，效果添加后被触发
        /// </summary>
        public virtual void OnAdd()
        {
            onAdd?.Invoke(this);
        }
        /// <summary>
        /// 移除钩子，效果移除后被触发
        /// </summary>
        public virtual void OnRemove()
        {
            onRemove?.Invoke(this);
        }

        public void BindGameObject(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }
        public void BindBaseBuffStrategy(BaseBuffStrategy baseBuffStrategy)
        {
            this.baseBuffStrategy = baseBuffStrategy;
        }
        public void BindMiniEventBus(MiniEventBus miniEventBus){
            this.miniEventBus = miniEventBus;
        }
        public void BindBuffContainer(BuffContainer buffContainer){
            this.buffContainer = buffContainer;
        }
    }
}