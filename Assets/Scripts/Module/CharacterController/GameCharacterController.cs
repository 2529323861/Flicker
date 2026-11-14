using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Flicker.Core;
using Flicker.Module.Buff;
using Flicker.Module.CharacterEvent;
using Flicker.Module.Context;
using Flicker.Module.Network;
using Flicker.Module.UIEvent;
using Flicker.Module.ViewEvent;
using Flicker.View;
using Mirror;
using UnityEngine;

namespace Flicker.Module
{   
    /// <summary>
    /// 游戏角色状态流转常量
    /// </summary>
    /// 
    public enum GameCharacterState
    {
        /// <summary>
        /// 静止
        /// </summary>
        Idle,
        /// <summary>
        /// 移动
        /// </summary>
        Moving,
        /// <summary>
        /// 跳跃滞空
        /// </summary>
        JumpHanging,
        /// <summary>
        /// 弹反滞空
        /// </summary>
        ParryHanging,
        /// <summary>
        /// 死亡
        /// </summary>
        Dead,
    }
    [RequireComponent(typeof(ModifyPipeline))]
    [RequireComponent(typeof(HealthController))]
    [RequireComponent(typeof(MiniEventBus))]
    [RequireComponent(typeof(EffectContainer))]
    [RequireComponent(typeof(BuffContainer))]
    [RequireComponent(typeof(CharacterStats))]
    /// <summary>
    /// 游戏角色行为基类，定义跳跃，跳跃冷却等所有游戏角色都有的基本行为
    /// </summary>
    public class GameCharacterController : NetworkBehaviour 
    {   
        /// <summary>
        /// 跳跃CD变更的委托，供玩家控制器注入
        /// </summary>
        public Action<UpdateJumpCDProgressEvent> onUpdateJumpCDProgress;
        /// <summary>
        /// 死亡回调，参数为死亡的角色自身，供上层结算判定
        /// </summary>
        public Action<GameCharacterController> onDestroy;
        /// <summary>
        /// 移动速度
        /// </summary>
        public float moveSpeed;
        public ModifiableValue moveSpeedMV;

        /// <summary>
        /// 滞空时间
        /// </summary>
        public float hangTime;
        public ModifiableValue hangTimeMV;

        /// <summary>
        /// 最大跳跃冷却
        /// </summary>
        public float maxJumpingCD;
        public ModifiableValue maxJumpingCDMV;

        /// <summary>
        /// 跳跃冷却速度
        /// </summary>
        public float jumpingCDRate;
        public ModifiableValue jumpingCDRateMV;

        /// <summary>
        /// 攻击伤害
        /// </summary>
        public int damage;
        public ModifiableValue damageMV;

        /// <summary>
        /// 僵直时间
        /// </summary>
        public float stiffenTime;
        public ModifiableValue stiffenTimeMV;

        /// <summary>
        /// 碰撞半径
        /// </summary>
        public float collisionRadius;

        /// <summary>
        /// 左边墙的位置
        /// </summary>
        public float leftWallPos;

        /// <summary>
        /// 右边墙的位置
        /// </summary>
        public float rightWallPos;

        /// <summary>
        /// 道具使用特效。跟随角色的子对象，未拖拽时由 Awake 自动查找
        /// </summary>
        public PropVfxPlayer propVfxPlayer;

        /// <summary>
        /// 阵营tag
        /// </summary>
        public string teamTag;

        public bool towardUp;

        /// <summary>
        /// 剩余跳跃冷却比例，0 = 就绪，1 = 刚进入冷却
        /// </summary>
        public float jumpCDRatio;
        /// <summary>
        /// 当前墙面
        /// </summary>
        public bool isRightWall;

        /// <summary>
        /// 跳跃协程
        /// </summary>
        private Coroutine _jumpingCoroutine = null;

        /// <summary>
        /// 游戏角色状态
        /// </summary>
        protected GameCharacterState state = GameCharacterState.Idle;
        public GameCharacterState State { get{return state;} }

        /// <summary>
        /// 这一帧是否发生弹反
        /// </summary>
        private bool parryAtFrame = false;
        /// <summary>
        /// 当前跳跃的上下文，供弹反相位修改
        /// </summary>
        private JumpContext _jumpingContext = null;
        /// <summary>
        /// 本帧待执行的弹反上下文
        /// </summary>
        private ParryContext _parryContext = null;

        private ModifyPipeline modifyPipeline;
        private MiniEventBus miniEventBus;
        private HealthController healthController;
        private List<bool> moveDirectionStack = new();
        private GameCharacterActor gameCharacterActor;

        /// <summary>
        /// 本机应运行逻辑（服务端或单机模式）。客户端空壳为 false。
        /// </summary>
        protected bool _isLogicActive;

        public void GenerateDataPool()
        {
            moveSpeedMV = new(moveSpeed);
            hangTimeMV = new(hangTime);
            maxJumpingCDMV = new(maxJumpingCD);
            damageMV = new(damage);
            jumpingCDRateMV = new(jumpingCDRate);
            stiffenTimeMV = new(stiffenTime);
        }
        protected virtual void Awake()
        {
            //View不受影响
            gameCharacterActor = GetComponent<GameCharacterActor>();

            modifyPipeline = GetComponent<ModifyPipeline>();
            miniEventBus = GetComponent<MiniEventBus>();
            healthController = GetComponent<HealthController>();

            /** 特效挂在子物体上，允许在预制体里不显式拖拽 */
            if (propVfxPlayer == null) propVfxPlayer = GetComponentInChildren<PropVfxPlayer>(true);

            GenerateDataPool();

            /** 冷却比例以"上限"为不变量，上限变化时比例不变，无需观察者做等比缩放 */
            jumpCDRatio = Ratio.Empty;

            modifyPipeline.modifierSet.Register(ModifierSet.Key.MoveSpeed, moveSpeedMV);
            modifyPipeline.modifierSet.Register(ModifierSet.Key.HangTime, hangTimeMV);
            modifyPipeline.modifierSet.Register(ModifierSet.Key.MaxJumpingCD, maxJumpingCDMV);
            modifyPipeline.modifierSet.Register(ModifierSet.Key.JumpingCDRate, jumpingCDRateMV);
            modifyPipeline.modifierSet.Register(ModifierSet.Key.Damage, damageMV);
            modifyPipeline.modifierSet.Register(ModifierSet.Key.StiffenTime, stiffenTimeMV);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.localPosition,collisionRadius);
        }
        /// <summary>
        /// 子类重写时必须先调用base
        /// </summary>
        protected virtual void Update()
        {   
            if (!isServer) return;  // 客户端空壳：逻辑空转
            if (((CustomNetworkManager)CustomNetworkManager.singleton).IsLogicDisabled) return;  //暂停/剧情/倒数
            /** 维护跳跃CD */
            if (jumpCDRatio > 0)
            {
                float jumpingCDRate = modifyPipeline.GetModifierSet().Final(ModifierSet.Key.JumpingCDRate);
                jumpCDRatio = Ratio.Advance(jumpCDRatio, maxJumpingCDMV.FinalValue, -Time.deltaTime * jumpingCDRate);
            }
            onUpdateJumpCDProgress?.Invoke(new(){jumpCDProgress=jumpCDRatio});

            miniEventBus.Publish<UpdateEvent>(new UpdateEvent());

            /** 维护移动标识 */
            UpdateMove();
        }
        /// <summary>
        /// 跳跃。受状态与跳跃CD限制
        /// </summary>
        public void Jump() => PerformJump(ignoreCD: false);
        /// <summary>
        /// 强制跳跃。不受跳跃CD限制
        /// </summary>
        public void ForceJump() => PerformJump(ignoreCD: true);
        /// <summary>
        /// 跳跃执行
        /// </summary>
        /// <param name="ignoreCD">是否无视跳跃冷却</param>
        private void PerformJump(bool ignoreCD)
        {
            if (((CustomNetworkManager)CustomNetworkManager.singleton).IsLogicDisabled) return;  //暂停/剧情/倒数
            if (state != GameCharacterState.Idle && state != GameCharacterState.Moving) return;
            if (!ignoreCD && jumpCDRatio > 0) return;

            ModifierSet attributes = modifyPipeline.GetModifierSet();
            JumpContext context = new()
            {
                attributes = attributes,
                toRightWall = !isRightWall,
                /** 起跳时快照滞空时长，此后只读；attributes 是共享实例会被重算 */
                hangTime = attributes.Final(ModifierSet.Key.HangTime)
            };

            /** 相位一：决定本次跳跃是否发生 */
            miniEventBus.Publish<BeforeJumpEvent>(new BeforeJumpEvent{context=context});
            if (context.cancelled) return;  //事件被取消

            /** 起跳已确定发生，此处广播音效。必须放在取消判定之后，
             *  否则被拦下的跳跃也会响 */
            ((CustomNetworkManager)CustomNetworkManager.singleton).EventBroadcast(new PlaySfxEvent(){key="PlayerJump",volumeScale=1f});

            /** 相位二：修饰本次跳跃的行为参数 */
            miniEventBus.Publish<JumpResultEvent>(new JumpResultEvent{context=context});

            /** 相位三：按上下文执行。
             *  状态闸门已保证不会在跳跃进行中重入，此分支当前不可达，
             *  保留以备后续二段跳等"可空中起跳"的改动 */
            if (_jumpingCoroutine != null)
            {
                StopCoroutine(_jumpingCoroutine);
            }
            _jumpingContext = context;
            _jumpingCoroutine = StartCoroutine(JumpingCoroutine(context));
        }
        /// <summary>
        /// 供Jump方法使用的跳跃协程
        /// </summary>
        /// <returns></returns>
        IEnumerator JumpingCoroutine(JumpContext context)
        {   
            
            
            SetState(GameCharacterState.JumpHanging);

            /** 视图脏检查：精灵当前显示的墙。飞行期间 toRightWall 可能被效果改写，
             *  此处逐帧比对并按需刷新朝向 */
            bool renderedWall = isRightWall;

            while (context.progress <= 1f)
            {
                yield return null;

                if (((CustomNetworkManager)CustomNetworkManager.singleton).IsLogicDisabled) continue;  //暂停/剧情/倒数
                context.progress += Time.deltaTime / context.hangTime;

                /** 弹反检测 */
                if (state == GameCharacterState.JumpHanging)
                    ParryDetect(); 
                if (parryAtFrame)
                {
                    parryAtFrame = false;                      
                    /** 复位触发，切换状态，无条件折返回出发墙。
                     *  不偏矢等"不换向"效果已在 Result 相位提前折返一次，两次调用相互抵消 */
                    SetState(GameCharacterState.ParryHanging);
                    context.FoldBack();

                    ((CustomNetworkManager)CustomNetworkManager.singleton).EventBroadcast(new AddVibration(){maxTime=0.4f,amplitudeX=0.8f,amplitudeY=0f,amplitudeZ=0f});
                    ((CustomNetworkManager)CustomNetworkManager.singleton).EventBroadcast(new PlaySfxEvent(){key="PlayerParry",volumeScale=1f});
                    /** 弹反比受击更重：更亮、更慢，顿帧也更长 */
                    ((CustomNetworkManager)CustomNetworkManager.singleton).EventBroadcast(new HitStopEvent(){duration=0.1f});
                    ((CustomNetworkManager)CustomNetworkManager.singleton).EventBroadcast(new PlayScreenFlashEvent(){alphaScale=1f,speed=0.85f});

                    miniEventBus.Publish<AfterParryEvent>(new AfterParryEvent{context=_parryContext});
                    _parryContext = null;
                }

                if (context.toRightWall != renderedWall)
                {
                    renderedWall = context.toRightWall;
                    UpdateActor(state,towardUp,renderedWall);
                }

                transform.localPosition = LerpFlightPosition(context);
            }
            transform.localPosition = LandFlightPosition(context);
            /** 进入冷却：比例 = 本次冷却时长 / 当前上限。上限为 0 时视为无冷却 */
            jumpCDRatio = Ratio.ToRatio(context.attributes.Final(ModifierSet.Key.MaxJumpingCD), context.attributes.Final(ModifierSet.Key.MaxJumpingCD));

            /** 攻击结算。命中时发布 AfterAttackEvent，供落地补偿判定 */
            if (state == GameCharacterState.JumpHanging)
            {
                AttackDetect();
            }

            /** 落地事件：此时命中与弹反结果均已确定，
             *  而僵直尚未结算，故可据此改写尚未生效的后果 */
            miniEventBus.Publish<AfterJumpEvent>(new AfterJumpEvent{context=context});

            /** 僵直结算 */
            if (state == GameCharacterState.ParryHanging)
            {
                Stiffen();
            }

            /** 落点墙即上下文中的终点墙，折返已将其调转 */
            isRightWall = context.toRightWall;

            SetState(GameCharacterState.Idle);
            MoveRecovery();
            ((CustomNetworkManager)CustomNetworkManager.singleton).EventBroadcast(new AddVibration(){maxTime=0.2f,amplitudeX=0.2f,amplitudeY=0f,amplitudeZ=0f});
            ((CustomNetworkManager)CustomNetworkManager.singleton).EventBroadcast(new PlaySfxEvent(){key="PlayerLand",volumeScale=1f});

            /** 落地：本次跳跃全部结算已完毕，仅作必然发生的通知 */
            miniEventBus.Publish<LandEvent>(new LandEvent());
        }
        /// <summary>
        /// 僵直
        /// </summary>
        private void Stiffen()
        {
            StiffenContext context = new()
            {
                attributes = modifyPipeline.GetModifierSet()
            };

            /** 相位一：决定本次僵直是否发生 */
            miniEventBus.Publish<BeforeStiffenEvent>(new BeforeStiffenEvent{context=context});
            if (context.cancelled) return;

            /** 相位二：确定本次僵直的临时变量 */
            miniEventBus.Publish<StiffenResultEvent>(new StiffenResultEvent{context=context});

            /** 相位三：按结果执行 */
            gameObject.GetComponent<BuffContainer>().AddBuff(new StiffnessBuff(),context.attributes.Final(ModifierSet.Key.StiffenTime));

            miniEventBus.Publish<AfterStiffenEvent>(new AfterStiffenEvent{context=context});
        }
        public void EnterMove(bool isUp)
        {
            moveDirectionStack.Add(isUp);
            if (!(state == GameCharacterState.Idle || state == GameCharacterState.Moving)) return;
            towardUp = moveDirectionStack[moveDirectionStack.Count - 1];
            SetState(GameCharacterState.Moving);
        }
        public void ExitMove(bool isUp)
        {
            moveDirectionStack.Remove(isUp);
            if (!(state == GameCharacterState.Idle || state == GameCharacterState.Moving)) return;
            if (moveDirectionStack.Count == 0)
            {                
                SetState(GameCharacterState.Idle);
            }
            else
            {
                towardUp = moveDirectionStack[moveDirectionStack.Count - 1];
                SetState(GameCharacterState.Moving);
            }
        }
        /// <summary>
        /// 移动恢复，供跳跃打断移动后调用，回复移动状态
        /// </summary>
        private void MoveRecovery()
        {
            if (moveDirectionStack.Count==0) return;
            towardUp = moveDirectionStack[moveDirectionStack.Count - 1];
            SetState(GameCharacterState.Moving);
        }
        private void UpdateMove()
        {
            if (moveDirectionStack.Count==0) return;
            if (!(state == GameCharacterState.Idle || state == GameCharacterState.Moving)) return;

            MoveContext context = new()
            {
                attributes = modifyPipeline.GetModifierSet(),
                isUp = moveDirectionStack[moveDirectionStack.Count-1]
            };

            /** 相位一：决定本次移动是否发生 */
            miniEventBus.Publish<BeforeMoveEvent>(new BeforeMoveEvent{context=context});
            if (context.cancelled) return;

            /** 相位二：修饰本次移动的行为参数 */
            miniEventBus.Publish<MoveResultEvent>(new MoveResultEvent{context=context});

            /** 相位三：按上下文执行 */
            bool isUp = context.isUp;
            towardUp = isUp;

            float moveSpeed = context.attributes.Final(ModifierSet.Key.MoveSpeed);

            MoveAlongWall(moveSpeed * Time.deltaTime * (isUp ? 1 : -1));

            miniEventBus.Publish<AfterMoveEvent>(new AfterMoveEvent{context=context});
        }
        /// <summary>
        /// 飞行插值位置。只接管横向（x），纵向（y）留给移动与冲刺。
        /// 起点墙由终点墙推出，二者必然相反
        /// </summary>
        /// <param name="context">跳跃上下文</param>
        private Vector3 LerpFlightPosition(JumpContext context)
        {
            float fromX = context.toRightWall ? leftWallPos : rightWallPos;
            float toX = context.toRightWall ? rightWallPos : leftWallPos;

            Vector3 position = transform.localPosition;
            position.x = Mathf.Lerp(fromX,toX,Mathf.Clamp01(context.progress));
            return position;
        }
        /// <summary>
        /// 飞行落点位置。同 LerpFlightPosition，只接管横向
        /// </summary>
        /// <param name="context">跳跃上下文</param>
        private Vector3 LandFlightPosition(JumpContext context)
        {
            Vector3 position = transform.localPosition;
            position.x = context.toRightWall ? rightWallPos : leftWallPos;
            return position;
        }
        /// <summary>
        /// 沿当前墙面移动指定距离，处理边界限制与碰壁回调
        /// </summary>
        /// <param name="deltaY">纵向位移，正数向上</param>
        public void MoveAlongWall(float deltaY)
        {
            Vector3 position = transform.localPosition;
            float targetY = position.y+deltaY;
            if (!isRightWall)
            {
                if (targetY > LevelManager.Instance.leftTop) OnIntoEdge(LevelEdge.LeftTop);
                if (targetY < LevelManager.Instance.leftBottom) OnIntoEdge(LevelEdge.LeftBottom);
                targetY = Mathf.Clamp(targetY,LevelManager.Instance.leftBottom,LevelManager.Instance.leftTop);
            }
            else
            {
                if (targetY > LevelManager.Instance.rightTop) OnIntoEdge(LevelEdge.RightTop);
                if (targetY < LevelManager.Instance.rightBottom) OnIntoEdge(LevelEdge.RightBottom);
                targetY = Mathf.Clamp(targetY,LevelManager.Instance.rightBottom,LevelManager.Instance.rightTop);
            }
            transform.localPosition = new Vector3(position.x,targetY,position.z);
        }
        /// <summary>
        /// 到达边缘
        /// </summary>
        protected virtual void OnIntoEdge(LevelEdge levelEdge){}
        /// <summary>
        /// 侦测缓冲。OverlapCircleNonAlloc 复用同一块内存，避免每帧产生垃圾。
        /// 容量 8 足够：同屏最多两名对手，其余是墙体等非角色碰撞体
        /// </summary>
        private readonly Collider2D[] detectBuffer = new Collider2D[8];
        /// <summary>
        /// 弹反侦测：飞行途中与迎面跳跃的敌人相撞。
        ///
        /// 用重叠查询而非射线：角色是圆形碰撞体，判定本质就是"两个圆是否相交"，
        /// 射线只能沿单一方向扫，对纵向偏移和相切情形都会漏判。
        /// </summary>
        private void ParryDetect()
        {
            /** 查询半径取 2 倍自身半径，覆盖"刚接触"的相切情形；
             *  精确接触判定仍由下面的圆心距比较负责，此处只是放宽搜索范围 */
            int count = Physics2D.OverlapCircleNonAlloc(
                transform.position, collisionRadius * 2f, detectBuffer);

            for (int i = 0; i < count; i++)
            {
                Collider2D hit = detectBuffer[i];
                /** 自身碰撞体也在查询结果里 */
                if (hit.gameObject == gameObject) continue;
                if (!hit.TryGetComponent<GameCharacterController>(out var opponent)) continue;  //是游戏角色
                if (opponent.teamTag == teamTag) continue;                                      //与自己不是一个阵营的
                if (opponent.state != GameCharacterState.JumpHanging) continue;                 //处于跳跃状态
                if (opponent.isRightWall == isRightWall) continue;                              //相向移动
                if ((opponent.transform.localPosition - transform.localPosition).magnitude
                    > collisionRadius + opponent.collisionRadius) continue;                     //碰撞

                opponent.Parry(gameObject);
                Parry(opponent.gameObject);
            }
        }
        /// <summary>
        /// 弹反
        /// </summary>
        public void Parry(GameObject opponent)
        {
            if (state != GameCharacterState.JumpHanging) return;

            ParryContext context = new()
            {
                attributes = modifyPipeline.GetModifierSet(),
                opponent = opponent,
                jump = _jumpingContext
            };

            /** 相位一：决定本次弹反是否发生 */
            miniEventBus.Publish<BeforeParryEvent>(new BeforeParryEvent{context=context});
            if (context.cancelled) return;  //事件是否被取消

            /** 相位二：修饰本次弹反的行为参数，可改写发起方跳跃的上下文 */
            miniEventBus.Publish<ParryResultEvent>(new ParryResultEvent{context=context});

            /** 相位三：标记本帧弹反，由跳跃协程执行并发布 AfterParryEvent */
            _parryContext = context;
            parryAtFrame = true;
        }

        /// <summary>
        /// 攻击侦测：跳跃落地瞬间，与站在同一面墙上的对手碰撞。
        ///
        /// 与弹反侦测同样改用重叠查询。原先的水平射线在此处必然落空：
        /// 落地后双方都在同一面墙、x 相同，只有 y 有差，水平方向扫不到任何东西。
        /// </summary>
        private void AttackDetect()
        {
            int count = Physics2D.OverlapCircleNonAlloc(
                transform.position, collisionRadius * 2f, detectBuffer);

            /** 属性整个侦测只取一次，循环内不重复结算修饰 */
            ModifierSet attributes = modifyPipeline.GetModifierSet();
            for (int i = 0; i < count; i++)
            {
                Collider2D hit = detectBuffer[i];
                /** 自身碰撞体也在查询结果里 */
                if (hit.gameObject == gameObject) continue;
                if (!hit.TryGetComponent<GameCharacterController>(out var victim)) continue;   //是游戏角色
                if (victim.teamTag == teamTag) continue;                                       //与自己不是一个阵营的
                if (victim.state == GameCharacterState.JumpHanging) continue;                  //对方跳走了
                if (victim.state == GameCharacterState.ParryHanging) continue;                 //对方正在弹反滞空
                if ((victim.transform.localPosition - transform.localPosition).magnitude
                    > collisionRadius + victim.collisionRadius) continue;                       //碰撞

                AttackContext context = new()
                {
                    attributes = attributes,
                    victim = victim.gameObject,
                    damage = new ModifiableValue(attributes.Final(ModifierSet.Key.Damage))
                };

                /** 相位一：决定本次攻击是否发生 */
                miniEventBus.Publish<BeforeAttackEvent>(new BeforeAttackEvent{context=context});
                if (context.cancelled) continue;  //事件是否被取消

                /** 相位二：确定本次攻击的伤害 */
                miniEventBus.Publish<AttackResultEvent>(new AttackResultEvent{context=context});

                /** 相位三：按结果执行 */
                victim.BeAttacked(gameObject,context.damage.FinalValue);

                miniEventBus.Publish<AfterAttackEvent>(new AfterAttackEvent{context=context});
            }
            
        }
        /// <summary>
        /// 受击
        /// </summary>
        /// <param name="attacker">攻击者</param>
        /// <param name="damage">攻击方传入的伤害</param>
        public void BeAttacked(GameObject attacker, float damage)
        {
            BeAttackedContext context = new()
            {
                attributes = modifyPipeline.GetModifierSet(),
                attacker = attacker,
                damage = new ModifiableValue(damage)
            };

            /** 相位一：决定本次受击是否发生 */
            miniEventBus.Publish<BeforeBeAttackedEvent>(new BeforeBeAttackedEvent{context=context});
            if (context.cancelled) return;  //事件是否被取消

            /** 相位二：确定本次承受的伤害 */
            miniEventBus.Publish<BeAttackedResultEvent>(new BeAttackedResultEvent{context=context});

            /** 相位三：按结果执行。伤害归零（被盾抵消等）不闪，避免没有伤害却有反馈 */
            int finalDamage = (int)context.damage.FinalValue;
            if (finalDamage > 0)
            {
                ((CustomNetworkManager)CustomNetworkManager.singleton).EventBroadcast(new HitStopEvent(){duration=0.2f});
                ((CustomNetworkManager)CustomNetworkManager.singleton).EventBroadcast(new PlayScreenFlashEvent(){alphaScale=0.7f,speed=1.1f});
            }

            healthController.ReduceHealth(finalDamage);
            miniEventBus.Publish<AfterBeAttackedEvent>(new AfterBeAttackedEvent{context=context});

            if (healthController.health <= 0) Die();
        }
        /// <summary>
        /// 死亡
        /// </summary>
        public void Die()
        {
            DieContext context = new()
            {
                attributes = modifyPipeline.GetModifierSet()
            };

            /** 相位一：决定本次死亡是否发生 */
            miniEventBus.Publish<BeforeDie>(new BeforeDie{context=context});
            if (context.cancelled) return;

            /** 相位二：确定本次死亡的临时变量 */
            miniEventBus.Publish<DieResultEvent>(new DieResultEvent{context=context});

            /** 相位三：按结果执行 */
            SetState(GameCharacterState.Dead);

            ((CustomNetworkManager)CustomNetworkManager.singleton).EventBroadcast(new PlaySfxEvent(){key="PlayerDead",volumeScale=1f});

            miniEventBus.Publish<AfterDie>(new AfterDie{context=context});
            onDestroy?.Invoke(this);
        }

        private void SetState(GameCharacterState newState)
        {
            state = newState;
            UpdateActor(state,towardUp,isRightWall);
        }
        public void SetPosition(float y, bool isRightWall)
        {
            this.isRightWall = isRightWall;
            transform.localPosition = new(isRightWall?rightWallPos:leftWallPos,y,0);
            UpdateActor(state,towardUp,isRightWall);
        }
        [ClientRpc]
        public void UpdateActor(GameCharacterState state, bool towardUp, bool isRightWall){
            gameCharacterActor.ChangeAnimationStateTo(state);
            gameCharacterActor.ChangeTowardTo(towardUp);
            gameCharacterActor.ChangeWallTo(isRightWall);
        }
        /// <summary>
        /// 设置阵营颜色。与 SetPlayerName 同一模式：ClientRpc 会回灌到 Host 本地，
        /// 因此服务端不需要额外应用一次
        /// </summary>
        [ClientRpc]
        public void SetTeamColor(Color color)
        {
            gameCharacterActor.SetColor(color);
        }

        /// <summary>
        /// 播放道具使用特效。道具效果本来就双方可见，
        /// 因此走 ClientRpc 让所有客户端都看到同一个人的特效。
        /// 与 SetTeamColor 同一模式，Host 本地会被回灌，无需额外调用
        /// </summary>
        /// <param name="propPath">道具图标文件名，对应 PropIcon 目录</param>
        [ClientRpc]
        public void PlayPropVfx(string propPath)
        {
            if (propVfxPlayer != null) propVfxPlayer.Play(propPath);
        }
    }
}