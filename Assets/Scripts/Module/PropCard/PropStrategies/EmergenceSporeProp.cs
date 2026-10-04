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
            description = "使用后3秒内，下一次跳跃的滞空时间减少80%。";
        }
        public override void OnUse()
        {
            buffContainer.AddBuff(new EmergenceSporeBuff(), 3f);
        }
    }
}