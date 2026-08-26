using System;
using System.Collections.Generic;
using Flicker.Core;
using Flicker.Module.CharacterEvent;
using Flicker.Module.Context;
using Flicker.Module.Network;
using Flicker.Module.UIEvent;
using Mirror;
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
    public class BuffContainer: NetworkBehaviour
    {
        MiniEventBus miniEventBus;
        /// <summary>
        /// buff添加时的UI发布委托，用于玩家控制器注入
        /// </summary>
        public Action<AddNewBuffEvent> onBuffAdd;
        /// <summary>
        /// buff移除时的UI发布委托，用于玩家控制器注入
        /// </summary>
        public Action<RemoveBuffEvent> onBuffRemove;
        /// <summary>
        /// buff层数变更时的UI发布委托，用于玩家控制器注入
        /// </summary>
        public Action<BuffStackChangeEvent> onBuffStackChange;
        List<BuffWrapper> buffs = new();
        /// <summary>
        /// 效果容器
        /// </summary>
        private EffectContainer effectContainer;
        void Awake()
        {
            effectContainer = GetComponent<EffectContainer>();
            miniEventBus = GetComponent<MiniEventBus>();
        }

        void Update()
        {
            if (!isServer) return;  // 客户端空壳：逻辑空转
            if (((CustomNetworkManager)CustomNetworkManager.singleton).IsLogicDisabled) return;  //暂停/剧情/倒数
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
                            ActionContext actionContext = new();
                            miniEventBus.Publish<BeforeBuffStackEvent>(new BeforeBuffStackEvent(){context=actionContext,baseBuffStrategy=baseBuffStrategy});
                            if (actionContext.cancelled) return;

                            buffs[i].duration = duration;
                            traverse.stacks++;
                            traverse.OnStacksChange();
                            miniEventBus.Publish<AfterBuffStackEvent>(new());
                            var sbs = traverse;
                            var evt = new BuffStackChangeEvent();
                            evt.baseBuffStrategyMetaData = sbs.GetMetaData();

                            onBuffStackChange?.Invoke(evt);
                            break;
                        }
                        else
                        {
                            buffs[i].duration = duration;
                            return;
                        }
                    }
                    i++;
                }
                if (i == buffs.Count)
                {
                    AddNewBuff(baseBuffStrategy,duration);
                }
            }
            else
            {          
                AddNewBuff(baseBuffStrategy,duration);
            }
        }
        /// <summary>
        /// 添加新buff的封装
        /// </summary>
        /// <param name="baseBuffStrategy"></param>
        private void AddNewBuff(BaseBuffStrategy baseBuffStrategy, float duration)
        {
            ActionContext actionContext = new();
            miniEventBus.Publish<BeforeAddBuffEvent>(new BeforeAddBuffEvent(){context=actionContext,baseBuffStrategy=baseBuffStrategy});
            if (actionContext.cancelled) return;

            buffs.Add(new BuffWrapper{baseBuffStrategy=baseBuffStrategy,duration=duration});
            baseBuffStrategy.BindEffectContainer(effectContainer);
            baseBuffStrategy.BindBuffContainer(this);
            baseBuffStrategy.OnAdd();
            miniEventBus.Publish<AfterAddBuffEvent>(new());

            var sbs = baseBuffStrategy;
            var evt = new AddNewBuffEvent();
            evt.baseBuffStrategyMetaData = sbs.GetMetaData();

            onBuffAdd?.Invoke(evt);
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
                    ActionContext actionContext = new();
                    miniEventBus.Publish<BeforeRemoveBuffEvent>(new(){context=actionContext,baseBuffStrategy=baseBuffStrategy});
                    if (actionContext.cancelled) return;

                    buffs.RemoveAt(i);
                    baseBuffStrategy.OnRemove();
                    miniEventBus.Publish<AfterRemoveBuffEvent>(new());

                    var sbs = baseBuffStrategy;
                    var evt = new RemoveBuffEvent();
                    evt.baseBuffStrategyMetaData = sbs.GetMetaData();

                    onBuffRemove?.Invoke(evt);
                    break;
                }
            }
        }
        /// <summary>
        /// 清除所有buff
        /// </summary>
        public void ClearBuff()
        {
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                ActionContext actionContext = new();
                miniEventBus.Publish<BeforeRemoveBuffEvent>(new(){context=actionContext,baseBuffStrategy=buffs[i].baseBuffStrategy});
                if (actionContext.cancelled) continue;

                buffs[i].baseBuffStrategy.OnRemove();

                miniEventBus.Publish<AfterRemoveBuffEvent>(new());
                buffs.RemoveAt(i);
            }
        }
        /// <summary>
        /// 强制清理所有buff
        /// </summary>
        public void ForeClearBuff()
        {
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                buffs[i].baseBuffStrategy.OnRemove();
                buffs.RemoveAt(i);
            }
        }
    }
}