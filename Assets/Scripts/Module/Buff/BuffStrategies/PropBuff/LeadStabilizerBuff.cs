namespace Flicker.Module.Buff.PropBuff
{
    /// <summary>
    /// 铅制稳定器：僵直时长降低50%，但移速降低30%
    /// </summary>
    public class LeadStabilizerBuff : BaseBuffStrategy
    {
        /// <summary>
        /// 僵直时长倍率
        /// </summary>
        public const float StiffenTimeMultiplier = 0.5f;
        /// <summary>
        /// 移速倍率
        /// </summary>
        public const float MoveSpeedMultiplier = 0.7f;

        BaseEffectStrategy baseEffectStrategy;
        public LeadStabilizerBuff()
        {
            name = "铅质稳定器";
            path = "LeadStabilizer";
            description = "僵直时长降低50%，移速降低30%";
            isStackable = true;
        }
        public override void OnAdd()
        {
            this.baseEffectStrategy = new BaseEffectStrategy();
            baseEffectStrategy.apply = (b, m) =>
            {
                m.Get(ModifierSet.Key.StiffenTime).AddMultiplier(StiffenTimeMultiplier);
                m.Get(ModifierSet.Key.MoveSpeed).AddMultiplier(MoveSpeedMultiplier);
            };
            effectContainer.AddEffect(baseEffectStrategy, this);
        }
        public override void OnRemove()
        {
            effectContainer.RemoveEffect(baseEffectStrategy);
        }
    }
}
