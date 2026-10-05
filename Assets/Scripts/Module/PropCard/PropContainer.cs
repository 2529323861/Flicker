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
    [RequireComponent(typeof(PropCardDeck))]
    /// <summary>
    /// 道具卡容器，用于管理道具卡的生成，使用
    /// </summary>
    public class PropContainer : NetworkBehaviour
    {
        /// <summary>
        /// 道具添加时的UI发布委托，供玩家控制器注入
        /// </summary>
        public Action<GetPropEvent> onPropAdded;
        /// <summary>
        /// 道具使用时的UI发布委托，供玩家控制器注入
        /// </summary>
        public Action<UsePropEvent> onPropUse;
        /// <summary>
        /// 获取当前道具槽数的UI发布委托，供玩家控制器注入
        /// </summary>
        public Action<GetPropNumberEvent> onGetPropNumber;
        /// <summary>
        /// 获取当前UICD进度比的UI发布委托，供玩家控制器注入
        /// </summary>
        public Action<UpdatePropCDProgressEvent> onUpdatePropCDProgress;

        private BuffContainer buffContainer;
        private EffectContainer effectContainer;
        private ModifyPipeline modifyPipeline;
        private PropCardDeck propCardDeck;
        private MiniEventBus miniEventBus;
        private GameCharacterController gameCharacterController;
        /// <summary>
        /// 道具卡槽位数
        /// </summary>
        public float propSlot;
        public ModifiableValue propSlotMV;
        /// <summary>
        /// 获取道具卡时间
        /// </summary>
        public float getPropCD;
        public ModifiableValue getPropCDMV;

        /// <summary>
        /// 获取道具卡冷却速率
        /// </summary>
        public float getPropCDRate;
        public ModifiableValue getPropCDRateMV;

        /// <summary>
        /// 当前获取道具卡冷却比例，0 = 就绪，1 = 刚触发过
        /// </summary>
        public float propCDRatio;

        public List<BasePropStrategy> props;

        public void GenerateDataPool()
        {
            propSlotMV = new(propSlot);
            getPropCDMV = new(getPropCD);
            getPropCDRateMV = new(getPropCDRate);
        }
        void Awake()
        {
            props= new();
            for (int i = 0; i < 5; i++)
            {
                props.Add(null);
            }
            GenerateDataPool();

            /** 冷却比例以"上限"为不变量，上限变化时比例不变，无需观察者做等比缩放。
             *  初始为满冷却；上限为 0 时视为无冷却，与旧逻辑一致 */
            propCDRatio = Ratio.ToRatio(getPropCD, getPropCD);

            modifyPipeline = GetComponent<ModifyPipeline>();
            propCardDeck = GetComponent<PropCardDeck>();
            miniEventBus = GetComponent<MiniEventBus>();
            buffContainer = GetComponent<BuffContainer>();
            effectContainer = GetComponent<EffectContainer>();
            gameCharacterController = GetComponent<GameCharacterController>();

            modifyPipeline.modifierSet.Register(ModifierSet.Key.GetPropCD, getPropCDMV);
            modifyPipeline.modifierSet.Register(ModifierSet.Key.GetPropCDRate, getPropCDRateMV);
            modifyPipeline.modifierSet.Register(ModifierSet.Key.PropSlot, propSlotMV);
        }
        void Update()
        {
            if (!isServer) return;  // 客户端空壳：逻辑空转
            if (((CustomNetworkManager)CustomNetworkManager.singleton).IsLogicDisabled) return;  //暂停/剧情/倒数
            ModifierSet values = modifyPipeline.GetModifierSet();
            if (propCDRatio > 0)
            {
                float getPropCD = values.Final(ModifierSet.Key.GetPropCD);
                if (GetNowPropCount() < values.Final(ModifierSet.Key.PropSlot)){
                    //只有还有空槽位的时候，CD才会流动，防止同时使用上限+1个道具
                    float getPropCDRate = values.Final(ModifierSet.Key.GetPropCDRate);
                    propCDRatio = Ratio.Advance(propCDRatio, getPropCD, -Time.deltaTime*getPropCDRate);
                }
                if (propCDRatio <= 0)
                {
                    propCDRatio = Ratio.ToRatio(getPropCD, getPropCD);
                    AddProp(propCardDeck.GetProp());
                }
            }
            
            onGetPropNumber?.Invoke(new(){propSlot=(int)values.Final(ModifierSet.Key.PropSlot)});
            onUpdatePropCDProgress?.Invoke(new(){propCDProgress=propCDRatio});
        }
        /// <summary>
        /// 获取当前道具容器中的道具数，超出槽位的不计
        /// </summary>
        /// <returns></returns>
        public int GetNowPropCount()
        {   
            float propSlot = modifyPipeline.GetModifierSet().Final(ModifierSet.Key.PropSlot);
            int result = 0;
            for (int i = 0; i < propSlot; i++)
            {
                if (props[i] != null)
                {
                    result ++;
                }
            }
            return result;
        }
        public void AddProp()
        {
            AddProp(propCardDeck.GetProp());
        }
        /// <summary>
        /// 添加道具卡到当前槽位，若超出当前槽位值则不赋值
        /// </summary>
        /// <param name="basePropStrategy"></param>
        public void AddProp(BasePropStrategy basePropStrategy)
        {
            ModifierSet values = modifyPipeline.GetModifierSet();
            if (GetNowPropCount()>=values.Final(ModifierSet.Key.PropSlot)) return;

            AddPropContext context = new()
            {
                attributes = values,
                prop = basePropStrategy
            };

            /** 相位一：决定本次获得道具是否发生 */
            miniEventBus.Publish<BeforeAddPropEvent>(new(){context=context});
            if (context.cancelled) return;

            /** 相位二：确定本次获得道具的临时变量 */
            miniEventBus.Publish<AddPropResultEvent>(new(){context=context});

            /** 相位三：按结果执行 */
            int i=0;
            while (props[i] != null)
            {
                i++;
            }
            context.prop.BindGameCharacterController(gameCharacterController);
            context.prop.BindBuffContainer(buffContainer);
            context.prop.BindEffectContainer(effectContainer);
            props[i]=context.prop;
            miniEventBus.Publish<AfterAddPropEvent>(new(){context=context});

            onPropAdded?.Invoke(new(){basePropStrategyMetaData=context.prop.GetMetaData(),slotIndex=i});
        }
        public void UseProp(int propSlotIndex)
        {
            if (((CustomNetworkManager)CustomNetworkManager.singleton).IsLogicDisabled) return;  //暂停/剧情/倒数

            ModifierSet values = modifyPipeline.GetModifierSet();
            if (propSlotIndex >= values.Final(ModifierSet.Key.PropSlot)) return;
            if (props[propSlotIndex]==null) return;

            /** 道具自身的先决条件，不满足则不消耗也不触发任何钩子 */
            if (!props[propSlotIndex].CanUse()) return;

            UsePropContext context = new()
            {
                attributes = values,
                prop = props[propSlotIndex]
            };

            /** 相位一：决定本次使用道具是否发生 */
            miniEventBus.Publish<BeforeUsePropEvent>(new(){context=context});
            if (context.cancelled) return;

            /** 相位二：确定本次使用道具的临时变量 */
            miniEventBus.Publish<UsePropResultEvent>(new(){context=context});

            /** 相位三：按结果执行 */
            props[propSlotIndex] = null;
            context.prop.OnUse();

            miniEventBus.Publish<AfterUsePropEvent>(new(){context=context});

            onPropUse?.Invoke(new(){propSlotIndex=propSlotIndex});
        }
    }
}