namespace Flicker.Module.Buff.PropBuff
{
    /// <summary>
    /// 轻质骨：跳跃冷却上限减少40%，但僵直时长提升30%
    /// </summary>
    public class LightBoneBuff : BaseBuffStrategy
    {
        /// <summary>
        /// 跳跃冷却上限倍率
        /// </summary>
        public const float JumpingCDMultiplier = 0.6f;
        /// <summary>
        /// 僵直时长倍率
        /// </summary>
        public const float StiffenTimeMultiplier = 1.3f;

        BaseEffectStrategy baseEffectStrategy;
        public LightBoneBuff()
        {
            name = "轻质骨";
            path = "LightBone";
            description = "跳跃冷却上限减少40%，僵直时长提升30%";
            isStackable = true;
        }
        public override void OnAdd()
        {
            this.baseEffectStrategy = new BaseEffectStrategy();
            baseEffectStrategy.apply = (b, m) =>
            {
                m.Get(ModifierSet.Key.MaxJumpingCD).AddMultiplier(JumpingCDMultiplier);
                m.Get(ModifierSet.Key.StiffenTime).AddMultiplier(StiffenTimeMultiplier);
            };
            effectContainer.AddEffect(baseEffectStrategy, this);
        }
        public override void OnRemove()
        {
            effectContainer.RemoveEffect(baseEffectStrategy);
        }
    }
}
