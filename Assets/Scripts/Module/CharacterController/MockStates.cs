using UnityEngine;

namespace Flicker.Module
{
    /// <summary>
    /// 人机决策阶段。
    /// 描述「AI 想做什么」，与 GameCharacterState（角色当前在做什么）是两个层次，
    /// 同一时刻两者可能完全不同：例如阶段为 Attack 而角色状态为 Idle（等跳跃冷却）
    /// </summary>
    public enum MockPhase
    {
        /// <summary>追击：靠近目标并在条件允许时发起攻击</summary>
        Chase,
        /// <summary>攻击：占一次道具时机后起跳</summary>
        Attack,
        /// <summary>逃离：调整位置直到重新具备进攻条件</summary>
        Escape,
    }

    /// <summary>
    /// 人机决策状态基类。
    ///
    /// 每个状态只负责「在当前阶段该做什么」以及「什么条件下交给谁」，
    /// 不持有任何跨帧的临时数据：需要的数据一律经 owner 读取，
    /// 因为角色状态本身就是唯一真值来源。
    /// </summary>
    public abstract class MockStateBase
    {
        /// <summary>所属控制器，由 MockController 在构造时绑定</summary>
        protected MockController owner;

        public void Bind(MockController owner)
        {
            this.owner = owner;
        }

        /// <summary>
        /// 本阶段希望使用的道具意图。
        /// 道具决策层据此挑选道具，状态本身不认识任何具体道具
        /// </summary>
        public abstract PropIntent Intent { get; }

        /// <summary>
        /// 进入阶段。一次性操作（清理移动意图、播放入场表现）写在这里，
        /// 不必在每帧的 OnUpdate 里重复判断
        /// </summary>
        public virtual void OnEnter() { }

        /// <summary>
        /// 阶段内的逐帧决策，只能在角色可自由行动时被调用
        /// </summary>
        public virtual void OnUpdate() { }

        /// <summary>
        /// 离开阶段。与 OnEnter 成对，保证状态切换后不残留上一阶段的副作用
        /// </summary>
        public virtual void OnExit() { }

        //—————————————————————————————————公共判定———————————————————————————————————————————
        /// <summary>
        /// 是否具备进攻条件：能跳、且与目标分处两面墙。
        /// 同墙时跳过去也打不到，因此同墙一律视为不能进攻。
        /// 攻击能力 = 跳跃能力 + 分墙，因此这里直接复用 owner.CanJump
        /// </summary>
        protected bool CanAttack =>
            owner.Target != null
            && owner.CanJump
            && !owner.IsSameWall;
    }

    /// <summary>
    /// 追击：靠近目标。
    /// 一旦失去进攻条件（跳跃冷却中 / 被禁止跳跃 / 与目标同墙）就转入逃离，
    /// 具备条件且距离足够时转入攻击
    /// </summary>
    public class MockChaseState : MockStateBase
    {
        public override PropIntent Intent => PropIntent.Chase;

        public override void OnUpdate()
        {
            if (owner.Target == null) return;

            /** 失去进攻条件 → 逃离。这三种情况的共同点是「现在跳过去也没有意义」 */
            if (!owner.CanJump || owner.IsSameWall)
            {
                owner.ChangeState(MockPhase.Escape);
                return;
            }

            /** 条件齐备且进入命中范围 → 攻击 */
            if (owner.IsDistanceEnough)
            {
                owner.ChangeState(MockPhase.Attack);
                return;
            }

            /** 继续纵向靠近 */
            owner.MoveToward(owner.DeltaY > 0f);
        }

        public override void OnExit()
        {
            /** 交给下一阶段或静止，由接手方决定方向，因此这里一律停住 */
            owner.ReleaseMove();
        }
    }

    /// <summary>
    /// 攻击：先用掉全部 Attack 类道具，再起跳，随后立刻转入逃离。
    /// 本阶段只存活一帧，因此全部逻辑放在 OnEnter
    /// </summary>
    public class MockAttackState : MockStateBase
    {
        public override PropIntent Intent => PropIntent.Attack;

        public override void OnEnter()
        {
            /** 起跳前必须停止主动移动：跳跃结束时 MoveRecovery 会把角色恢复成 Moving，
             *  残留方向会导致边跳边飘、命中判定不稳定 */
            owner.ReleaseMove();

            /** 占一次道具时机，强化本次攻击。
             *  忽略节流：这是本次起跳唯一的机会，不能因为上一帧刚决策过而被跳过。
             *  这里会用尽全部候选，因为这些道具都只修饰「下一次跳跃」，
             *  不同时开就是浪费 */
            owner.UsePropNow(PropIntent.Attack);

            /** Jump 内部仍会自检冷却与状态，条件已由上一阶段保证，属兜底 */
            owner.Jump();

            /** 起跳后交给逃离：落地后重新评估位置与冷却 */
            owner.ChangeState(MockPhase.Escape);
        }
    }

    /// <summary>
    /// 逃离：调整位置，直到重新具备进攻条件。
    ///
    /// 能跳就跳（此时必然是同墙，跳一次即换墙）；
    /// 跳不了则拉开距离，直到跳跃恢复就绪再转回追击
    /// </summary>
    public class MockEscapeState : MockStateBase
    {
        public override PropIntent Intent => PropIntent.Escape;

        public override void OnEnter()
        {
            owner.ReleaseMove();
        }

        public override void OnUpdate()
        {
            if (owner.Target == null) return;

            /** 重新具备进攻条件 → 回追击。
             *  与 MockChaseState 的转入条件互为反面，保证不会来回抖动 */
            if (CanAttack)
            {
                owner.ChangeState(MockPhase.Chase);
                return;
            }

            /** 能跳就去跳。
             *  此处已排除 CanAttack，而 Target 非空，因此能跳等价于同墙；
             *  同墙时跳一次即可换到对面墙，不必再判断一次冷却与禁止 */
            if (owner.CanJump)
            {
                owner.Jump();
                return;
            }

            /** 拉开距离：目标在上方就往下，反之往上 */
            owner.MoveToward(owner.DeltaY <= 0f);
        }

        public override void OnExit()
        {
            owner.ReleaseMove();
        }
    }
}
