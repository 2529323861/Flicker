namespace Flicker.Module.Buff.PropBuff
{
    public class RushGelBuff : BaseBuffStrategy
    {
        BaseEffectStrategy baseEffectStrategy;
        public RushGelBuff()
        {
            name = "疾行凝胶";
            path = "RushGel";
            description = "移速提高150%，跳跃冷却恢复速率下降50%";
            isStackable = true;
        }
        public override void OnAdd()
        {
            this.baseEffectStrategy = new BaseEffectStrategy();
            baseEffectStrategy.apply = (b, m) =>
            {
                m.Get(ModifierSet.Key.MoveSpeed).AddMultiplier(2.5f);
                m.Get(ModifierSet.Key.JumpingCDRate).AddMultiplier(0.5f);
            };
            effectContainer.AddEffect(baseEffectStrategy, this);
        }
        public override void OnRemove()
        {
            effectContainer.RemoveEffect(baseEffectStrategy);
        }
    }
}