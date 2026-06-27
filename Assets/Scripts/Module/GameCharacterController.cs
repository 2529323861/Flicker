using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Flicker.Core.Service;
using Mirror.Transports.Encryption;
using UnityEditor;
using UnityEngine;

namespace Flicker.Module
{   
    /// <summary>
    /// 游戏角色状态流转常量
    /// </summary>
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
        /// 僵直
        /// </summary>
        Stiffness
    }
    /// <summary>
    /// 游戏角色行为基类，定义跳跃，跳跃冷却等所有游戏角色都有的基本行为
    /// </summary>
    public class GameCharacterController : MonoBehaviour 
    {   
        /// <summary>
        /// 移动速度
        /// </summary>
        public float moveSpeed = 3;

        /// <summary>
        /// 滞空时间
        /// </summary>
        public float hangTime = 0.2f;

        /// <summary>
        /// 最大跳跃冷却
        /// </summary>
        public float maxJumpingCD = 2f;

        /// <summary>
        /// 碰撞半径
        /// </summary>
        public float collisionRadius = 0.5f;

        /// <summary>
        /// 左边墙的位置
        /// </summary>
        public float leftWallPos = -5.3f;

        /// <summary>
        /// 右边墙的位置
        /// </summary>
        public float rightWallPos = 5.3f;

        /// <summary>
        /// 阵营tag
        /// </summary>
        public string teamTag = "default";

        /// <summary>
        /// 剩余僵直时间
        /// </summary>
        public float nowStiffTime = 0;
        /// <summary>
        /// 剩余跳跃冷却时间
        /// </summary>
        public float nowJumpingCD = 0f;
        /// <summary>
        /// 当前墙面
        /// </summary>
        public bool isRightWall = false;

        /// <summary>
        /// 跳跃协程
        /// </summary>
        [HideInInspector]
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
            if (nowJumpingCD > 0)
            {
                nowJumpingCD -= Time.deltaTime;
                if (nowJumpingCD < 0)
                {
                    nowJumpingCD = 0;
                }
            }
            /** 维护僵直CD */
            if (nowStiffTime > 0)
            {
                nowStiffTime -= Time.deltaTime;
                if (nowStiffTime < 0)
                {
                    nowStiffTime = 0;
                    state = GameCharacterState.Idle;
                }
            }
        }
        /// <summary>
        /// 跳跃
        /// </summary>
        public void Jump()
        {
            if (state != GameCharacterState.Idle && state != GameCharacterState.Moving) return;
            if (nowJumpingCD >0) return;

            ParryDetect();

            if (_jumpingCoroutine != null)
            {
                StopCoroutine(_jumpingCoroutine);
            }
            _jumpingCoroutine = StartCoroutine(JumpingCoroutine());
        }
        /// <summary>
        /// 供Jump方法使用的跳跃协程
        /// </summary>
        /// <returns></returns>
        IEnumerator JumpingCoroutine()
        {
            state = GameCharacterState.JumpHanging;
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
                    /** 如果在这一帧中触发了弹反，先复位触发，切换状态，然后倒置计时器，交换移动终/起点 */
                    parryAtFrame = false;
                    state = GameCharacterState.ParryHanging;
                    timer = hangTime - timer;
                    Vector2 temp = startingPoint;
                    startingPoint = distination;
                    distination = temp;
                    ServiceLocator.GetService<VibrationMgrInterface>().AddVibration(0.4f,new Vector3(0.8f,0,0));
                }
                transform.localPosition = Vector3.Lerp(startingPoint,distination,timer/hangTime);
            }
            transform.localPosition = distination;
            nowJumpingCD = maxJumpingCD;
            if (state == GameCharacterState.JumpHanging)
            {                
                state = GameCharacterState.Idle;
                isRightWall = !isRightWall;
            }
            else
            {
                Stiff(3);
            }
            ServiceLocator.GetService<VibrationMgrInterface>().AddVibration(0.2f,new Vector3(0.2f,0,0));
        }
        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="isUp"></param>
        public void Move(bool isUp)
        {
            if (state != GameCharacterState.Idle && state != GameCharacterState.Moving) return;
            float deltaY = moveSpeed * Time.deltaTime * (isUp ? 1 : -1);
            Vector3 temp = transform.localPosition;
            transform.localPosition = new Vector3(temp.x,temp.y+deltaY,temp.z);
        }
        /// <summary>
        /// 弹反侦测
        /// </summary>
        private void ParryDetect()
        {
            Vector2 topStart = new Vector2(transform.position.x,transform.position.y+collisionRadius);
            Vector2 bottomStart = new Vector2(transform.position.x,transform.position.y-collisionRadius);

            Debug.DrawRay(topStart,new Vector2(isRightWall ? -100 : 100,0),Color.red,hangTime);
            Debug.DrawRay(bottomStart,new Vector2(isRightWall ? -100 : 100,0),Color.red,hangTime);

            RaycastHit2D[] topHit =  Physics2D.RaycastAll(topStart,new Vector2(isRightWall ? -1 : 1,0),collisionRadius);
            RaycastHit2D[] bottomHit =  Physics2D.RaycastAll(bottomStart,new Vector2(isRightWall ? -1 : 1,0),collisionRadius);

            RaycastHit2D[] raycastHit2Ds = topHit.Concat(bottomHit).ToArray();
            foreach (var i in raycastHit2Ds)
            {
                if (i.collider.gameObject.TryGetComponent<GameCharacterController>(out var gameCharacterController) && //是游戏角色
                    gameCharacterController.teamTag != teamTag && //与自己不是一个阵营的
                    gameCharacterController.state == GameCharacterState.JumpHanging && //处于跳跃状态
                    (i.collider.gameObject.transform.localPosition - transform.localPosition).magnitude <= collisionRadius + gameCharacterController.collisionRadius && //碰撞
                    gameCharacterController.isRightWall != isRightWall //相向移动
                    )
                {
                    gameCharacterController.Parry();
                    Parry();
                }
            }
            
        }
        /// <summary>
        /// 弹反
        /// </summary>
        public void Parry()
        {
            if (state != GameCharacterState.JumpHanging) return;
            parryAtFrame = true;
        }
        /// <summary>
        /// 僵直
        /// </summary>
        /// <param name="time">僵直时间</param>
        public void Stiff(float time)
        {
            state = GameCharacterState.Stiffness;
            nowStiffTime = time;
        }
    }
}