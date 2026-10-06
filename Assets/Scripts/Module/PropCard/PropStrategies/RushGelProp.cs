using Flicker.Module.Buff.PropBuff;

namespace Flicker.Module.Prop
{
    public class RushGelProp: BasePropStrategy
    {
        public RushGelProp()
        {
            name = "疾行凝胶";
            factoryId = "00001";
            path = "RushGel";
            description = "使用后3秒内，移动速度提高150%，跳跃冷却恢复速率下降50%。";
        }
        public override void OnUse()
        {
            buffContainer.AddBuff(new RushGelBuff(),3);
        }
    }
}