using System.Collections.Generic;
using UnityEngine;
using Flicker.Module;
using Flicker.Module.Buff;
using Flicker.Core;
using Flicker.Module.Network;
using Flicker.Module.UIEvent;
using TMPro;
using Mirror;

namespace Flicker.Module
{   
    /// <summary>
    /// 人机控制器。作为服务端权威逻辑运行，不向 UI 发布任何事件。
    ///
    /// 每帧固定顺序：
    ///   选目标 → 道具决策（用当前阶段的意图）→ base.Update（冷却推进与移动）→ 当前阶段 OnUpdate
    /// 道具决策排在 base.Update 之前，是为了让「锁移动类」道具在当帧生效。
    ///
    /// 状态机采用状态对象而非 switch 分发：
    /// 每个阶段的进入、逐帧、退出各自成方法，一次性副作用写在 OnEnter / OnExit，
    /// 不会因为「上一帧在哪个阶段」而需要重复判断
    /// </summary>
    public class MockController : GameCharacterController
    {
        public TMP_Text playerName;

        private GameObject target;
        private PropContainer propContainer;
        private BuffContainer buffContainer;

        /// <summary>当前阶段。只描述 AI 想做什么，与 GameCharacterState 互不干扰</summary>
        private MockStateBase currentState;
        private MockPhase currentPhase;
        /// <summary>阶段实例缓存，避免每帧分配</summary>
        private readonly Dictionary<MockPhase, MockStateBase> states = new();

        /// <summary>
        /// 当前主动施加的移动方向。
        /// null 表示未主动移动。用它避免每帧重复 EnterMove 造成的 SetState 抖动
        /// </summary>
        private bool? moveIntent = null;

        [Header("道具决策")]
        [Tooltip("两次道具决策之间的最小间隔。只需略大于一帧，避免同一帧内重复决策；" +
                 "拉长会错过对手滞空这类短窗口")]
        public float propDecisionInterval = 0.2f;

        /// <summary>道具决策节流剩余时间</summary>
        private float propDecisionCooldown = 0f;

        //—————————————————————————————————状态对外接口————————————————————————————————————
        /// <summary>当前目标，可能为 null</summary>
        public GameCharacterController Target =>
            target != null ? target.GetComponent<GameCharacterController>() : null;

        /// <summary>目标相对自身的纵向距离，正数表示目标在上方</summary>
        public float DeltaY => Target != null
            ? Target.transform.localPosition.y - transform.localPosition.y
            : 0f;

        /// <summary>跳跃是否已就绪</summary>
        public bool IsJumpReady => jumpCDRatio <= Ratio.Empty;

        /// <summary>是否被已知效果禁止跳跃，见 MockJumpBlockPolicy</summary>
        public bool IsJumpBlocked => MockJumpBlockPolicy.IsBlocked(buffContainer);

        /// <summary>
        /// 是否可以跳跃：冷却就绪且未被禁止。
        /// 这是 AI 最基础的能力判定，攻击与换墙都建立在它之上
        /// </summary>
        public bool CanJump => IsJumpReady && !IsJumpBlocked;

        /// <summary>是否与目标处于同一面墙。同墙时无法命中对方，需要换墙才能进攻</summary>
        public bool IsSameWall => Target != null && Target.isRightWall == isRightWall;

        /// <summary>纵向距离是否已进入命中范围。与 AttackDetect 的碰撞条件一致</summary>
        public bool IsDistanceEnough => Target != null
            && Mathf.Abs(DeltaY) <= collisionRadius + Target.collisionRadius;

        /// <summary>
        /// 立即遍历并使用全部满足该意图的道具，忽略节流。
        /// 一个动作窗口内可能叠加多个道具（例如同时开两个修饰本次跳跃的），
        /// 因此这里会用尽全部候选，而不是只挑一个
        /// </summary>
        public void UsePropNow(PropIntent wanted)
        {
            UseProps(wanted, useAll: true);

            /** 与常规决策共享节流：刚用完不应在下一帧继续决策 */
            propDecisionCooldown = propDecisionInterval;
        }

        /// <summary>切换阶段。同阶段重复调用无效，保证 OnEnter / OnExit 严格成对</summary>
        public void ChangeState(MockPhase phase)
        {
            if (currentState != null && currentPhase == phase) return;

            currentState?.OnExit();
            currentPhase = phase;
            currentState = states[phase];
            currentState.OnEnter();
        }

        protected override void Awake()
        {
            base.Awake();   // GCC 内部已处理 _isLogicActive 守卫
            propContainer = GetComponent<PropContainer>();
            buffContainer = GetComponent<BuffContainer>();
            InjectUIEvent();

            states[MockPhase.Chase] = new MockChaseState();
            states[MockPhase.Attack] = new MockAttackState();
            states[MockPhase.Escape] = new MockEscapeState();
            foreach (var state in states.Values)
            {
                state.Bind(this);
            }
        }
        /// <summary>
        /// 人机不向 UI 发布事件：HUD 只显示本地玩家
        /// </summary>
        public void InjectUIEvent()
        {

        }
        void Start()
        {
            // 生命周期正常走，客户端空壳不做事
        }
        protected override void Update()
        {
            if (!isServer) return;  // 客户端空壳：逻辑空转
            if (((CustomNetworkManager)CustomNetworkManager.singleton).IsLogicDisabled) return;  //暂停/剧情/倒数

            UpdateTarget();

            /** 首次决策前需要有一个阶段：默认进入追击 */
            if (currentState == null) ChangeState(MockPhase.Chase);

            /** 道具决策先于帧更新：锁移动类道具才能在当帧生效 */
            if (Target != null) DecideProp(currentState.Intent);

            /** 冷却推进、UpdateEvent、纵向移动都由基类完成，必须每帧调用 */
            base.Update();

            /** 协程接管期间不干预移动与跳跃 */
            if (State == GameCharacterState.JumpHanging
                || State == GameCharacterState.ParryHanging
                || State == GameCharacterState.Dead) return;

            /** 没有目标时保持静止，但不退出当前阶段，以免目标回来时丢失意图 */
            if (Target == null)
            {
                ReleaseMove();
                return;
            }

            currentState.OnUpdate();
        }
        //—————————————————————————————————目标———————————————————————————————————————————
        /// <summary>
        /// 维护当前目标：目标死亡或消失时重新选最近的敌人
        /// </summary>
        private void UpdateTarget()
        {
            if (target != null)
            {
                GameCharacterController current = target.GetComponent<GameCharacterController>();
                if (current != null && current.State != GameCharacterState.Dead) return;
                target = null;
            }

            GameCharacterController nearest = null;
            float nearestDistance = float.MaxValue;
            foreach (var candidate in CharacterControllerManager.Instance.GetAll())
            {
                if (candidate == this) continue;
                if (candidate.teamTag == teamTag) continue;
                if (candidate.State == GameCharacterState.Dead) continue;

                float distance = Vector3.Distance(candidate.transform.localPosition, transform.localPosition);
                if (distance >= nearestDistance) continue;
                nearestDistance = distance;
                nearest = candidate;
            }
            target = nearest != null ? nearest.gameObject : null;
        }
        //—————————————————————————————————移动———————————————————————————————————————————
        /// <summary>
        /// 朝指定方向移动。方向未变时不重复下发，避免每帧触发 SetState 与视图同步
        /// </summary>
        public void MoveToward(bool isUp)
        {
            if (moveIntent == isUp) return;
            ReleaseMove();
            EnterMove(isUp);
            moveIntent = isUp;
        }
        /// <summary>
        /// 停止主动移动。两个方向一起退订，与既有约定一致，不必关心压栈顺序
        /// </summary>
        public void ReleaseMove()
        {
            if (moveIntent == null) return;
            ExitMove(true);
            ExitMove(false);
            moveIntent = null;
        }
        //—————————————————————————————————道具———————————————————————————————————————————
        /// <summary>
        /// 常规道具决策：按当前阶段意图使用一个道具。
        /// 与 UsePropNow 的区别在于只用一次 —— 这是逐帧维持的路径，
        /// 全用会瞬间耗光冷却
        /// </summary>
        private void DecideProp(PropIntent wanted)
        {
            if (propDecisionCooldown > 0f)
            {
                propDecisionCooldown -= Time.deltaTime;
                return;
            }

            UseProps(wanted, useAll: false);

            /** 无论有没有买成，都进入节流窗口，避免每帧重试 */
            propDecisionCooldown = propDecisionInterval;
        }
        /// <summary>
        /// 遍历携带的道具，满足两级过滤的交给容器使用。
        ///
        /// 两级过滤：意图决定「能不能用」，条件决定「此刻值不值得用」。
        /// 冷却与 CanUse 不在这里判断，交给 UseProp 内部把关，被拒绝时静默跳过。
        ///
        /// 从随机位置开始遍历，避免总是优先用靠前的槽位
        /// </summary>
        /// <param name="wanted">当前阶段希望的意图</param>
        /// <param name="useAll">true = 用尽全部候选，false = 用掉第一个就停</param>
        private void UseProps(PropIntent wanted, bool useAll)
        {
            if (wanted == PropIntent.None) return;

            int start = Random.Range(0, PropContainer.EquippedPropCount);
            for (int k = 0; k < PropContainer.EquippedPropCount; k++)
            {
                int i = (start + k) % PropContainer.EquippedPropCount;
                BasePropStrategy prop = propContainer.GetProp(i);
                if (prop == null) continue;

                /** 第一级：分类标签粗筛 */
                if ((MockPropPolicy.IntentOf(prop.factoryId) & wanted) == 0) continue;
                /** 第二级：道具特判精筛 */
                if (!MockPropPolicy.ConditionOf(prop.factoryId, this)) continue;

                propContainer.UseProp(i);
                if (!useAll) return;
            }
        }
        //—————————————————————————————————其它———————————————————————————————————————————
        protected override void OnIntoEdge(LevelEdge levelEdge)
        {
            LevelManager.Instance.OnPlayerIntoEdge(levelEdge);
        }
        [ClientRpc]
        public void SetPlayerName(string name)
        {
            playerName.text = name;
        }
    }
}