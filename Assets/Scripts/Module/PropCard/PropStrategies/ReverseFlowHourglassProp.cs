using Flicker.Core;
using Flicker.Module.CharacterEvent;

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

            BaseBuffStrategy buff = new()
            {
                name = name,
                description = description,
            };

            BaseEffectStrategy effect = new();
            /** 跳跃结束后把冷却进度减半，剩余冷却时间随之减半，随后自搞 */
            effect.onAdd = (b) => b.Listen<AfterJumpEvent>(OnJumped);

            void OnJumped(AfterJumpEvent afterJumpEvent)
            {
                gameCharacterController.jumpCDRatio *= 0.5f;
                buffContainer.RemoveBuff(buff);
            }

            buff.onAdd = (b) => effectContainer.AddEffect(effect, b);
            buff.onRemove = (b) => effectContainer.RemoveEffect(effect);

            buffContainer.AddBuff(buff, -1f);
        }
    }
}