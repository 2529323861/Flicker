using Flicker.Core;
using Flicker.Module.Buff.PropBuff;

namespace Flicker.Module.Prop
{
    public class IdleShaftProp: BasePropStrategy
    {
        public IdleShaftProp()
        {
            name = "空转轴";
            factoryId = "00011";
            /** 暂无美术资源，补齐 PropIcon/IdleShaft.png 后改为 "IdleShaft" */
            path = "Unknown";
            description = "立刻清空道具获取冷却，但道具获取冷却上升20%，下次抽到道具后解除。";
        }
        public override void OnUse()
        {
            /** 立即清空道具获取冷却，下一次 Update 发放道具 */
            propContainer.propCDRatio = Ratio.Empty;
            buffContainer.AddBuff(new IdleShaftBuff(), -1f);
        }
    }
}
