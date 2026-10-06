using Flicker.Module.Buff.PropBuff;

namespace Flicker.Module.Prop
{
    public class LeadStabilizerProp: BasePropStrategy
    {
        public LeadStabilizerProp()
        {
            name = "铅制稳定器";
            factoryId = "00010";
            /** 暂无美术资源，补齐 PropIcon/LeadStabilizer.png 后改为 "LeadStabilizer" */
            path = "Unknown";
            description = "使用后7秒内，僵直时长降低50%，但移速降低30%。";
        }
        public override void OnUse()
        {
            buffContainer.AddBuff(new LeadStabilizerBuff(), 7f);
        }
    }
}
