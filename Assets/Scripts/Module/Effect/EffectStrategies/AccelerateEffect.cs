namespace Flicker.Module.Effect
{
    /// <summary>
    /// 通用加速效果
    /// </summary>
    public class AccelerateEffect: BaseEffectStrategy
    {
        private float factor;
        public AccelerateEffect(float factor)
        {
            this.factor = factor;
        }

        public override void Apply(ModifierSet ModifierSet)
        {
            ModifierSet.moveSpeed.AddMultiplier(factor);
        }
    }
}