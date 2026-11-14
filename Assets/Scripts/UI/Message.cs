using TMPro;
using UnityEngine;

namespace Flicker.UI
{
    /// <summary>
    /// 单条聊天消息。
    ///
    /// 挂在 Message 预制体根节点上，对应层级：
    ///   Message       ← 本脚本
    ///   ├─ Sender     ← 发送者名
    ///   └─ MessageBox ← 气泡底板
    ///      └─ Text    ← 消息正文
    ///
    /// 高度由布局系统算出：正文按行数上报 preferredHeight，
    /// MessageBox 加上 padding 后上报给 Message，Message 再加上 Sender 的高度。
    /// 因此这里只负责填字，不碰任何尺寸。
    /// </summary>
    public class Message : MonoBehaviour
    {
        [Tooltip("发送者名，对应 Message/Sender")]
        public TMP_Text sender;
        [Tooltip("消息正文，对应 Message/MessageBox/Text")]
        public TMP_Text content;

        /// <summary>
        /// 设置发送者名
        /// </summary>
        public void SetSender(string name)
        {
            sender.text = name;
        }

        /// <summary>
        /// 设置消息正文。高度变化由布局系统在下一帧重建时处理，
        /// 需要立刻拿到最终高度（如滚动到底）请先调 LayoutRebuilder.ForceRebuildLayoutImmediate
        /// </summary>
        public void SetContent(string text)
        {
            content.text = text;
        }
    }
}
