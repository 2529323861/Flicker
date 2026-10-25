using Flicker.Module.CharacterEvent;
using UnityEngine;

namespace Flicker.Module.Buff.PropBuff
{
    /// <summary>
    /// 补偿券：下一次跳跃的落地补偿。
    /// 未命中返还70%跳跃冷却；命中（含被弹反）则道具冷却速率降低，持续7秒
    /// </summary>
    public class CompensationVoucherBuff : BaseBuffStrategy
    {
        /// <summary>
        /// 跳跃冷却是按"剩余比例"存储的，返还70%即剩余30%
        /// </summary>
        public const float JumpCDRemainRatio = 0.3f;
        /// <summary>
        /// 后段道具冷却速率倍率。原获取CD ×1.5 等价于速率 ÷1.5
        /// </summary>
        public const float PropCDRateMultiplier = 2f / 3f;
        /// <summary>
        /// 后段持续时间
        /// </summary>
        public const float BackDuration = 7f;

        /// <summary>
        /// 本次跳跃是否命中，被弹反视作命中
        /// </summary>
        private bool hitted = false;
        BaseEffectStrategy baseEffectStrategy;
        /// <summary>
        /// 后段修饰：道具获取冷却提升
        /// </summary>
        BaseEffectStrategy backEffect;
        /// <summary>
        /// 后段剩余时间，非正数表示尚未进入后段或已结束
        /// </summary>
        float backTimer = 0f;
        GameCharacterController gameCharacterController;
        public CompensationVoucherBuff()
        {
            name = "补偿券";
            /** 暂无美术资源，补齐 BuffIcon/CompensationVoucher.png 后改为 "CompensationVoucher" */
            path = "CompensationVoucher";
            description = "未命中返还70%跳跃冷却；命中则道具冷却速率降低33%，持续7秒";
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
                b.Listen<UpdateEvent>(OnUpdate);
            };
            effectContainer.AddEffect(baseEffectStrategy, this);
        }
        public override void OnRemove()
        {
            if (backEffect != null) effectContainer.RemoveEffect(backEffect);
            effectContainer.RemoveEffect(baseEffectStrategy);
        }
        private void OnHitted(AfterAttackEvent afterAttackEvent)
        {
            hitted = true;
        }
        private void OnParried(AfterParryEvent afterParryEvent)
        {
            /** 被弹反视作命中 */
            hitted = true;
        }
        /// <summary>
        /// 落地结算
        /// </summary>
        private void OnLanded(AfterJumpEvent afterJumpEvent)
        {
            if (!hitted)
            {
                /** 未命中：返还70%跳跃冷却，随后自摘 */
                gameCharacterController.jumpCDRatio *= JumpCDRemainRatio;
                buffContainer.RemoveBuff(this);
                return;
            }

            /** 命中：切换为后段，附加道具冷却速率惩罚并开始计时 */
            this.backEffect = new BaseEffectStrategy();
            backEffect.apply = (b, m) => m.Get(ModifierSet.Key.PropCDRate).AddMultiplier(PropCDRateMultiplier);
            effectContainer.AddEffect(backEffect);

            backTimer = BackDuration;
        }
        private void OnUpdate(UpdateEvent updateEvent)
        {
            if (backTimer <= 0f) return;
            backTimer -= Time.deltaTime;
            if (backTimer <= 0f) buffContainer.RemoveBuff(this);
        }
    }
}
