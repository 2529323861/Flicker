using Flicker.Module.CharacterEvent;
using UnityEngine;

namespace Flicker.Module.Buff.PropBuff
{
    /// <summary>
    /// 冲刺：沿使用瞬间的移动方向冲刺固定距离。
    /// 冲刺期间无法主动移动，但可以跳跃、弹反、攻击；不可被打断
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
        /// 剩余距离
        /// </summary>
        private float remaining;
        /// <summary>
        /// 冲刺方向，1 向上，-1 向下
        /// </summary>
        private readonly float direction;
        GameCharacterController gameCharacterController;
        BaseEffectStrategy baseEffectStrategy;
        public PulseClockBuff(bool isUp)
        {
            name = "脉冲发条";
            /** 暂无美术资源，补齐 BuffIcon/PulseClock.png 后改为 "PulseClock" */
            path = "Unknown";
            description = "沿当前方向冲刺3单位距离";
            /** 逐帧动作需允许多个实例并存，否则连续使用会被吞掉 */
            isStackable = false;
            this.direction = isUp ? 1f : -1f;
        }
        public override void OnAdd()
        {
            this.remaining = Distance;
            this.gameCharacterController = effectContainer.GetComponent<GameCharacterController>();
            this.baseEffectStrategy = new BaseEffectStrategy();
            baseEffectStrategy.onAdd = (b) =>
            {
                /** 锁主动移动 */
                b.Listen<BeforeMoveEvent>(OnBeforeMove);
                /** 逐帧推进冲刺 */
                b.Listen<UpdateEvent>(OnUpdate);
            };
            effectContainer.AddEffect(baseEffectStrategy, this);
        }
        public override void OnRemove()
        {
            effectContainer.RemoveEffect(baseEffectStrategy);
        }
        private void OnBeforeMove(BeforeMoveEvent beforeMoveEvent)
        {
            beforeMoveEvent.context.cancelled = true;
        }
        private void OnUpdate(UpdateEvent updateEvent)
        {
            /** 最后一帧不越过剩余距离，保证总位移精确 */
            float step = Mathf.Min(Speed * Time.deltaTime, remaining);
            gameCharacterController.MoveAlongWall(step * direction);
            remaining -= step;
            if (remaining <= 0f)
            {
                buffContainer.RemoveBuff(this);
            }
        }
    }
}
