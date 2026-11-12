using System.Collections;
using System.Collections.Generic;
using Flicker.Core;
using Flicker.Data;
using Flicker.Module.ViewEvent;
using UnityEngine;

namespace Flicker.View
{
    /// <summary>
    /// 音频管理器。音乐走双音源交叉淡化，音效走 PlayOneShot 叠加。
    ///
    /// 音乐与音效的区别全部来自两点：
    ///   1. 音乐需要循环、需要渐变，因此用 Play() 独占一个音源；
    ///   2. 音效不需要渐变且转瞬即逝，因此用 PlayOneShot() 共用同一个音源。
    ///
    /// 资源路径约定（与项目现有 Resources 用法一致，均不带扩展名）：
    ///   Assets/Resources/Sounds/Music/&lt;key&gt;
    ///   Assets/Resources/Sounds/Sfx/&lt;key&gt;
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("音量")]
        [SerializeField, Range(0f, 1f)] private float musicVolumeValue = 0.6f;
        [SerializeField, Range(0f, 1f)] private float sfxVolumeValue = 0.8f;

        /// <summary>
        /// 音乐音量。赋值立即生效：淡化结束后音源的 volume 不再被任何代码写入，
        /// 所以必须在这里主动同步，否则拖滑条时听不到变化
        /// </summary>
        public float musicVolume
        {
            get => musicVolumeValue;
            set
            {
                musicVolumeValue = Mathf.Clamp01(value);
                ApplyMusicVolume();
            }
        }

        /// <summary>
        /// 音效音量。写在音源自身而不是每次 PlayOneShot 的系数上，
        /// 这样已经在播的音也会跟着变
        /// </summary>
        public float sfxVolume
        {
            get => sfxVolumeValue;
            set
            {
                sfxVolumeValue = Mathf.Clamp01(value);
                if (sfxSource != null) sfxSource.volume = sfxVolumeValue;
            }
        }

        [Header("淡化")]
        [Tooltip("交叉淡化的默认时长（秒）")]
        public float defaultFadeTime = 0.8f;

        private const string MusicRoot = "Sounds/Music/";
        private const string SfxRoot = "Sounds/Sfx/";

        /// <summary>
        /// 音乐双音源。交叉淡化要求两首曲子同时存在一段时间，
        /// 而单个 AudioSource 只有一个 clip，所以必须准备两个轮换使用
        /// </summary>
        private AudioSource musicA;
        private AudioSource musicB;
        /// <summary>音效音源。PlayOneShot 允许同一音源上叠加多个音，因此一个就够</summary>
        private AudioSource sfxSource;

        /// <summary>当前持有音乐的音源，null 表示当前无音乐</summary>
        private AudioSource currentMusic;
        /// <summary>当前音乐的 key，用于忽略同一首的重复请求</summary>
        private string currentMusicKey;

        private Coroutine fadeRoutine;

        /// <summary>已加载的音频片段。音频解码有开销，同一路径只加载一次</summary>
        private readonly Dictionary<string, AudioClip> clipCache = new();

        protected override void OnSingletonInit()
        {
            musicA = CreateSource("MusicA", loop: true);
            musicB = CreateSource("MusicB", loop: true);
            sfxSource = CreateSource("Sfx", loop: false);

            /** 走属性赋值，让音源与设置值从上电就一致 */
            musicVolume = SettingsManager.Instance.GetSetting<float>("musicVolume");
            sfxVolume = SettingsManager.Instance.GetSetting<float>("sfxVolume");
        }

        /// <summary>
        /// 把当前音量写到正在播放的音源上。
        /// 淡化期间不写：渐变协程每帧都在覆写 volume，此时插手会与它冲突。
        /// 而淡化期间协程每帧读的就是 musicVolume，本来就能取到新值
        /// </summary>
        private void ApplyMusicVolume()
        {
            if (fadeRoutine != null) return;
            if (currentMusic == null) return;
            currentMusic.volume = musicVolumeValue;
        }

        /// <summary>
        /// 订阅表现层事件。与 VibrationManager 同一模式：
        /// 逻辑层只管广播，音频侧自己订阅，双方互不依赖
        /// </summary>
        public void Init()
        {
            EventBus.Subscribe<PlaySfxEvent>(OnPlaySfx);
            EventBus.Subscribe<PlayMusicEvent>(OnPlayMusic);
        }

        private AudioSource CreateSource(string sourceName, bool loop)
        {
            GameObject go = new(sourceName);
            go.transform.SetParent(transform, false);
            AudioSource source = go.AddComponent<AudioSource>();
            /** 默认 true 会在赋值 clip 的瞬间自动播放一次 */
            source.playOnAwake = false;
            source.loop = loop;
            /** 0 = 纯 2D，不做距离衰减。本项目没有空间音频需求 */
            source.spatialBlend = 0f;
            return source;
        }
        //—————————————————————————————————音乐———————————————————————————————————————————
        /// <summary>
        /// 切换背景音乐。当前音乐淡出、新音乐淡入，两者在中途同时存在。
        /// 同一首请求正在播放时直接忽略，因此可以在每个面板的 OnOpen 里无条件调用
        /// </summary>
        /// <param name="key">Music 目录下的相对路径</param>
        /// <param name="fadeTime">淡化时长，非正数用默认值</param>
        public void PlayMusic(string key, float fadeTime = -1f)
        {
            if (fadeTime <= 0f) fadeTime = defaultFadeTime;

            /** 同一首仍在播就不重播，避免每次进面板都从头开始 */
            if (currentMusic != null && currentMusicKey == key && currentMusic.isPlaying) return;

            AudioClip clip = LoadClip(MusicRoot + key);
            if (clip == null) return;

            /** 乒乓：始终用当前空闲的那个音源，保证新旧两首能并存 */
            AudioSource next = currentMusic == musicA ? musicB : musicA;
            next.clip = clip;
            next.volume = 0f;
            next.Play();

            AudioSource prev = currentMusic;
            currentMusic = next;
            currentMusicKey = key;

            StartFade(prev, next, fadeTime);
        }

        /// <summary>
        /// 停止背景音乐并淡出
        /// </summary>
        public void StopMusic(float fadeTime = -1f)
        {
            if (currentMusic == null) return;
            if (fadeTime <= 0f) fadeTime = defaultFadeTime;

            AudioSource prev = currentMusic;
            currentMusic = null;
            currentMusicKey = null;

            StartFade(prev, null, fadeTime);
        }

        /// <summary>
        /// 启动一次交叉淡化。任一参数可为 null：
        /// from 为 null 表示只有淡入，to 为 null 表示只有淡出
        /// </summary>
        private void StartFade(AudioSource from, AudioSource to, float duration)
        {
            /** 上一次淡化尚未结束就被打断时，直接跳到终态，避免音量停在中间值 */
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                fadeRoutine = null;
            }

            if (duration <= 0f)
            {
                ApplyFadeEnd(from, to);
                return;
            }
            fadeRoutine = StartCoroutine(FadeRoutine(from, to, duration));
        }

        private IEnumerator FadeRoutine(AudioSource from, AudioSource to, float duration)
        {
            /** 记录起始音量：被打断后重新淡出时不会从 1 跳变 */
            float fromStart = from != null ? from.volume : 0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                /** 用非缩放时间：暂停或慢动作时音乐也要继续淡 */
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (to != null) to.volume = musicVolume * t;
                if (from != null) from.volume = fromStart * (1f - t);

                yield return null;
            }

            ApplyFadeEnd(from, to);
            fadeRoutine = null;
        }

        /// <summary>
        /// 淡化的终态：目标音源到满音量，来源音源归零并释放
        /// </summary>
        private void ApplyFadeEnd(AudioSource from, AudioSource to)
        {
            if (to != null) to.volume = musicVolume;

            if (from == null) return;
            from.volume = 0f;
            from.Stop();
            /** 清空 clip 以释放引用，下次复用时不会残留上一首 */
            from.clip = null;
        }
        //—————————————————————————————————音效———————————————————————————————————————————
        /// <summary>
        /// 播放一次音效。PlayOneShot 不打断已在播放的音，也不需要等待结束，
        /// 因此音效只需一个音源，不需要音源池
        /// </summary>
        /// <param name="key">Sfx 目录下的相对路径</param>
        /// <param name="volumeScale">额外音量系数，0~1</param>
        public void PlaySfx(string key, float volumeScale = 1f)
        {
            AudioClip clip = LoadClip(SfxRoot + key);
            if (clip == null) return;

            /** 这里的系数只表达单次差异，总音量由音源自身承担 */
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
        }
        //—————————————————————————————————资源———————————————————————————————————————————
        /// <summary>
        /// 按路径加载并缓存音频片段。Resources.Load 的路径不带扩展名
        /// </summary>
        private AudioClip LoadClip(string path)
        {
            if (clipCache.TryGetValue(path, out AudioClip cached)) return cached;

            AudioClip clip = Resources.Load<AudioClip>(path);
            if (clip == null)
            {
                Debug.LogWarning($"[Audio] 找不到音频资源：Resources/{path}");
                return null;
            }
            clipCache[path] = clip;
            return clip;
        }
        //—————————————————————————————————事件———————————————————————————————————————————
        private void OnPlaySfx(PlaySfxEvent evt)
        {
            PlaySfx(evt.key, evt.volumeScale);
        }

        private void OnPlayMusic(PlayMusicEvent evt)
        {
            PlayMusic(evt.key, evt.fadeTime);
        }
    }
}
