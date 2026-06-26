using System.Collections;
using System.Collections.Generic;
using Flicker.Core.Service;
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
        /// 滞空
        /// </summary>
        JumpHanging,
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
        /// 剩余跳跃冷却时间
        /// </summary>
        public float nowJumpingCD = 0f;
        /// <summary>
        /// 当前墙面
        /// </summary>
        private bool isRightWall = false;

        /// <summary>
        /// 跳跃协程
        /// </summary>
        [HideInInspector]
        private Coroutine _jumpingCoroutine = null;

        /// <summary>
        /// 游戏角色状态
        /// </summary>
        protected GameCharacterState state = GameCharacterState.Idle;

        /// <summary>
        /// 子类重写时必须先调用base
        /// </summary>
        protected void Update()
        {
            if (nowJumpingCD > 0)
            {
                nowJumpingCD -= Time.deltaTime;
                if (nowJumpingCD < 0)
                {
                    nowJumpingCD = 0;
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
                transform.localPosition = Vector3.Lerp(startingPoint,distination,timer/hangTime);
            }
            transform.localPosition = distination;
            nowJumpingCD = maxJumpingCD;
            state = GameCharacterState.Idle;
            isRightWall = !isRightWall;
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
    }
}