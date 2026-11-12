using Flicker.Core;
using Flicker.Module.ViewEvent;
using UnityEngine;
using UnityEngine.UI;

namespace Flicker.UI
{
    /// <summary>
    /// 全屏闪白。峰值与衰减曲线由预制体上的 ScreenFlash 动画决定，
    /// 代码只负责「何时播」与「播多强」。
    ///
    /// 作为 UIPanel 存在是为了复用 UIManager 的预制体加载与缓存，
    /// 但每次播放都会把自己提到最上层并重播动画，因此不会排队也不会被遮挡。
    /// </summary>
    public class ScreenFlash : UIPanel
    {
        private static ScreenFlash instance;

        /// <summary>预制体上的动画状态名，非循环</summary>
        private const string AnimState = "ScreenFlash";

        [Tooltip("动画状态名，默认与预制体中的状态同名")]
        public string animState = AnimState;

        private Animator animator;
        /// <summary>用于整体缩放峰值亮度。动画负责曲线形状，它负责强度</summary>
        private CanvasGroup canvasGroup;

        void Awake()
        {
            instance = this;

            /** 作为 UIPanel 首次被创建时会处于关闭状态，因此不禁用自身 */
            animator = GetComponent<Animator>();
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

            /** 闪白层不能吃掉点击，否则会让底下的按钮失效 */
            foreach (var graphic in GetComponentsInChildren<Graphic>(true))
            {
                graphic.raycastTarget = false;
            }

            EventBus.Subscribe<PlayScreenFlashEvent>(OnPlayScreenFlash);
        }

        void OnDestroy()
        {
            EventBus.Unsubscribe<PlayScreenFlashEvent>(OnPlayScreenFlash);
            if (instance == this) instance = null;
        }

        private void OnPlayScreenFlash(PlayScreenFlashEvent evt)
        {
            Play(evt.alphaScale, evt.speed);
        }

        /// <summary>
        /// 播放一次闪白。动画从头重播，连续触发不会叠加成一片白
        /// </summary>
        /// <param name="alphaScale">峰值亮度系数，0~1</param>
        /// <param name="speed">播放倍速，小于 1 变慢、看起来更重</param>
        public static void Play(float alphaScale, float speed)
        {
            if (instance == null) return;
            instance.PlayInner(alphaScale, speed);
        }

        private void PlayInner(float alphaScale, float speed)
        {
            if (animator == null) return;

            /** 闪白必须在最上层，否则会被后开的面板盖住 */
            transform.SetAsLastSibling();

            canvasGroup.alpha = Mathf.Clamp01(alphaScale);
            animator.speed = Mathf.Max(0.01f, speed);
            /** 归一化时间显式归零，保证连击时每次都从第一帧开始 */
            animator.Play(animState, 0, 0f);
        }
    }
}
