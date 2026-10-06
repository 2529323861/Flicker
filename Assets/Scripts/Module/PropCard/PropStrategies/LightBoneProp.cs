using Flicker.Module.Buff.PropBuff;

namespace Flicker.Module.Prop
{
    public class LightBoneProp: BasePropStrategy
    {
        public LightBoneProp()
        {
            name = "轻质骨";
            factoryId = "00009";
            path = "LightBone";
            description = "使用后5秒内，跳跃冷却上限减少40%，但僵直时长提升30%。";
        }
        public override void OnUse()
        {
            buffContainer.AddBuff(new LightBoneBuff(), 5f);
        }
    }
}
