using Flicker.Module.Buff.PropBuff;

namespace Flicker.Module.Prop
{
    public class PulseClockProp: BasePropStrategy
    {
        public PulseClockProp()
        {
            name = "脉冲发条";
            factoryId = "00007";
            path = "PulseClock";
            description = "沿当前方向冲刺3单位距离，期间不可被攻击，随后移速下降30%持续4秒。";
        }
        public override void OnUse()
        {
            buffContainer.AddBuff(new PulseClockBuff(gameCharacterController.towardUp), -1f);
        }
    }
}
