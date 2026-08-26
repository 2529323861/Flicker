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
    [RequireComponent(typeof(PlayerController))]
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
        /// 当前获取道具卡CD
        /// </summary>
        public MutableValue nowGetPropCDMV;

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
            nowGetPropCDMV = new(getPropCD);
            nowGetPropCDMV.onUpdate = (a, b) =>
            {
                if (a==0) nowGetPropCDMV.value = 0;
                else nowGetPropCDMV.value *= b/a;
            };
            getPropCDMV.AddObserver(nowGetPropCDMV);

            modifyPipeline = GetComponent<ModifyPipeline>();
            propCardDeck = GetComponent<PropCardDeck>();
            miniEventBus = GetComponent<MiniEventBus>();
            buffContainer = GetComponent<BuffContainer>();
            effectContainer = GetComponent<EffectContainer>();

            modifyPipeline.modifierSet.getPropCD = getPropCDMV;
            modifyPipeline.modifierSet.getPropCDRate = getPropCDRateMV;
            modifyPipeline.modifierSet.propSlot = propSlotMV;
            modifyPipeline.modifierSet.nowGetPropCD = nowGetPropCDMV;
        }
        void Update()
        {
            if (!isServer) return;  // 客户端空壳：逻辑空转
            if (((CustomNetworkManager)CustomNetworkManager.singleton).IsLogicDisabled) return;  //暂停/剧情/倒数
            ModifierSet values = modifyPipeline.GetModifierSet();
            if (nowGetPropCDMV.value > 0)
            {
                if (GetNowPropCount() < values.propSlot.FinalValue){
                    //只有还有空槽位的时候，CD才会流动，防止同时使用上限+1个道具
                    float getPropCDRate = values.getPropCDRate.FinalValue;
                    nowGetPropCDMV.value -= Time.deltaTime*getPropCDRate;
                }
                if (nowGetPropCDMV.value < 0)
                {
                    nowGetPropCDMV.value = values.getPropCD.FinalValue;
                    AddProp(propCardDeck.GetProp());
                }
            }
            
            onGetPropNumber?.Invoke(new(){propSlot=(int)values.propSlot.FinalValue});
            onUpdatePropCDProgress?.Invoke(new(){propCDProgress=values.nowGetPropCD.value/values.getPropCD.FinalValue});
        }
        /// <summary>
        /// 获取当前道具容器中的道具数，超出槽位的不计
        /// </summary>
        /// <returns></returns>
        public int GetNowPropCount()
        {   
            ModifierSet values = modifyPipeline.GetModifierSet();
            int result = 0;
            for (int i = 0; i < values.propSlot.FinalValue; i++)
            {
                if (props[i] != null)
                {
                    result ++;
                }
            }
            return result;
        }
        /// <summary>
        /// 添加道具卡到当前槽位，若超出当前槽位值则不赋值
        /// </summary>
        /// <param name="basePropStrategy"></param>
        public void AddProp(BasePropStrategy basePropStrategy)
        {
            ModifierSet values = modifyPipeline.GetModifierSet();
            if (GetNowPropCount()>=values.propSlot.FinalValue) return;

            ActionContext context = new();
            miniEventBus.Publish<BeforeAddPropEvent>(new(){context=context,basePropStrategy=basePropStrategy});
            if (context.cancelled) return;

            int i=0;
            while (props[i] != null)
            {
                i++;
            }
            props[i]=basePropStrategy;
            miniEventBus.Publish<AfterAddPropEvent>(new());

            onPropAdded(new(){basePropStrategyMetaData=basePropStrategy.GetMetaData(),slotIndex=i});
        }
        public void UseProp(int propSlotIndex)
        {
            if (((CustomNetworkManager)CustomNetworkManager.singleton).IsLogicDisabled) return;  //暂停/剧情/倒数

            ModifierSet values = modifyPipeline.GetModifierSet();
            if (propSlotIndex >= values.propSlot.FinalValue) return;
            if (props[propSlotIndex]==null) return;

            ActionContext context = new();
            miniEventBus.Publish<BeforeUsePropEvent>(new(){context=context,basePropStrategy=props[propSlotIndex]});
            if (context.cancelled) return;

            BasePropStrategy basePropStrategy = props[propSlotIndex];
            props[propSlotIndex] = null;
            basePropStrategy.OnUse(buffContainer,effectContainer);

            miniEventBus.Publish<AfterUsePropEvent>(new());

            onPropUse?.Invoke(new(){propSlotIndex=propSlotIndex});
        }
    }
}