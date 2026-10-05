using Flicker.Module.Buff.PropBuff;

namespace Flicker.Module.Prop
{
    public class PulseClockProp: BasePropStrategy
    {
        public PulseClockProp()
        {
            name = "脉冲发条";
            factoryId = "00007";
            /** 暂无美术资源，补齐 PropIcon/PulseClock.png 后改为 "PulseClock" */
            path = "Unknown";
            description = "沿当前移动方向冲刺3单位距离。";
        }
        public override void OnUse()
        {
            buffContainer.AddBuff(new PulseClockBuff(gameCharacterController.towardUp), -1f);
        }
    }
}
