using Flicker.Module.CharacterEvent;

namespace Flicker.Module.Buff.PropBuff
{
    /// <summary>
    /// 逆流沙漏：跳跃冷却上限上升30%，下次跳跃的冷却走完后自动移除
    /// </summary>
    public class ReverseFlowHourglassBuff : BaseBuffStrategy
    {
        /// <summary>
        /// 跳跃冷却上限倍率
        /// </summary>
        public const float JumpingCDMultiplier = 1.3f;

        BaseEffectStrategy baseEffectStrategy;
        GameCharacterController gameCharacterController;
        /// <summary>
        /// 下次跳跃是否已发生。冷却在落地那一刻才开始走，故需等它走完再解除
        /// </summary>
        bool jumped = false;
        public ReverseFlowHourglassBuff()
        {
            name = "逆流沙漏";
            path = "ReverseFlowHourglass";
            description = "跳跃冷却上限上升30%，下次跳跃的冷却走完后解除";
            isStackable = true;
        }
        public override void OnAdd()
        {
            this.gameCharacterController = effectContainer.GetComponent<GameCharacterController>();
            this.baseEffectStrategy = new BaseEffectStrategy();
            baseEffectStrategy.apply = (b, m) => m.Get(ModifierSet.Key.MaxJumpingCD).AddMultiplier(JumpingCDMultiplier);
            baseEffectStrategy.onAdd = (b) =>
            {
                b.Listen<AfterJumpEvent>(OnJumped);
                b.Listen<UpdateEvent>(OnUpdate);
            };
            effectContainer.AddEffect(baseEffectStrategy, this);
        }
        public override void OnRemove()
        {
            effectContainer.RemoveEffect(baseEffectStrategy);
        }
        private void OnJumped(AfterJumpEvent afterJumpEvent)
        {
            jumped = true;
        }
        private void OnUpdate(UpdateEvent updateEvent)
        {
            /** 等下次跳跃的冷却真正走完，代价才算兑现 */
            if (jumped && gameCharacterController.jumpCDRatio <= 0)
            {
                buffContainer.RemoveBuff(this);
            }
        }
    }
}
