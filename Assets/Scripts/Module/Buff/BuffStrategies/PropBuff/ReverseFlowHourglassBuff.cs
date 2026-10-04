using Flicker.Module.CharacterEvent;

namespace Flicker.Module.Buff.PropBuff
{
    /// <summary>
    /// 逆流沙漏：下一次跳跃结束后剩余冷却时间减半，随后自动移除
    /// </summary>
    public class ReverseFlowHourglassBuff : BaseBuffStrategy
    {
        BaseEffectStrategy baseEffectStrategy;
        GameCharacterController gameCharacterController;
        public ReverseFlowHourglassBuff()
        {
            name = "逆流沙漏";
            path = "ReverseFlowHourglass";
            description = "下一次跳跃的冷却时间减少50%";
            isStackable = true;
        }
        public override void OnAdd()
        {
            this.gameCharacterController = effectContainer.GetComponent<GameCharacterController>();
            this.baseEffectStrategy = new BaseEffectStrategy();
            /** 跳跃结束后减半，随后自摘 */
            baseEffectStrategy.onAdd = (b) => b.Listen<AfterJumpEvent>(OnJumped);
            effectContainer.AddEffect(baseEffectStrategy, this);
        }
        public override void OnRemove()
        {
            effectContainer.RemoveEffect(baseEffectStrategy);
        }
        private void OnJumped(AfterJumpEvent afterJumpEvent)
        {
            /** 跳跃刚进入满冷却，比例减半即剩余冷却时间减半 */
            gameCharacterController.jumpCDRatio *= 0.5f;
            buffContainer.RemoveBuff(this);
        }
    }
}
