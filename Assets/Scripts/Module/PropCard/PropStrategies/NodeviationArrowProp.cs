using Flicker.Module.Buff.PropBuff;

namespace Flicker.Module.Prop
{
    public class NodeviationArrowProp: BasePropStrategy
    {
        public NodeviationArrowProp()
        {
            name = "不偏矢";
            factoryId = "00004";
            path = "NodeviationArrow";
            description = "下一次弹反后不改变方向";
        }
        public override void OnUse(BuffContainer buffContainer, EffectContainer effectContainer)
        {

        }
    }
}