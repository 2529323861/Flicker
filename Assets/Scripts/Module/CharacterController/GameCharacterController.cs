using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Flicker.Core;
using Flicker.Core.Service;
using Flicker.Module.Buff;
using Flicker.Module.CharacterEvent;
using Flicker.Module.Context;
using Flicker.Module.UIEvent;
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
    }
    [RequireComponent(typeof(ModifyPipeline))]
    [RequireComponent(typeof(HealthController))]
    [RequireComponent(typeof(MiniEventBus))]
    [RequireComponent(typeof(EffectContainer))]
    [RequireComponent(typeof(BuffContainer))]
    /// <summary>
    /// 游戏角色行为基类，定义跳跃，跳跃冷却等所有游戏角色都有的基本行为
    /// </summary>
    public class GameCharacterController : MonoBehaviour 
    {   
        /// <summary>
        /// 跳跃CD变更的委托，供玩家控制器注入
        /// </summary>
        public Action<UpdateJumpCDProgressEvent> onUpdateJumpCDProgress;
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

        // /// <summary>
        // /// 剩余僵直时间
        // /// </summary>
        // public MutableValue nowStiffTimeMV;
        
        /// <summary>
        /// 剩余跳跃冷却时间
        /// </summary>
        public MutableValue nowJumpingCDMV;
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

        private bool _lastTowardUp = false;

        /// <summary>
        /// 这一帧是否发生弹反
        /// </summary>
        private bool parryAtFrame = false;

        private ModifyPipeline modifyPipeline;
        private MiniEventBus miniEventBus;
        private HealthController healthController;
        private List<bool> moveDirectionStack = new();

        public void GenerateDataPool()
        {
            moveSpeedMV = new(moveSpeed);
            hangTimeMV = new(hangTime);
            maxJumpingCDMV = new(maxJumpingCD);
            damageMV = new(damage);
            jumpingCDRateMV = new(jumpingCDRate);
        }
        protected void Awake()
        {
            SetState(GameCharacterState.Idle);

            modifyPipeline = GetComponent<ModifyPipeline>();
            miniEventBus = GetComponent<MiniEventBus>();
            healthController = GetComponent<HealthController>();

            GenerateDataPool();

            nowJumpingCDMV = new MutableValue(0f);

            /** 对于依赖于属性类数据的瞬时数据进行观察者绑定，实现等比缩放 */
            nowJumpingCDMV.onUpdate = (a, b) =>
            {
                if (a==0) nowJumpingCDMV.value = 0;
                else nowJumpingCDMV.value *= b/a;
            };
            maxJumpingCDMV.AddObserver(nowJumpingCDMV);
            
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.localPosition,collisionRadius);
        }
        /// <summary>
        /// 子类重写时必须先调用base
        /// </summary>
        protected void Update()
        {   
            /** 维护跳跃CD */
            if (nowJumpingCDMV.value > 0)
            {
                ModifierSet values = modifyPipeline.GetModifierSet();
                float jumpingCDRate = values.jumpingCDRate.FinalValue;

                nowJumpingCDMV.value -= Time.deltaTime*jumpingCDRate;
                if (nowJumpingCDMV.value < 0)
                {
                    nowJumpingCDMV.value = 0;
                }
            }
            onUpdateJumpCDProgress?.Invoke(new(){jumpCDProgress=nowJumpingCDMV.value/maxJumpingCDMV.FinalValue});

            miniEventBus.Publish<UpdateEvent>(new UpdateEvent());

            /** 维护移动标识 */
            UpdateMove();
        }
        /// <summary>
        /// 跳跃
        /// </summary>
        public void Jump()
        {
            if (state != GameCharacterState.Idle && state != GameCharacterState.Moving) return;
            if (nowJumpingCDMV.value > 0 ) return;

            ActionContext actionContext = new();
            miniEventBus.Publish<BeforeJumpEvent>(new BeforeJumpEvent{context=actionContext,toRightWall=!isRightWall});
            if (actionContext.cancelled) return;  //事件被取消

            ForceJump(false);
        }
        /// <summary>
        /// 强制跳跃，无视CD，无视状态
        /// </summary>
        public void ForceJump(bool isForce=true)
        {
            if (_jumpingCoroutine != null)
            {
                StopCoroutine(_jumpingCoroutine);
            }
            _jumpingCoroutine = StartCoroutine(JumpingCoroutine(isForce));
        }
        /// <summary>
        /// 供Jump方法使用的跳跃协程
        /// </summary>
        /// <returns></returns>
        IEnumerator JumpingCoroutine(bool isForce=true)
        {   
            
            
            ModifierSet values = modifyPipeline.GetModifierSet();
            float hangTime = values.hangTime.FinalValue;
            float maxJumpingCD = values.maxJumpingCD.FinalValue;

            SetState(GameCharacterState.JumpHanging);
            float timer = 0;


            Vector3 startingPoint = transform.localPosition;
            Vector3 distination = startingPoint;
            distination.x = isRightWall ? leftWallPos : rightWallPos;
            while (timer <= hangTime)
            {
                yield return null;
                timer += Time.deltaTime;

                /** 弹反检测 */
                if (state == GameCharacterState.JumpHanging)
                    ParryDetect(); 
                if (parryAtFrame)
                {
                    parryAtFrame = false;                      
                    /** 如果在这一帧中触发了弹反，先复位触发，切换状态，然后倒置计时器，交换移动终/起点 */
                    SetState(GameCharacterState.ParryHanging);
                    timer = hangTime - timer;
                    Vector2 temp = startingPoint;
                    startingPoint = distination;
                    distination = temp;
                    ServiceLocator.GetService<VibrationMgrInterface>().AddVibration(0.4f,new Vector3(0.8f,0,0));
                    miniEventBus.Publish<AfterParryEvent>(new AfterParryEvent());

                }
                transform.localPosition = Vector3.Lerp(startingPoint,distination,timer/hangTime);
            }
            transform.localPosition = distination;
            nowJumpingCDMV.value = maxJumpingCD;

            if (!isForce)
            {
                miniEventBus.Publish<AfterJumpEvent>(new AfterJumpEvent{toRightWall=!isRightWall});
            }
            if (state == GameCharacterState.JumpHanging)
            {
                AttackDetect();
                isRightWall = !isRightWall;
            }
            else
            {
                /** 这里改为施加僵直buff */
                gameObject.GetComponent<BuffContainer>().AddBuff(new StiffnessBuff(),3);
            }
            SetState(GameCharacterState.Idle);
            MoveRecovery();
            ServiceLocator.GetService<VibrationMgrInterface>().AddVibration(0.2f,new Vector3(0.2f,0,0));
        }
        public void EnterMove(bool isUp)
        {
            moveDirectionStack.Add(isUp);
            if (state == GameCharacterState.JumpHanging || state == GameCharacterState.ParryHanging) return;
            towardUp = moveDirectionStack[moveDirectionStack.Count - 1];
            SetState(GameCharacterState.Moving);
        }
        public void ExitMove(bool isUp)
        {
            moveDirectionStack.Remove(isUp);
            if (state == GameCharacterState.JumpHanging || state == GameCharacterState.ParryHanging) return;
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
            if (state == GameCharacterState.JumpHanging || state == GameCharacterState.ParryHanging) return;

            bool isUp = moveDirectionStack[moveDirectionStack.Count-1];
            ActionContext actionContext = new();
            miniEventBus.Publish<BeforeMoveEvent>(new BeforeMoveEvent{context=actionContext,isUp = isUp});
            if (actionContext.cancelled) return;

            towardUp = isUp;

            ModifierSet values = modifyPipeline.GetModifierSet();
            float moveSpeed = values.moveSpeed.FinalValue;

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

            miniEventBus.Publish<AfterMoveEvent>(new AfterMoveEvent{isUp = isUp});
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
            ActionContext actionContext = new();
            miniEventBus.Publish<BeforeParryEvent>(new BeforeParryEvent{context=actionContext,opponent=opponent});
            if (actionContext.cancelled) return;  //事件是否被取消
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
                    ModifierSet modifierSet =  modifyPipeline.GetModifierSet();
                    int damage = (int)modifierSet.damage.FinalValue;

                    DamageContext damageContext = new();
                    damageContext.damage = damage;

                    miniEventBus.Publish<BeforeAttackEvent>(new BeforeAttackEvent{context=damageContext,victim=i.collider.gameObject});
                    if (damageContext.cancelled) continue;  //事件是否被取消

                    gameCharacterController.BeAttacked(gameObject,(int)damageContext.damage);
                    attacked.Add(i.collider.gameObject);

                    miniEventBus.Publish<AfterAttackEvent>(new AfterAttackEvent{victim=i.collider.gameObject});
                }
            }
            
        }
        /// <summary>
        /// 受击
        /// </summary>
        /// <param name="attacker">攻击者</param>
        public void BeAttacked(GameObject attacker, int damage)
        {
            DamageContext damageContext = new();
            damageContext.damage = damage;
            /** 暂时先不考虑道具卡和藏品造成的修饰，后面再引入 */
            miniEventBus.Publish<BeforeBeAttackedEvent>(new BeforeBeAttackedEvent{context=damageContext,attacker=attacker});
            if (damageContext.cancelled) return;  //事件是否被取消

            healthController.ReduceHealth((int)damageContext.damage);
            miniEventBus.Publish<AfterBeAttackedEvent>(new AfterBeAttackedEvent{attacker=attacker});

            if (healthController.health <= 0) Die();
        }
        /// <summary>
        /// 死亡
        /// </summary>
        public void Die()
        {
            ActionContext actionContext = new();
            miniEventBus.Publish<BeforeDie>(new BeforeDie{context=actionContext});
            if (actionContext.cancelled) return;

            miniEventBus.Publish<AfterDie>(new AfterDie());
            Destroy(gameObject);
        }

        private void SetState(GameCharacterState newState)
        {
            if (state == newState && towardUp == _lastTowardUp) return;
            state = newState;
            _lastTowardUp = towardUp;
            miniEventBus.Publish(new StateChangedEvent { state = state, towardUp = towardUp, isRightWall = isRightWall});
        }
        public void SetPosition(float y, bool isRightWall)
        {
            this.isRightWall = isRightWall;
            transform.localPosition = new(isRightWall?rightWallPos:leftWallPos,y,0);
        }
    }
}