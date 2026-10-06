using Flicker.Module.Buff.PropBuff;

namespace Flicker.Module.Prop
{
    public class EmergenceSporeProp: BasePropStrategy
    {
        public EmergenceSporeProp()
        {
            name = "羽化孢子";
            factoryId = "00002";
            path = "EmergenceSpore";
            description = "下一次跳跃的滞空时间减少80%，落地后移速下降50%，持续5秒。";
        }
        public override void OnUse()
        {
            buffContainer.AddBuff(new EmergenceSporeBuff(), -1f);
        }
    }
}