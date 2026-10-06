using Flicker.Module.Buff.PropBuff;

namespace Flicker.Module.Prop
{
    public class CompensationVoucherProp: BasePropStrategy
    {
        public CompensationVoucherProp()
        {
            name = "补偿券";
            factoryId = "00005";
            path = "CompensationVoucher";
            description = "使用后，下一次跳跃若未命中目标，返还70%跳跃冷却；命中则道具获取冷却提升50%，持续7秒。";
        }
        public override void OnUse()
        {
            buffContainer.AddBuff(new CompensationVoucherBuff(), -1f);
        }
    }
}
