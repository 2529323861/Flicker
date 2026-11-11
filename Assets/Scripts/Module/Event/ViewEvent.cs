using UnityEngine;

namespace Flicker.Module.ViewEvent
{
    public struct AddVibration
    {
        public float maxTime;
        public float amplitudeX;
        public float amplitudeY;
        public float amplitudeZ;
    }

    /// <summary>
    /// 播放一次音效。由逻辑层广播，负责表现的一端订阅后播放。
    /// PlayOneShot 可叠加，同一帧连续触发不会互相打断
    /// </summary>
    public struct PlaySfxEvent
    {
        /// <summary>Resources/Audio/Sfx 下的相对路径，不含扩展名</summary>
        public string key;
        /// <summary>音量系数，0~1。最终音量还会乘上 AudioManager 的音效音量</summary>
        public float volumeScale;
    }

    /// <summary>
    /// 切换背景音乐。与音效的区别是走交叉淡化，且同一首重复请求会被忽略
    /// </summary>
    public struct PlayMusicEvent
    {
        /// <summary>Resources/Audio/Music 下的相对路径，不含扩展名</summary>
        public string key;
        /// <summary>淡化时长，非正数表示使用 AudioManager 的默认值</summary>
        public float fadeTime;
    }
}