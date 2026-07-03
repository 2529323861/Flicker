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

        private ModifierSet modifierSet;

        void Awake(){
            this.gameCharacterController = gameObject.GetComponent<GameCharacterController>();
            this.effectContainer = gameObject.GetComponent<EffectContainer>();
        }
        void Start()
        {
            modifierSet = new()
            {
                moveSpeed = gameCharacterController.moveSpeedMV,
                hangTime = gameCharacterController.hangTimeMV,
                maxJumpingCD = gameCharacterController.maxJumpingCDMV,
                jumpingCDRate = gameCharacterController.jumpingCDRateMV,
                maxStiffTime = gameCharacterController.maxStiffTimeMV,
                stiffTimeRate = gameCharacterController.stiffTimeRateMV,
                damage = gameCharacterController.damageMV,
                nowStiffTime = gameCharacterController.nowStiffTimeMV,
                nowJumpingCD = gameCharacterController.nowJumpingCDMV
            };
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