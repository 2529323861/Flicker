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
using Mirror.BouncyCastle.Asn1.Crmf;
using Mirror.Transports.Encryption;
using Unity.VisualScripting;
using UnityEditor;
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
    /// <summary>
    /// 游戏角色行为基类，定义跳跃，跳跃冷却等所有游戏角色都有的基本行为
    /// </summary>
    public class GameCharacterController : NetworkBehaviour 
    {   
        /// <summary>
        /// 跳跃CD变更的委托，供玩家控制器注入
        /// </summary>
        public Action<UpdateJumpCDProgressEvent> onUpdateJumpCDProgress;
        public Action onDestroy;
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
                ignoreCD = ignoreCD
            };

            /** 相位一：决定本次跳跃是否发生 */
            miniEventBus.Publish<BeforeJumpEvent>(new BeforeJumpEvent{context=context});
            if (context.cancelled) return;  //事件被取消

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
            
            
            float hangTime = context.attributes.Final(ModifierSet.Key.HangTime);

            SetState(GameCharacterState.JumpHanging);
            float timer = 0;


            Vector3 startingPoint = transform.localPosition;
            Vector3 distination = startingPoint;
            distination.x = context.toRightWall ? rightWallPos : leftWallPos;
            while (timer <= hangTime)
            {
                yield return null;

                if (((CustomNetworkManager)CustomNetworkManager.singleton).IsLogicDisabled) continue;  //暂停/剧情/倒数
                timer += Time.deltaTime;

                /** 弹反检测 */
                if (state == GameCharacterState.JumpHanging)
                    ParryDetect(); 
                if (parryAtFrame)
                {
                    parryAtFrame = false;                      
                    /** 复位触发，切换状态。若未被取消反向，则倒置计时器并交换移动终/起点 */
                    SetState(GameCharacterState.ParryHanging);
                    if (context.reverse)
                    {
                        timer = hangTime - timer;
                        Vector2 temp = startingPoint;
                        startingPoint = distination;
                        distination = temp;
                    }

                    ((CustomNetworkManager)CustomNetworkManager.singleton).EventBroadcast(new AddVibration(){maxTime=0.4f,amplitudeX=0.8f,amplitudeY=0f,amplitudeZ=0f});

                    miniEventBus.Publish<AfterParryEvent>(new AfterParryEvent{context=_parryContext});
                    _parryContext = null;
                }
                transform.localPosition = Vector3.Lerp(startingPoint,distination,timer/hangTime);
            }
            transform.localPosition = distination;
            /** 进入冷却：比例 = 本次冷却时长 / 当前上限。上限为 0 时视为无冷却 */
            jumpCDRatio = Ratio.ToRatio(context.attributes.Final(ModifierSet.Key.MaxJumpingCD), context.attributes.Final(ModifierSet.Key.MaxJumpingCD));

            miniEventBus.Publish<AfterJumpEvent>(new AfterJumpEvent{context=context});

            if (state == GameCharacterState.JumpHanging)
            {
                AttackDetect();
                isRightWall = context.toRightWall;
            }
            else
            {
                float stiffenTime = modifyPipeline.GetModifierSet().Final(ModifierSet.Key.StiffenTime);
                gameObject.GetComponent<BuffContainer>().AddBuff(new StiffnessBuff(),stiffenTime);
            }
            SetState(GameCharacterState.Idle);
            MoveRecovery();
            ((CustomNetworkManager)CustomNetworkManager.singleton).EventBroadcast(new AddVibration(){maxTime=0.2f,amplitudeX=0.2f,amplitudeY=0f,amplitudeZ=0f});
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

            float deltaY = moveSpeed * Time.deltaTime * (isUp ? 1 : -1);
            Vector3 temp = transform.localPosition;
            float targetY = temp.y+deltaY;
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
            transform.localPosition = new Vector3(temp.x,targetY,temp.z);

            miniEventBus.Publish<AfterMoveEvent>(new AfterMoveEvent{context=context});
        }
        /// <summary>
        /// 到达边缘
        /// </summary>
        protected virtual void OnIntoEdge(LevelEdge levelEdge){}
        /// <summary>
        /// 弹反侦测
        /// </summary>
        private void ParryDetect()
        {
            Vector2 centerStart = new Vector2(transform.position.x,transform.position.y);
            Vector2 topStart = new Vector2(transform.position.x,transform.position.y+collisionRadius);
            Vector2 bottomStart = new Vector2(transform.position.x,transform.position.y-collisionRadius);

            RaycastHit2D[] centerHit =  Physics2D.RaycastAll(centerStart,new Vector2(isRightWall ? -1 : 1,0),collisionRadius);
            RaycastHit2D[] topHit =  Physics2D.RaycastAll(topStart,new Vector2(isRightWall ? -1 : 1,0),collisionRadius);
            RaycastHit2D[] bottomHit =  Physics2D.RaycastAll(bottomStart,new Vector2(isRightWall ? -1 : 1,0),collisionRadius);

            RaycastHit2D[] raycastHit2Ds = topHit.Concat(bottomHit).Concat(centerHit).ToArray();
            foreach (var i in raycastHit2Ds)
            {
                if (i.collider.gameObject.TryGetComponent<GameCharacterController>(out var gameCharacterController) && //是游戏角色
                    gameCharacterController.teamTag != teamTag && //与自己不是一个阵营的
                    gameCharacterController.state == GameCharacterState.JumpHanging && //处于跳跃状态
                    (i.collider.gameObject.transform.localPosition - transform.localPosition).magnitude <= collisionRadius + gameCharacterController.collisionRadius && //碰撞
                    gameCharacterController.isRightWall != isRightWall //相向移动
                    )
                {
                    gameCharacterController.Parry(gameObject);
                    Parry(i.collider.gameObject);
                }
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
        /// 攻击侦测
        /// </summary>
        private void AttackDetect()
        {
            Vector2 centerStart = new Vector2(transform.position.x,transform.position.y);
            Vector2 topStart = new Vector2(transform.position.x,transform.position.y+collisionRadius);
            Vector2 bottomStart = new Vector2(transform.position.x,transform.position.y-collisionRadius);

            RaycastHit2D[] centerHit =  Physics2D.RaycastAll(centerStart,new Vector2(isRightWall ? -1 : 1,0),collisionRadius);
            RaycastHit2D[] topHit =  Physics2D.RaycastAll(topStart,new Vector2(isRightWall ? -1 : 1,0),collisionRadius);
            RaycastHit2D[] bottomHit =  Physics2D.RaycastAll(bottomStart,new Vector2(isRightWall ? -1 : 1,0),collisionRadius);

            RaycastHit2D[] raycastHit2Ds = topHit.Concat(bottomHit).Concat(centerHit).ToArray();
            List<GameObject> attacked = new();
            /** 属性整个侦测只取一次，循环内不重复结算修饰 */
            ModifierSet attributes = modifyPipeline.GetModifierSet();
            foreach (var i in raycastHit2Ds)
            {
                if (i.collider.gameObject.TryGetComponent<GameCharacterController>(out var gameCharacterController) && //是游戏角色
                    gameCharacterController.teamTag != teamTag && //与自己不是一个阵营的
                    gameCharacterController.state != GameCharacterState.JumpHanging && //不处于跳跃滞空
                    gameCharacterController.state != GameCharacterState.ParryHanging && //不处于弹反滞空
                    !attacked.Contains(i.collider.gameObject) && //没有计算过伤害
                    (i.collider.gameObject.transform.localPosition - transform.localPosition).magnitude <= collisionRadius + gameCharacterController.collisionRadius //碰撞
                    )
                {
                    AttackContext context = new()
                    {
                        attributes = attributes,
                        victim = i.collider.gameObject,
                        damage = new ModifiableValue(attributes.Final(ModifierSet.Key.Damage))
                    };

                    /** 相位一：决定本次攻击是否发生 */
                    miniEventBus.Publish<BeforeAttackEvent>(new BeforeAttackEvent{context=context});
                    if (context.cancelled) continue;  //事件是否被取消

                    /** 相位二：确定本次攻击的伤害 */
                    miniEventBus.Publish<AttackResultEvent>(new AttackResultEvent{context=context});

                    /** 相位三：按结果执行 */
                    gameCharacterController.BeAttacked(gameObject,context.damage.FinalValue);
                    attacked.Add(i.collider.gameObject);

                    miniEventBus.Publish<AfterAttackEvent>(new AfterAttackEvent{context=context});
                }
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

            /** 相位三：按结果执行 */
            healthController.ReduceHealth((int)context.damage.FinalValue);
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

            miniEventBus.Publish<AfterDie>(new AfterDie{context=context});
            onDestroy?.Invoke();
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
    }
}