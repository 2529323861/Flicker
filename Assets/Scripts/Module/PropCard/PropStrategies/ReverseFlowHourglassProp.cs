using Flicker.Core;
using Flicker.Module.Buff.PropBuff;

namespace Flicker.Module.Prop
{
    public class ReverseFlowHourglassProp: BasePropStrategy
    {
        public ReverseFlowHourglassProp()
        {
            name = "逆流沙漏";
            factoryId = "00003";
            path = "ReverseFlowHourglass";
            description = "立即重置跳跃冷却，并且下一次跳跃的冷却时间减少50%。";
        }
        public override void OnUse()
        {
            /** 立即重置跳跃冷却 */
            gameCharacterController.jumpCDRatio = Ratio.Empty;
            /** 下一次跳跃的冷却时间减半 */
            buffContainer.AddBuff(new ReverseFlowHourglassBuff(), -1f);
        }
    }
}