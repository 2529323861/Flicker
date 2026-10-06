using Flicker.Module.Buff;
using Flicker.Module.Buff.PropBuff;

namespace Flicker.Module.Prop
{
    public class WindBellProp: BasePropStrategy
    {
        public WindBellProp()
        {
            name = "借风铃";
            factoryId = "00012";
            path = "WindBell";
            description = "立刻起跳，无视跳跃冷却，立刻恢复僵直，滞空时间增加30%，本次跳跃不造成攻击。";
        }
        /// <summary>
        /// 需要在可起跳时才使用，否则跳跃不会发生而buff会残留
        /// </summary>
        public override bool CanUse()
        {
            var state = gameCharacterController.State;
            return state == GameCharacterState.Idle || state == GameCharacterState.Moving;
        }
        public override void OnUse()
        {
            /** 僵直会禁止跳跃，先解除 */
            buffContainer.RemoveBuff<StiffnessBuff>();

            /** 先挂上滞空修饰，跳跃读取修饰集时才会生效 */
            buffContainer.AddBuff(new WindBellBuff(), -1f);

            /** 立刻起跳，无视跳跃冷却 */
            gameCharacterController.ForceJump();
        }
    }
}
