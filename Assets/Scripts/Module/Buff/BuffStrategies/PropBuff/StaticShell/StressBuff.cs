using Flicker.Module.CharacterEvent;

namespace Flicker.Module.Buff.PropBuff
{
    /// <summary>
    /// 应激：下一次僵直时间提升25%，僵直结算后移除
    /// </summary>
    public class StressBuff : BaseBuffStrategy
    {
        /// <summary>
        /// 僵直时间倍率
        /// </summary>
        public const float Multiplier = 1.25f;

        BaseEffectStrategy baseEffectStrategy;
        public StressBuff()
        {
            name = "应激";
            /** 暂无美术资源，补齐 BuffIcon/Stress.png 后改为 "Stress" */
            path = "Unknown";
            description = "下一次僵直时间提升25%";
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
