using Flicker.Module.CharacterEvent;

namespace Flicker.Module.Prop
{
    public class EmergenceSporeProp: BasePropStrategy
    {
        public EmergenceSporeProp()
        {
            name = "羽化孢子";
            factoryId = "00002";
            path = "EmergenceSpore";
            description = "使用后，下一次跳跃的滞空时间减少80%";
        }
        public override void OnUse()
        {
            BaseBuffStrategy buff = new()
            {
                name = name,
                description = description,
            };

            BaseEffectStrategy effect = new();
            /** 持久修饰滞空时间，本次跳跃读取时生效 */
            effect.apply = (b, m) => m.Get(ModifierSet.Key.HangTime).AddMultiplier(0.2f);
            /** 跳跃结束后自搞 */
            effect.onAdd = (b) => b.Listen<AfterJumpEvent>(evt => buffContainer.RemoveBuff(buff));

            buff.onAdd = (b) => effectContainer.AddEffect(effect, b);
            buff.onRemove = (b) => effectContainer.RemoveEffect(effect);

            buffContainer.AddBuff(buff, -1f);
        }
    }
}