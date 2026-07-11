using System.Collections.Generic;
using UnityEngine;

namespace Flicker.Module
{
    /// <summary>
    /// 修饰管线
    /// 用于处理buff/藏品造成的效果
    /// </summary>
    public class ModifyPipeline: MonoBehaviour
    {
        private GameCharacterController gameCharacterController;
        private EffectContainer effectContainer;
        private PropContainer propContainer = null;
        private ModifierSet modifierSet;

        void Awake(){
            this.gameCharacterController = gameObject.GetComponent<GameCharacterController>();
            this.effectContainer = gameObject.GetComponent<EffectContainer>();
            this.propContainer = GetComponent<PropContainer>();
        }
        void Start()
        {
            modifierSet = new()
            {
                moveSpeed = gameCharacterController.moveSpeedMV,
                hangTime = gameCharacterController.hangTimeMV,
                maxJumpingCD = gameCharacterController.maxJumpingCDMV,
                jumpingCDRate = gameCharacterController.jumpingCDRateMV,
                damage = gameCharacterController.damageMV,
                nowJumpingCD = gameCharacterController.nowJumpingCDMV
            };
            if (propContainer != null)
            {
                modifierSet.getPropCD = propContainer.getPropCDMV;
                modifierSet.getPropCDRate = propContainer.getPropCDRateMV;
                modifierSet.propSlot = propContainer.propSlotMV;
                modifierSet.nowGetPropCD = propContainer.nowGetPropCDMV;
            }
            else
            {
                modifierSet.getPropCD = new(0f);
                modifierSet.getPropCDRate = new(0f);
                modifierSet.propSlot = new(0f);
                modifierSet.nowGetPropCD = new(0f);
            }
        }
        /// <summary>
        /// 获取修饰
        /// </summary>
        /// <param name="baseModifierSet"></param>
        /// <returns></returns>
        public ModifierSet GetModifierSet()
        {

            modifierSet.ClearModify();
            List<BaseEffectStrategy> effectStrategies = effectContainer.GetEffectStrategies();
            foreach (var i in effectStrategies)
                {
                    i.Apply(modifierSet);
                }
            return modifierSet;
        }
    }
}