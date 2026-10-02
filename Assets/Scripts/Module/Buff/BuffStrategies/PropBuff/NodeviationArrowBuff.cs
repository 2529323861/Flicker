namespace Flicker.Module.Buff.PropBuff
{
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

        }
        public override void OnRemove()
        {
            effectContainer.RemoveEffect(baseEffectStrategy);
        }
    }
}