using Flicker.Module.Buff.PropBuff;

namespace Flicker.Module.Prop
{
    public class StaticShellProp: BasePropStrategy
    {
        public StaticShellProp()
        {
            name = "静电甲壳";
            factoryId = "00008";
            path = "StaticShell";
            description = "使用后5秒内，受到弹反时自身僵直时间增加25%，攻击来源增加75%。";
        }
        public override void OnUse()
        {
            buffContainer.AddBuff(new StaticShellBuff(), 5f);
        }
    }
}
