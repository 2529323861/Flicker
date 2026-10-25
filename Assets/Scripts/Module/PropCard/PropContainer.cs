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
    /// 一个已携带的道具。
    /// 状态归容器，配置归策略体：冷却不写进 BasePropStrategy，避免多实例时状态分散
    /// </summary>
    public class EquippedProp
    {
        /// <summary>
        /// 道具策略体
        /// </summary>
        public BasePropStrategy prop;
        /// <summary>
        /// 冷却时长，来自 prop.cooldown，Init 时读一次，不参与修饰
        /// </summary>
        public float cdMax;
        /// <summary>
        /// 剩余冷却比例，0 = 就绪，1 = 刚使用
        /// </summary>
        public float cdRatio;
    }

    /// <summary>
    /// 道具容器。道具固定携带、不消耗，使用后进入各自的冷却
    /// </summary>
    public class PropContainer : NetworkBehaviour
    {
        /// <summary>
        /// 固定携带数
        /// </summary>
        public const int EquippedPropCount = 3;

        /// <summary>
        /// 道具装配时的UI发布委托，供玩家控制器注入
        /// </summary>
        public Action<GetPropEvent> onPropAdded;
        /// <summary>
        /// 道具使用时的UI发布委托，供玩家控制器注入
        /// </summary>
        public Action<UsePropEvent> onPropUse;
        /// <summary>
        /// 道具冷却进度的UI发布委托，供玩家控制器注入。
        /// 每帧发布一次完整状态，UI 只做渲染
        /// </summary>
        public Action<PropStateEvent> onPropState;

        private BuffContainer buffContainer;
        private EffectContainer effectContainer;
        private ModifyPipeline modifyPipeline;
        private MiniEventBus miniEventBus;
        private GameCharacterController gameCharacterController;

        /// <summary>
        /// 道具冷却速率基准值
        /// </summary>
        public float propCDRate = 1f;
        public ModifiableValue propCDRateMV;

        /// <summary>
        /// 已携带的道具，长度恒为 EquippedPropCount，元素不为 null
        /// </summary>
        private List<EquippedProp> equippedProps = new();

        void Awake()
        {
            modifyPipeline = GetComponent<ModifyPipeline>();
            miniEventBus = GetComponent<MiniEventBus>();
            buffContainer = GetComponent<BuffContainer>();
            effectContainer = GetComponent<EffectContainer>();
            gameCharacterController = GetComponent<GameCharacterController>();

            /** 必须显式注册：惰性补值会得到基值0，冷却将永不恢复 */
            propCDRateMV = new(propCDRate);
            modifyPipeline.modifierSet.Register(ModifierSet.Key.PropCDRate, propCDRateMV);
        }

        /// <summary>
        /// 注入携带道具，开局由服务端调用一次
        /// </summary>
        /// <param name="propIds">道具工厂 id，数量必须等于 EquippedPropCount</param>
        public void Init(List<string> propIds)
        {
            if (propIds == null || propIds.Count != EquippedPropCount)
                throw new ArgumentException($"携带道具数必须为 {EquippedPropCount}，实际为 {propIds?.Count}");

            equippedProps.Clear();
            for (int i = 0; i < EquippedPropCount; i++)
            {
                BasePropStrategy prop = PropFactory.Instance.GetProp(propIds[i]);
                prop.BindGameCharacterController(gameCharacterController);
                prop.BindBuffContainer(buffContainer);
                prop.BindEffectContainer(effectContainer);
                prop.BindPropContainer(this);

                equippedProps.Add(new EquippedProp
                {
                    prop = prop,
                    cdMax = prop.cooldown,
                    cdRatio = Ratio.Empty
                });

                onPropAdded?.Invoke(new(){basePropStrategyMetaData = prop.GetMetaData(), slotIndex = i});
            }
        }

        void Update()
        {
            if (!isServer) return;  // 客户端空壳：逻辑空转
            if (((CustomNetworkManager)CustomNetworkManager.singleton).IsLogicDisabled) return;  //暂停/剧情/倒数

            /** 冷却速率每帧只取一次。GetModifierSet 每次调用都会清空并重算全部 Effect */
            float propCDRate = modifyPipeline.GetModifierSet().Final(ModifierSet.Key.PropCDRate);

            for (int i = 0; i < equippedProps.Count; i++)
            {
                EquippedProp equippedProp = equippedProps[i];
                if (equippedProp.cdRatio > Ratio.Empty)
                {
                    equippedProp.cdRatio = Ratio.Advance(equippedProp.cdRatio, equippedProp.cdMax, -Time.deltaTime * propCDRate);
                }
            }

            /** 每帧发布完整状态，UI 不需要自己拼凑增量 */
            onPropState?.Invoke(new()
            {
                propCD0 = GetCDRatio(0),
                propCD1 = GetCDRatio(1),
                propCD2 = GetCDRatio(2)
            });
        }

        /// <summary>
        /// 清空全部道具冷却，供道具效果调用
        /// </summary>
        public void ClearAllCD()
        {
            foreach (var i in equippedProps)
            {
                i.cdRatio = Ratio.Empty;
            }
        }

        /// <summary>
        /// 读取指定位置的道具，供只读查询
        /// </summary>
        public BasePropStrategy GetProp(int index)
        {
            if (index < 0 || index >= equippedProps.Count) return null;
            return equippedProps[index].prop;
        }

        /// <summary>
        /// 读取指定位置的冷却比例，供HUD快照使用
        /// </summary>
        public float GetCDRatio(int index)
        {
            if (index < 0 || index >= equippedProps.Count) return Ratio.Empty;
            return equippedProps[index].cdRatio;
        }

        public void UseProp(int propSlotIndex)
        {
            if (propSlotIndex < 0 || propSlotIndex >= equippedProps.Count) return;
            if (((CustomNetworkManager)CustomNetworkManager.singleton).IsLogicDisabled) return;  //暂停/剧情/倒数

            EquippedProp equippedProp = equippedProps[propSlotIndex];

            /** 冷却中就不可用。与 CanUse 同为前置条件，不消耗也不发布任何事件 */
            if (equippedProp.cdRatio > Ratio.Empty) return;

            /** 道具自身的先决条件，不满足则不进入冷却也不触发任何钩子 */
            if (!equippedProp.prop.CanUse()) return;

            ModifierSet values = modifyPipeline.GetModifierSet();
            UsePropContext context = new()
            {
                attributes = values,
                prop = equippedProp.prop
            };

            /** 相位一：决定本次使用道具是否发生 */
            miniEventBus.Publish<BeforeUsePropEvent>(new(){context=context});
            if (context.cancelled) return;

            /** 相位二：确定本次使用道具的临时变量 */
            miniEventBus.Publish<UsePropResultEvent>(new(){context=context});

            /** 相位三：按结果执行。道具不消耗，只进入冷却 */
            context.prop.OnUse();
            equippedProp.cdRatio = Ratio.Full;

            miniEventBus.Publish<AfterUsePropEvent>(new(){context=context});

            onPropUse?.Invoke(new(){propSlotIndex=propSlotIndex});
        }
    }
}