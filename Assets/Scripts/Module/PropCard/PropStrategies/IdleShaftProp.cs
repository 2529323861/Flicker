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
            path = "IdleShaft";
            description = "立刻清空全部道具冷却，但道具冷却速率降低20%，下次使用道具后解除。";
        }
        public override void OnUse()
        {
            /** 立即清空全部道具冷却，随后挂上冷却速率惩罚 */
            propContainer.ClearAllCD();
            buffContainer.AddBuff(new IdleShaftBuff(), -1f);
        }
    }
}
