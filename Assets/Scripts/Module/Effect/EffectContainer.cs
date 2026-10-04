using System;
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
        /// <summary>
        /// 遍历期间被移除的效果，遍历结束后统一清理
        /// </summary>
        private List<BaseEffectStrategy> pendingRemove = new();
        /// <summary>
        /// 当前是否处于遍历中
        /// </summary>
        private bool iterating = false;

        void Awake()
        {
            miniEventBus = GetComponent<MiniEventBus>();
            buffContainer = GetComponent<BuffContainer>();
        }

        /// <summary>
        /// 遍历所有效果。
        /// 遍历期间移除的效果延迟到遍历结束后生效，避免改动正在遍历的列表
        /// </summary>
        /// <param name="action">对每个效果执行的操作</param>
        public void ForEachEffect(Action<BaseEffectStrategy> action)
        {
            iterating = true;
            for (int i = 0; i < effects.Count; i++)
            {
                action(effects[i]);
            }
            iterating = false;

            if (pendingRemove.Count == 0) return;
            for (int i = 0; i < pendingRemove.Count; i++)
            {
                RemoveEffectNow(pendingRemove[i]);
            }
            pendingRemove.Clear();
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
            /** 遍历中移除会改动列表，延迟到遍历结束后执行 */
            if (iterating)
            {
                if (!pendingRemove.Contains(baseEffectStrategy)) pendingRemove.Add(baseEffectStrategy);
                return;
            }
            RemoveEffectNow(baseEffectStrategy);
        }
        private void RemoveEffectNow(BaseEffectStrategy baseEffectStrategy)
        {
            effects.Remove(baseEffectStrategy);
            baseEffectStrategy.OnRemove();
        }
    }
}