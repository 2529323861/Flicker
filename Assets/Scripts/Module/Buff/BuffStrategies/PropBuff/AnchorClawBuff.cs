using Flicker.Module.CharacterEvent;
using Flicker.Module.Context;
using UnityEngine;

namespace Flicker.Module.Buff.PropBuff
{
    /// <summary>
    /// 锚爪：下次跳跃若在指定进度处仍未被弹反，则立刻折返。
    /// 折返后落地会附加移速惩罚
    /// </summary>
    public class AnchorClawBuff : BaseBuffStrategy
    {
        /// <summary>
        /// 折返触发进度
        /// </summary>
        public const float ReturnProgress = 0.3f;
        /// <summary>
        /// 后段移速倍率
        /// </summary>
        public const float MoveSpeedMultiplier = 0.8f;
        /// <summary>
        /// 后段持续时间
        /// </summary>
        public const float BackDuration = 4f;

        /// <summary>
        /// 本次跳跃的上下文，飞行期间用于读取与折返
        /// </summary>
        JumpContext context;
        /// <summary>
        /// 触发折返的进度
        /// </summary>
        float triggerProgress;
        /// <summary>
        /// 是否仍在等待折返时机
        /// </summary>
        bool armed = false;
        /// <summary>
        /// 后段剩余时间，非正数表示尚未进入后段或已结束
        /// </summary>
        float backTimer = 0f;

        BaseEffectStrategy baseEffectStrategy;
        /// <summary>
        /// 后段修饰：移速惩罚
        /// </summary>
        BaseEffectStrategy backEffect;
        public AnchorClawBuff()
        {
            name = "锚爪";
            /** 暂无美术资源，补齐 BuffIcon/AnchorClaw.png 后改为 "AnchorClaw" */
            path = "Unknown";
            description = "下次跳跃在30%处未被弹反则立刻折返，落地后移速下降20%持续4秒";
            isStackable = true;
        }
        public override void OnAdd()
        {
            this.baseEffectStrategy = new BaseEffectStrategy();
            baseEffectStrategy.onAdd = (b) =>
            {
                b.Listen<JumpResultEvent>(OnJumpResult);
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
        /// <summary>
        /// 结果相位：预定本次跳跃的折返时机
        /// </summary>
        private void OnJumpResult(JumpResultEvent jumpResultEvent)
        {
            this.context = jumpResultEvent.context;
            this.triggerProgress = ReturnProgress;
            this.armed = true;
        }
        /// <summary>
        /// 已被弹反，放弃折返
        /// </summary>
        private void OnParried(AfterParryEvent afterParryEvent)
        {
            armed = false;
        }
        /// <summary>
        /// 落地：切换为后段，无论是否折返都附加惩罚
        /// </summary>
        private void OnLanded(AfterJumpEvent afterJumpEvent)
        {
            this.context = null;
            this.armed = false;

            this.backEffect = new BaseEffectStrategy();
            backEffect.apply = (b, m) => m.Get(ModifierSet.Key.MoveSpeed).AddMultiplier(MoveSpeedMultiplier);
            effectContainer.AddEffect(backEffect);

            backTimer = BackDuration;
        }
        private void OnUpdate(UpdateEvent updateEvent)
        {
            /** 后段：移速惩罚倒计时 */
            if (backTimer > 0f)
            {
                backTimer -= Time.deltaTime;
                if (backTimer <= 0f) buffContainer.RemoveBuff(this);
                return;
            }

            /** 前段：到达预定进度则折返 */
            if (!armed || context == null) return;
            if (context.progress < triggerProgress) return;

            armed = false;
            context.FoldBack();
        }
    }
}
