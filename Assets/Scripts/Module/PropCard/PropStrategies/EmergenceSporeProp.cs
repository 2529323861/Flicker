namespace Flicker.Module.Prop
{
    public class EmergenceSporeProp: BasePropStrategy
    {
        public EmergenceSporeProp()
        {
            name = "羽化孢子";
            factoryId = "00002";
            path = "EmergenceSpore";
            description = "使用后，下一次跳跃的滞空时间减少80%";
        }
        public override void OnUse(BuffContainer buffContainer, EffectContainer effectContainer)
        {
            
        }
    }
}