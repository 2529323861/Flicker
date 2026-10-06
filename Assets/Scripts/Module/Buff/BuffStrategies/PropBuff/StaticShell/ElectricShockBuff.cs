using Flicker.Module.CharacterEvent;

namespace Flicker.Module.Buff.PropBuff
{
    /// <summary>
    /// 触电：下一次僵直时间提升75%，僵直结算后移除
    /// </summary>
    public class ElectricShockBuff : BaseBuffStrategy
    {
        /// <summary>
        /// 僵直时间倍率
        /// </summary>
        public const float Multiplier = 1.75f;

        BaseEffectStrategy baseEffectStrategy;
        public ElectricShockBuff()
        {
            name = "触电";
            /** 暂无美术资源，补齐 BuffIcon/ElectricShock.png 后改为 "ElectricShock" */
            path = "StaticShell";
            description = "下一次僵直时间提升75%";
            isStackable = true;
            maxStacks = 1;
        }
        public override void OnAdd()
        {
            this.baseEffectStrategy = new BaseEffectStrategy();
            /** 持久修饰僵直时长，僵直结算读取修饰集时生效 */
            baseEffectStrategy.apply = (b, m) => m.Get(ModifierSet.Key.StiffenTime).AddMultiplier(Multiplier);
            /** 落地后自摘。统一走落地事件，僵直即使被取消也不会泄漏 */
            baseEffectStrategy.onAdd = (b) => b.Listen<LandEvent>(evt => buffContainer.RemoveBuff(this));
            effectContainer.AddEffect(baseEffectStrategy, this);
        }
        public override void OnRemove()
        {
            effectContainer.RemoveEffect(baseEffectStrategy);
        }
    }
}
