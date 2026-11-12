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
    /// 顿帧。命中或弹反瞬间冻结游戏时间若干毫秒，制造重击感。
    /// 项目内所有逻辑与表现都已避开 timeScale（NetworkTime 用非缩放时间、
    /// 音频淡化也用非缩放时间），因此 timeScale 可以专门留给顿帧。
    /// 属于表现，走 EventBroadcast，两端同时冻结
    /// </summary>
    public struct HitStopEvent
    {
        /// <summary>冻结时长（秒），通常 0.04 ~ 0.08</summary>
        public float duration;
    }

    /// <summary>
    /// 全屏闪白。屏幕空间的表现，所有客户端都要看到，
    /// 因此与震动同走 EventBroadcast，不挂在任何角色上。
    /// 曲线形状由动画决定，这里只控制强度与速度
    /// </summary>
    public struct PlayScreenFlashEvent
    {
        /// <summary>峰值亮度系数，0~1</summary>
        public float alphaScale;
        /// <summary>播放倍速，小于 1 变慢、看起来更重</summary>
        public float speed;
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