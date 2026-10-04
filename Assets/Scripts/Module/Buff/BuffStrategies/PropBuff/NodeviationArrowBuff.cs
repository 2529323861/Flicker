using Flicker.Module.CharacterEvent;

namespace Flicker.Module.Buff.PropBuff
{
    /// <summary>
    /// 不偏矢：下一次弹反后保持原方向，弹反结束后自动移除
    /// </summary>
    public class NodeviationArrowBuff : BaseBuffStrategy
    {
        BaseEffectStrategy baseEffectStrategy;
        public NodeviationArrowBuff()
        {
            name = "不偏矢";
            description = "下一次弹反后不改变方向";
            isStackable = true;
        }
        public override void OnAdd()
        {
            this.baseEffectStrategy = new BaseEffectStrategy();
            /** 弹反结果相位把本次跳跃的反向开关关掉，随后自摘 */
            baseEffectStrategy.onAdd = (b) => b.Listen<ParryResultEvent>(OnParried);
            effectContainer.AddEffect(baseEffectStrategy, this);
        }
        public override void OnRemove()
        {
            effectContainer.RemoveEffect(baseEffectStrategy);
        }
        private void OnParried(ParryResultEvent parryResultEvent)
        {
            parryResultEvent.context.jump.reverse = false;
            buffContainer.RemoveBuff(this);
        }
    }
}