using UnityEngine;

namespace Flicker.View
{
    /// <summary>
    /// 道具使用特效。挂在角色预制体的子对象上，随角色移动。
    ///
    /// 放大与淡出由 Animator 播放，组件只负责「换图 + 从头播一次」。
    /// 动画结束需要隐藏自己：在动画最后一帧加一个 AnimationEvent，
    /// 函数名填 OnAnimFinished。
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Animator))]
    public class PropVfxPlayer : MonoBehaviour
    {
        [Header("资源")]
        [Tooltip("道具图标目录，与 PlayerHUD 使用同一套图")]
        public string iconRoot = "Images/UI/PropIcon/";

        [Header("动画")]
        [Tooltip("Animator 中的状态名")]
        public string animState = "PlayerPropVfx";

        SpriteRenderer spriteRenderer;
        Animator animator;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            /** 平时隐藏，只在播放时出现 */
            spriteRenderer.enabled = false;
        }

        /// <summary>
        /// 播放一次道具特效。重复调用会从头重播，不会叠加
        /// </summary>
        /// <param name="propPath">道具图标的相对路径，对应 PropIcon 目录下的文件名</param>
        public void Play(string propPath)
        {
            Sprite sprite = Resources.Load<Sprite>(iconRoot + propPath);
            /** 缺图标时静默跳过：美术资源可能尚未补齐，
             *  不该因为一张图没加就中断对局 */
            if (sprite == null) return;

            spriteRenderer.sprite = sprite;
            /** 上一次淡出把 alpha 改成了 0，重播前必须复位，
             *  否则动画只改缩放时图标仍然不可见 */
            spriteRenderer.color = Color.white;
            spriteRenderer.enabled = true;

            /** 第三个参数为归一化起始时间：0 表示从头播，
             *  正在播的动画会被打断并重置，无需额外 Stop */
            animator.Play(animState, 0, 0f);
        }

        /// <summary>
        /// 动画结束。由 Animator 最后一帧的 AnimationEvent 调用。
        ///
        /// 不能省略：SpriteRenderer 是常驻子物体，不主动关闭会一直留在画面上。
        /// </summary>
        public void OnAnimFinished()
        {
            spriteRenderer.enabled = false;
        }

        private void OnDisable()
        {
            /** 角色消失时收尾，避免重新启用时残留上一次的图 */
            if (spriteRenderer != null) spriteRenderer.enabled = false;
        }
    }
}
