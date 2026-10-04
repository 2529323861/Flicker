namespace Flicker.Module.Buff.PropBuff
{
    public class RushGelBuff : BaseBuffStrategy
    {
        BaseEffectStrategy baseEffectStrategy;
        public RushGelBuff()
        {
            name = "疾行凝胶";
            description = "移速提高150%";
            isStackable = true;
        }
        public override void OnAdd()
        {
            this.baseEffectStrategy = new BaseEffectStrategy();
            baseEffectStrategy.apply = (b, m) =>
            {
                m.Get(ModifierSet.Key.MoveSpeed).AddMultiplier(2.5f);
            };
            effectContainer.AddEffect(baseEffectStrategy);
        }
        public override void OnRemove()
        {
            effectContainer.RemoveEffect(baseEffectStrategy);
        }
    }
}