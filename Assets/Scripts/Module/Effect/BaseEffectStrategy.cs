using System;
using System.Collections.Generic;
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
        /// 效果容器的绑定，供效果自移除
        /// </summary>
        public EffectContainer effectContainer;
        /// <summary>
        /// 游戏角色的事件总线
        /// </summary>
        public MiniEventBus miniEventBus;
        public  Action<BaseEffectStrategy,ModifierSet> apply;
        public  Action<BaseEffectStrategy> onAdd;
        public  Action<BaseEffectStrategy> onRemove;
        /// <summary>
        /// 经 Listen 注册的订阅退订动作，供 UnlistenAll 统一清理
        /// </summary>
        readonly List<Action> listeners = new();
        /// <summary>
        /// 持续性修饰，每次读取数据都会应用
        /// </summary>
        /// <param name="modifierSet">迭代数据集</param>
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
        /// <summary>
        /// 订阅事件，移除效果时自动退订
        /// </summary>
        /// <param name="handler">事件处理</param>
        public void Listen<T>(Action<T> handler) where T : struct
        {
            miniEventBus.Subscribe(handler);
            listeners.Add(() => miniEventBus.Unsubscribe(handler));
        }
        /// <summary>
        /// 退订所有经 Listen 注册的事件，由效果容器在移除时调用
        /// </summary>
        public void UnlistenAll()
        {
            foreach (var unsubscribe in listeners)
            {
                unsubscribe();
            }
            listeners.Clear();
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
        public void BindEffectContainer(EffectContainer effectContainer){
            this.effectContainer = effectContainer;
        }
    }
}