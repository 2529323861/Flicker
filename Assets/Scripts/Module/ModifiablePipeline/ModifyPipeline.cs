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
        public ModifierSet modifierSet = new()
            {
                moveSpeed = new(0f),
                hangTime = new(0f),
                maxJumpingCD = new(0f),
                jumpingCDRate = new(0f),
                damage = new(0f),
                nowJumpingCD = new(0f),
                getPropCD = new(0f),
                getPropCDRate = new(0f),
                propSlot = new(0f),
                nowGetPropCD = new(0f),
            };

        void Awake(){
            this.gameCharacterController = gameObject.GetComponent<GameCharacterController>();
            this.effectContainer = gameObject.GetComponent<EffectContainer>();
            this.propContainer = GetComponent<PropContainer>();
        }
        void Start()
        {
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