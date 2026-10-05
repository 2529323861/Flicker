using Flicker.Module.Buff.PropBuff;

namespace Flicker.Module.Prop
{
    public class CompensationVoucherProp: BasePropStrategy
    {
        public CompensationVoucherProp()
        {
            name = "补偿券";
            factoryId = "00005";
            /** 暂无美术资源，补齐 PropIcon/CompensationVoucher.png 后改为 "CompensationVoucher" */
            path = "Unknown";
            description = "使用后，下一次跳跃若未命中目标，返还70%跳跃冷却；若被弹反，僵直时间降低50%。";
        }
        public override void OnUse()
        {
            buffContainer.AddBuff(new CompensationVoucherBuff(), -1f);
        }
    }
}
