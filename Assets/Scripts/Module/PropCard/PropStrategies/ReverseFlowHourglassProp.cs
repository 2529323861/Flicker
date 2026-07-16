namespace Flicker.Module.Prop
{
    public class ReverseFlowHourglassProp: BasePropStrategy
    {
        public ReverseFlowHourglassProp()
        {
            name = "逆流沙漏";
            id = "00003";
            path = "ReverseFlowHourglass";
            description = "立即重置跳跃冷却，并且下一次跳跃的冷却时间减少50%。";
        }
        public override void OnUse(BuffContainer buffContainer, EffectContainer effectContainer)
        {
            
        }
    }
}