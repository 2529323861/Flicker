using Flicker.Module.UIEvent;
using Flicker.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Flicker.UI
{
    public class MultiPlaySettlementPanel: UIPanel
    {
        [Header("按钮")]
        public Button quit;

        [Header("胜负图")]
        [Tooltip("显示胜负图的 Image，胜负通过换 sprite 切换")]
        public Image resultImage;
        [Tooltip("获胜时使用的图片")]
        public Sprite winSprite;
        [Tooltip("失败时使用的图片")]
        public Sprite loseSprite;

        [Header("统计数据")]
        [Tooltip("对局时长，如「02:15」")]
        public TMP_Text durationValue;
        [Tooltip("道具使用次数")]
        public TMP_Text propUseValue;
        [Tooltip("跳跃次数")]
        public TMP_Text jumpCountValue;
        [Tooltip("弹反次数")]
        public TMP_Text parryCountValue;

        void Awake()
        {
            quit.onClick.AddListener(() =>
            {
                UIManager.Instance.Close<MultiPlaySettlementPanel>();
                UIManager.Instance.CloseAll(UILayer.HUD);
                UIManager.Instance.Open<LobbyPanel>();
            });
        }
        /// <summary>
        /// 打开时接收本局胜负与统计数据
        /// </summary>
        /// <param name="args">SettlementArgs</param>
        public override void OnOpen(object args = null)
        {
            base.OnOpen(args);
            AudioManager.Instance.PlayMusic("Settlement", 0.4f);

            SettlementArgs data = (SettlementArgs)args;

            /** 胜负通过换一张 sprite 表达，不需要两个 GameObject 来回开关 */
            if (resultImage != null)
            {
                resultImage.sprite = data.win ? winSprite : loseSprite;
            }

            if (durationValue != null) durationValue.text = FormatDuration(data.duration);
            if (propUseValue != null) propUseValue.text = data.propUseCount.ToString();
            if (jumpCountValue != null) jumpCountValue.text = data.jumpCount.ToString();
            if (parryCountValue != null) parryCountValue.text = data.parryCount.ToString();
        }

        /// <summary>
        /// 秒数格式化为 分'秒''帧，如 11'22''34，帧按 60fps 换算
        /// </summary>
        private static string FormatDuration(float seconds)
        {
            if (seconds < 0f) seconds = 0f;

            int totalSeconds = Mathf.FloorToInt(seconds);
            /** 用截断而不是四舍五入，避免 59.99 帧被进位成 60 */
            int frames = Mathf.Min(59, Mathf.FloorToInt((seconds - totalSeconds) * 60f));

            return $"{totalSeconds / 60}'{totalSeconds % 60:00}''{frames:00}";
        }
    }
}