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
            description = "下一次跳跃期间弹反不改变方向，但自身僵直时间增加25%，跳跃结束即失效。";
        }
        public override void OnUse()
        {
            buffContainer.AddBuff(new NodeviationArrowBuff(), -1f);
        }
    }
}