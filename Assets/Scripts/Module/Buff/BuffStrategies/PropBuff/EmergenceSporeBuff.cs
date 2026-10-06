using Flicker.Module.CharacterEvent;
using UnityEngine;

namespace Flicker.Module.Buff.PropBuff
{
    /// <summary>
    /// 羽化孢子：下一次跳跃的滞空时间减少80%，落地后移速下降50%，持续5秒
    /// </summary>
    public class EmergenceSporeBuff : BaseBuffStrategy
    {
        /// <summary>
        /// 前段滞空时间倍率
        /// </summary>
        public const float HangTimeMultiplier = 0.2f;
        /// <summary>
        /// 后段移速倍率
        /// </summary>
        public const float MoveSpeedMultiplier = 0.5f;
        /// <summary>
        /// 后段持续时间
        /// </summary>
        public const float BackDuration = 5f;

        /// <summary>
        /// 前段修饰：滞空时间减少
        /// </summary>
        BaseEffectStrategy frontEffect;
        /// <summary>
        /// 后段修饰：移速降低
        /// </summary>
        BaseEffectStrategy backEffect;
        /// <summary>
        /// 后段剩余时间，非正数表示尚未进入后段或已结束
        /// </summary>
        float backTimer = 0f;
        public EmergenceSporeBuff()
        {
            name = "羽化孢子";
            path = "EmergenceSpore";
            description = "下一次跳跃滞空时间减少80%，落地后移速下降50%，持续5秒";
            /** 前后两段各自计时切换，多实例并存互不干扰 */
            isStackable = false;
        }
        public override void OnAdd()
        {
            this.frontEffect = new BaseEffectStrategy();
            /** 持久修饰滞空时间，跳跃读取修饰集时生效 */
            frontEffect.apply = (b, m) => m.Get(ModifierSet.Key.HangTime).AddMultiplier(HangTimeMultiplier);
            frontEffect.onAdd = (b) =>
            {
                b.Listen<AfterJumpEvent>(OnLanded);
                b.Listen<UpdateEvent>(OnUpdate);
            };
            effectContainer.AddEffect(frontEffect, this);
        }
        public override void OnRemove()
        {
            if (frontEffect != null) effectContainer.RemoveEffect(frontEffect);
            if (backEffect != null) effectContainer.RemoveEffect(backEffect);
        }
        /// <summary>
        /// 落地后切换为后段：撤掉滞空修饰，改为移速惩罚并开始计时
        /// </summary>
        private void OnLanded(AfterJumpEvent afterJumpEvent)
        {
            effectContainer.RemoveEffect(frontEffect);
            frontEffect = null;

            this.backEffect = new BaseEffectStrategy();
            backEffect.apply = (b, m) => m.Get(ModifierSet.Key.MoveSpeed).AddMultiplier(MoveSpeedMultiplier);
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
