using UnityEngine;

namespace Flicker.Module
{
    public class MockStrategy: BaseEffectStrategy
    {
        public override void Apply(ModifierSet ModifierSet)
        {
            /** 僵直时获得5倍移速 */
            if (gameObject.GetComponent<GameCharacterController>().State == GameCharacterState.Stiffness)
                ModifierSet.moveSpeed.AddMultiplier(5);
        }
    }
}