using System.Collections.Generic;
using Flicker.Core;
using Flicker.Module.CharacterEvent;
using Flicker.Module.UIEvent;
using UnityEngine;

namespace Flicker.Module
{
    [RequireComponent(typeof(PlayerController))]
    [RequireComponent(typeof(PropCardDeck))]
    /// <summary>
    /// 道具卡容器，用于管理道具卡的生成，使用
    /// </summary>
    public class PropContainer : MonoBehaviour
    {
        private ModifyPipeline modifyPipeline;
        private PropCardDeck propCardDeck;
        private MiniEventBus miniEventBus;
        /// <summary>
        /// 道具卡槽位数
        /// </summary>
        public float propSlot = 3f;
        public ModifiableValue propSlotMV;
        /// <summary>
        /// 获取道具卡时间
        /// </summary>
        public float getPropCD = 3f;
        public ModifiableValue getPropCDMV;

        /// <summary>
        /// 获取道具卡冷却速率
        /// </summary>
        public float getPropCDRate = 1f;
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

            modifyPipeline = GetComponent<ModifyPipeline>();
            propCardDeck = GetComponent<PropCardDeck>();
            miniEventBus = GetComponent<MiniEventBus>();

        }
        void Update()
        {
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

            miniEventBus.Publish<GetPropNumberEvent>(new GetPropNumberEvent{propSlot = (int)values.propSlot.FinalValue});
            miniEventBus.Publish<GetPropCDProgressEvent>(new GetPropCDProgressEvent{propCDProgress=values.nowGetPropCD.value/values.getPropCD.FinalValue});
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
            int i=0;
            while (props[i] != null)
            {
                i++;
            }
            props[i]=basePropStrategy;
            miniEventBus.Publish<GetPropEvent>(new GetPropEvent
            {
                basePropStrategy = basePropStrategy,
                slotIndex = i,
            });
        }
    }
}