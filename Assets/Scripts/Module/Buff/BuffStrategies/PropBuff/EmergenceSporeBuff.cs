using Flicker.Module.CharacterEvent;

namespace Flicker.Module.Buff.PropBuff
{
    /// <summary>
    /// 羽化孢子：3秒内下一次跳跃的滞空时间减少80%，跳跃后或到期自动移除
    /// </summary>
    public class EmergenceSporeBuff : BaseBuffStrategy
    {
        BaseEffectStrategy baseEffectStrategy;
        public EmergenceSporeBuff()
        {
            name = "羽化孢子";
            path = "EmergenceSpore";
            description = "3秒内，下一次跳跃的滞空时间减少80%";
            isStackable = true;
        }
        public override void OnAdd()
        {
            this.baseEffectStrategy = new BaseEffectStrategy();
            /** 持久修饰滞空时间，跳跃读取修饰集时生效 */
            baseEffectStrategy.apply = (b, m) => m.Get(ModifierSet.Key.HangTime).AddMultiplier(0.2f);
            /** 跳跃结束后自摘 */
            baseEffectStrategy.onAdd = (b) => b.Listen<AfterJumpEvent>(evt => buffContainer.RemoveBuff(this));
            effectContainer.AddEffect(baseEffectStrategy, this);
        }
        public override void OnRemove()
        {
            effectContainer.RemoveEffect(baseEffectStrategy);
        }
    }
}
