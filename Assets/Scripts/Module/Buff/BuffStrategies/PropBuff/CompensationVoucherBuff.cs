using Flicker.Module.CharacterEvent;

namespace Flicker.Module.Buff.PropBuff
{
    /// <summary>
    /// 补偿券：下一次跳跃的落地补偿。
    /// 未命中返还70%跳跃冷却；被弹反则本次僵直减半；命中则无补偿
    /// </summary>
    public class CompensationVoucherBuff : BaseBuffStrategy
    {
        /// <summary>
        /// 本次跳跃的结果：0 = 未命中，1 = 弹反，2 = 命中
        /// </summary>
        private int status = 0;
        BaseEffectStrategy baseEffectStrategy;
        /// <summary>
        /// 僵直减半的临时修饰，仅在僵直结算前挂载
        /// </summary>
        BaseEffectStrategy stiffenEffect;
        GameCharacterController gameCharacterController;
        public CompensationVoucherBuff()
        {
            name = "补偿券";
            /** 暂无美术资源，补齐 BuffIcon/CompensationVoucher.png 后改为 "CompensationVoucher" */
            path = "Unknown";
            description = "未命中返还70%跳跃冷却；被弹反则僵直减半";
            isStackable = true;
        }
        public override void OnAdd()
        {
            this.gameCharacterController = effectContainer.GetComponent<GameCharacterController>();
            this.baseEffectStrategy = new BaseEffectStrategy();
            /** 命中与弹反只记录结果，落地后才统一结算 */
            baseEffectStrategy.onAdd = (b) =>
            {
                b.Listen<AfterAttackEvent>(OnHitted);
                b.Listen<AfterParryEvent>(OnParried);
                b.Listen<AfterJumpEvent>(OnLanded);
                b.Listen<LandEvent>(OnLandFinished);
            };
            effectContainer.AddEffect(baseEffectStrategy, this);
        }
        public override void OnRemove()
        {
            /** 仅弹反路径挂载过临时修饰 */
            if (stiffenEffect != null) effectContainer.RemoveEffect(stiffenEffect);
            effectContainer.RemoveEffect(baseEffectStrategy);
        }
        private void OnHitted(AfterAttackEvent afterAttackEvent)
        {
            status = 2;
        }
        private void OnParried(AfterParryEvent afterParryEvent)
        {
            status = 1;
        }
        /// <summary>
        /// 落地结算。此时命中与弹反均已确定，且僵直尚未结算
        /// </summary>
        private void OnLanded(AfterJumpEvent afterJumpEvent)
        {
            if (status == 0)
            {
                /** 未命中：返还70%跳跃冷却，剩余冷却为30% */
                gameCharacterController.jumpCDRatio *= 0.3f;
            }
            else if (status == 1)
            {
                /** 被弹反：挂上僵直减半的修饰，待僵直结算后摘除 */
                this.stiffenEffect = new BaseEffectStrategy();
                stiffenEffect.apply = (b, m) => m.Get(ModifierSet.Key.StiffenTime).AddMultiplier(0.5f);
                effectContainer.AddEffect(stiffenEffect);
            }
        }
        /// <summary>
        /// 跳跃结算完毕。无论僵直是否发生都会走到这里，在此保证清理并自摘
        /// </summary>
        private void OnLandFinished(LandEvent landEvent)
        {
            if (stiffenEffect != null)
            {
                effectContainer.RemoveEffect(stiffenEffect);
                stiffenEffect = null;
            }
            buffContainer.RemoveBuff(this);
        }
    }
}
