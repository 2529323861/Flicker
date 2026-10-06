using Flicker.Module.CharacterEvent;
using UnityEngine;

namespace Flicker.Module.Buff.PropBuff
{
    /// <summary>
    /// 脉冲发条：沿使用瞬间的移动方向冲刺固定距离。
    /// 冲刺期间无法主动移动也无法被攻击，冲刺结束后移速下降持续数秒
    /// </summary>
    public class PulseClockBuff : BaseBuffStrategy
    {
        /// <summary>
        /// 冲刺总距离
        /// </summary>
        public const float Distance = 3f;
        /// <summary>
        /// 冲刺速度，单位/秒
        /// </summary>
        public const float Speed = 20f;
        /// <summary>
        /// 后段移速倍率
        /// </summary>
        public const float MoveSpeedMultiplier = 0.7f;
        /// <summary>
        /// 后段持续时间
        /// </summary>
        public const float BackDuration = 4f;

        /// <summary>
        /// 剩余冲刺距离
        /// </summary>
        private float remaining;
        /// <summary>
        /// 冲刺方向，1 向上，-1 向下
        /// </summary>
        private readonly float direction;
        /// <summary>
        /// 后段剩余时间，非正数表示尚未进入后段或已结束
        /// </summary>
        float backTimer = 0f;
        GameCharacterController gameCharacterController;
        /// <summary>
        /// 前段：冲刺推进与移动锁
        /// </summary>
        BaseEffectStrategy frontEffect;
        /// <summary>
        /// 后段：移速惩罚
        /// </summary>
        BaseEffectStrategy backEffect;
        public PulseClockBuff(bool isUp)
        {
            name = "脉冲发条";
            path = "PulseClock";
            description = "沿当前方向冲刺3单位距离，期间不可被攻击，随后移速下降30%持续4秒";
            /** 逐帧动作需允许多个实例并存，否则连续使用会被吞掉 */
            isStackable = false;
            this.direction = isUp ? 1f : -1f;
        }
        public override void OnAdd()
        {
            this.remaining = Distance;
            this.gameCharacterController = effectContainer.GetComponent<GameCharacterController>();
            this.frontEffect = new BaseEffectStrategy();
            frontEffect.onAdd = (b) =>
            {
                /** 锁主动移动 */
                b.Listen<BeforeMoveEvent>(OnBeforeMove);
                /** 冲刺期间免疫攻击 */
                b.Listen<BeforeBeAttackedEvent>(OnBeforeBeAttacked);
                /** 逐帧推进冲刺与后段计时 */
                b.Listen<UpdateEvent>(OnUpdate);
            };
            effectContainer.AddEffect(frontEffect, this);
        }
        public override void OnRemove()
        {
            if (frontEffect != null) effectContainer.RemoveEffect(frontEffect);
            if (backEffect != null) effectContainer.RemoveEffect(backEffect);
        }
        private void OnBeforeMove(BeforeMoveEvent beforeMoveEvent)
        {
            beforeMoveEvent.context.cancelled = true;
        }
        private void OnBeforeBeAttacked(BeforeBeAttackedEvent beforeBeAttackedEvent)
        {
            beforeBeAttackedEvent.context.cancelled = true;
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

            /** 前段：逐帧推进，最后一帧不越过剩余距离以保证总位移精确 */
            float step = Mathf.Min(Speed * Time.deltaTime, remaining);
            gameCharacterController.MoveAlongWall(step * direction);
            remaining -= step;
            if (remaining <= 0f) OnDashFinished();
        }
        /// <summary>
        /// 冲刺结束，切换为后段：解除移动锁与免伤，改为移速惩罚并开始计时
        /// </summary>
        private void OnDashFinished()
        {
            effectContainer.RemoveEffect(frontEffect);
            frontEffect = null;

            this.backEffect = new BaseEffectStrategy();
            backEffect.apply = (b, m) => m.Get(ModifierSet.Key.MoveSpeed).AddMultiplier(MoveSpeedMultiplier);
            effectContainer.AddEffect(backEffect);

            backTimer = BackDuration;
        }
    }
}
