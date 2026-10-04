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
        public ModifierSet modifierSet = new();

        void Awake(){
            this.gameCharacterController = gameObject.GetComponent<GameCharacterController>();
            this.effectContainer = gameObject.GetComponent<EffectContainer>();
        }
        /// <summary>
        /// 获取修饰
        /// </summary>
        /// <returns></returns>
        public ModifierSet GetModifierSet()
        {
            modifierSet.ClearModify();
            effectContainer.ForEachEffect(effect => effect.Apply(modifierSet));
            return modifierSet;
        }
    }
}