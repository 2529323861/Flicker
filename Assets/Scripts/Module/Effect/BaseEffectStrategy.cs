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
        /// buff容器的绑定
        /// </summary>
        protected BuffContainer buffContainer;
        /// <summary>
        /// 游戏角色的事件总线
        /// </summary>
        protected MiniEventBus miniEventBus;
        /// <summary>
        /// 持续性修饰，每次读取数据都会应用
        /// </summary>
        /// <param name="ModifierSet">迭代数据集</param>
        public virtual void Apply(ModifierSet ModifierSet){}
        /// <summary>
        /// 添加钩子，效果添加后被触发
        /// </summary>
        public virtual void OnAdd(){}
        /// <summary>
        /// 移除钩子，效果移除后被触发
        /// </summary>
        public virtual void OnRemove(){}

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