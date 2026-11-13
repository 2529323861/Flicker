using Flicker.Core;
using Flicker.Module.CharacterEvent;
using UnityEngine;

namespace Flicker.Module
{
    /// <summary>
    /// 单局内各角色的行为统计，供结算面板展示。
    ///
    /// 只订阅本对象 MiniEventBus 上的事件，因此天然按角色隔离，
    /// 不需要额外的归属判断。三个事件都只在服务端发布，
    /// 客户端是空壳不参与，所以统计值只在服务端有效。
    /// </summary>
    [RequireComponent(typeof(MiniEventBus))]
    public class CharacterStats : MonoBehaviour
    {
        /// <summary>跳跃次数。以跳跃确认发生为准，被取消的跳跃不计</summary>
        public int jumpCount {get;private set;}
        /// <summary>弹反次数。双方互相弹反时各自都算一次</summary>
        public int parryCount {get;private set;}
        /// <summary>道具使用成功次数。冷却中或条件不满足被拒绝的不计</summary>
        public int propUseCount {get;private set;}

        void Awake()
        {
            MiniEventBus bus = GetComponent<MiniEventBus>();
            /** JumpResult 在 Before 的取消判定之后发布，因此只统计真正发生的跳跃 */
            bus.Subscribe<JumpResultEvent>(OnJump);
            bus.Subscribe<AfterParryEvent>(OnParry);
            bus.Subscribe<AfterUsePropEvent>(OnUseProp);
        }

        private void OnJump(JumpResultEvent evt) => jumpCount++;
        private void OnParry(AfterParryEvent evt) => parryCount++;
        private void OnUseProp(AfterUsePropEvent evt) => propUseCount++;

        /// <summary>
        /// 清零。分数与计数都不需要跨局保留
        /// </summary>
        public void ResetCounters()
        {
            jumpCount = 0;
            parryCount = 0;
            propUseCount = 0;
        }
    }
}
