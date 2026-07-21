using System;
using System.Collections.Generic;
using UnityEngine;

namespace Flicker.Module
{
    /// <summary>
    /// buff包装类，附加持续时间属性
    /// </summary>
    public class BuffWrapper
    {
        public BaseBuffStrategy baseBuffStrategy;
        public float duration;
    }
    /// <summary>
    /// buff容器
    /// </summary>
    public class BuffContainer: MonoBehaviour
    {
        /// <summary>
        /// buff添加时的UI发布委托，用于玩家控制器注入
        /// </summary>
        public Action<BaseBuffStrategy> onBuffAdd;
        /// <summary>
        /// buff移除时的UI发布委托，用于玩家控制器注入
        /// </summary>
        public Action<BaseBuffStrategy> onBuffRemove;
        /// <summary>
        /// buff层数变更时的UI发布委托，用于玩家控制器注入
        /// </summary>
        public Action<BaseBuffStrategy> onBuffStackChange;
        List<BuffWrapper> buffs = new();
        /// <summary>
        /// 效果容器
        /// </summary>
        private EffectContainer effectContainer;
        void Awake()
        {
            effectContainer = GetComponent<EffectContainer>();
        }

        void Update()
        {
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                if (buffs[i].duration > 0)
                {
                    buffs[i].duration -= Time.deltaTime;
                    if (buffs[i].duration <= 0)
                    {
                        RemoveBuff(buffs[i].baseBuffStrategy);
                    }
                }
            }
        }
        
        /// <summary>
        /// 添加buff
        /// </summary>
        /// <param name="baseBuffStrategy">buff引用</param>
        /// <param name="duration">持续时间，任意负数表示永续</param>
        public void AddBuff(BaseBuffStrategy baseBuffStrategy, float duration)
        {
            if (baseBuffStrategy.isStackable)
            {
                int i = 0;
                while (i < buffs.Count)
                {
                    var traverse = buffs[i].baseBuffStrategy;
                    if (traverse.GetType() == baseBuffStrategy.GetType())
                    {
                        if (traverse.stacks != traverse.maxStacks)
                        {
                            traverse.stacks++;
                            buffs[i].duration = duration;
                            traverse.OnStacksChange();
                            onBuffStackChange?.Invoke(traverse);
                            break;
                        }
                        else
                        {
                            return;
                        }
                    }
                    i++;
                }
                if (i == buffs.Count)
                {
                    buffs.Add(new BuffWrapper{baseBuffStrategy=baseBuffStrategy,duration=duration});
                    baseBuffStrategy.BindEffectContainer(effectContainer);
                    baseBuffStrategy.BindBuffContainer(this);
                    baseBuffStrategy.OnAdd();
                    onBuffAdd?.Invoke(baseBuffStrategy);
                }
            }
            else
            {                
                buffs.Add(new BuffWrapper{baseBuffStrategy=baseBuffStrategy,duration=duration});
                baseBuffStrategy.BindEffectContainer(effectContainer);
                baseBuffStrategy.BindBuffContainer(this);
                baseBuffStrategy.OnAdd();
                onBuffAdd?.Invoke(baseBuffStrategy);
            }
        }
        /// <summary>
        /// 移除buff
        /// </summary>
        /// <param name="baseBuffStrategy">buff引用</param>
        public void RemoveBuff(BaseBuffStrategy baseBuffStrategy)
        {
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                if (buffs[i].baseBuffStrategy == baseBuffStrategy)
                {
                    buffs.RemoveAt(i);
                    baseBuffStrategy.OnRemove();
                    onBuffRemove?.Invoke(baseBuffStrategy);
                    break;
                }
            }
        }
        /// <summary>
        /// 清理所有buff
        /// </summary>
        public void Clear()
        {
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                buffs[i].baseBuffStrategy.OnRemove();
                buffs.RemoveAt(i);
            }
        }
    }
}