using Flicker.Module.CharacterEvent;

namespace Flicker.Module.Prop
{
    public class NodeviationArrowProp: BasePropStrategy
    {
        public NodeviationArrowProp()
        {
            name = "不偏矢";
            factoryId = "00004";
            path = "NodeviationArrow";
            description = "下一次弹反后不改变方向";
        }
        public override void OnUse()
        {
            BaseBuffStrategy buff = new()
            {
                name = name,
                description = description,
            };

            BaseEffectStrategy effect = new();
            /** 弹反结果相位把本次跳跃的反向开关关掉，随后自搞 */
            effect.onAdd = (b) => b.Listen<ParryResultEvent>(OnParried);

            void OnParried(ParryResultEvent parryResultEvent)
            {
                parryResultEvent.context.jump.reverse = false;
                buffContainer.RemoveBuff(buff);
            }

            buff.onAdd = (b) => effectContainer.AddEffect(effect, b);
            buff.onRemove = (b) => effectContainer.RemoveEffect(effect);

            buffContainer.AddBuff(buff, -1f);
        }
    }
}