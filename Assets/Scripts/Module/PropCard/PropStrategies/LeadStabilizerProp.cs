using Flicker.Module.Buff.PropBuff;

namespace Flicker.Module.Prop
{
    public class LeadStabilizerProp: BasePropStrategy
    {
        public LeadStabilizerProp()
        {
            name = "铅质稳定器";
            factoryId = "00010";
            path = "LeadStabilizer";
            description = "使用后7秒内，僵直时长降低50%，但移速降低30%。";
        }
        public override void OnUse()
        {
            buffContainer.AddBuff(new LeadStabilizerBuff(), 7f);
        }
    }
}
