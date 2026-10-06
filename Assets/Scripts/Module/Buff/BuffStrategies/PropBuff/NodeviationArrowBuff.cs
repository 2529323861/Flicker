using Flicker.Module.CharacterEvent;

namespace Flicker.Module.Buff.PropBuff
{
    /// <summary>
    /// 不偏矢：下一次弹反后保持原方向，但自身僵直时间增加25%，落地后自动移除
    /// </summary>
    public class NodeviationArrowBuff : BaseBuffStrategy
    {
        /// <summary>
        /// 僵直时间倍率
        /// </summary>
        public const float StiffenTimeMultiplier = 1.25f;

        BaseEffectStrategy baseEffectStrategy;
        /// <summary>
        /// 僵直提升的临时修饰，仅在弹反后挂载
        /// </summary>
        BaseEffectStrategy stiffenEffect;
        public NodeviationArrowBuff()
        {
            name = "不偏矢";
            description = "下一次弹反不改变方向，僵直时间增加25%";
            isStackable = true;
        }
        public override void OnAdd()
        {
            this.baseEffectStrategy = new BaseEffectStrategy();
            /** 弹反结果相位关掉反向开关并挂上僵直修饰，落地后统一清理 */
            baseEffectStrategy.onAdd = (b) =>
            {
                b.Listen<ParryResultEvent>(OnParried);
                b.Listen<LandEvent>(OnLandFinished);
            };
            effectContainer.AddEffect(baseEffectStrategy, this);
        }
        public override void OnRemove()
        {
            /** 仅弹反路径挂载过临时修饰 */
            if (stiffenEffect != null) effectContainer.RemoveEffect(stiffenEffect);
            effectContainer.RemoveEffect(baseEffectStrategy);
        }
        private void OnParried(ParryResultEvent parryResultEvent)
        {
            parryResultEvent.context.jump.reverse = false;

            /** 僵直结算会重新读取修饰集，故此处挂载即时生效 */
            this.stiffenEffect = new BaseEffectStrategy();
            stiffenEffect.apply = (b, m) => m.Get(ModifierSet.Key.StiffenTime).AddMultiplier(StiffenTimeMultiplier);
            effectContainer.AddEffect(stiffenEffect);
        }
        /// <summary>
        /// 跳跃结算完毕。无论僵直是否发生都会走到这里，在此保证清理并自摘
        /// </summary>
        private void OnLandFinished(LandEvent landEvent)
        {
            if (stiffenEffect != null)
            {
                effectContainer.RemoveEffect(stiffenEffect);
                stiffenEffect = null;
            }
            buffContainer.RemoveBuff(this);
        }
    }
}