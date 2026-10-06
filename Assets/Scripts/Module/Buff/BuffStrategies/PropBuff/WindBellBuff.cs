using Flicker.Module.CharacterEvent;

namespace Flicker.Module.Buff.PropBuff
{
    /// <summary>
    /// 借风铃：本次跳跃滞空时间增加30%，且不造成攻击
    /// </summary>
    public class WindBellBuff : BaseBuffStrategy
    {
        /// <summary>
        /// 滞空时间倍率
        /// </summary>
        public const float HangTimeMultiplier = 1.3f;

        BaseEffectStrategy baseEffectStrategy;
        public WindBellBuff()
        {
            name = "借风铃";
            /** 暂无美术资源，补齐 BuffIcon/WindBell.png 后改为 "WindBell" */
            path = "Unknown";
            description = "本次跳跃滞空时间增加30%，不造成攻击";
            isStackable = true;
        }
        public override void OnAdd()
        {
            this.baseEffectStrategy = new BaseEffectStrategy();
            /** 持久修饰滞空时间，跳跃读取修饰集时生效 */
            baseEffectStrategy.apply = (b, m) => m.Get(ModifierSet.Key.HangTime).AddMultiplier(HangTimeMultiplier);
            baseEffectStrategy.onAdd = (b) =>
            {
                b.Listen<BeforeAttackEvent>(OnBeforeAttack);
                b.Listen<LandEvent>(OnLandFinished);
            };
            effectContainer.AddEffect(baseEffectStrategy, this);
        }
        public override void OnRemove()
        {
            effectContainer.RemoveEffect(baseEffectStrategy);
        }
        /// <summary>
        /// 取消本次跳跃造成的所有攻击
        /// </summary>
        private void OnBeforeAttack(BeforeAttackEvent beforeAttackEvent)
        {
            beforeAttackEvent.context.cancelled = true;
        }
        private void OnLandFinished(LandEvent landEvent)
        {
            buffContainer.RemoveBuff(this);
        }
    }
}
