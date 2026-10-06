using Flicker.Module.Buff.PropBuff;

namespace Flicker.Module.Prop
{
    public class AnchorClawProp: BasePropStrategy
    {
        public AnchorClawProp()
        {
            name = "锚爪";
            factoryId = "00013";
            path = "AnchorClaw";
            description = "下次跳跃若在30%处没有被弹反，则立刻折返，落地后移速降低20%，持续4秒。";
        }
        public override void OnUse()
        {
            buffContainer.AddBuff(new AnchorClawBuff(), -1f);
        }
    }
}
