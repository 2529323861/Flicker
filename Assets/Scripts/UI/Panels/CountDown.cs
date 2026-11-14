using System.Collections;
using Flicker.View;
using UnityEngine;

namespace Flicker.UI
{
    public class CountDown: UIPanel
    {
        Coroutine coroutine;
        /// <summary>
        /// 本次倒计时是否已经播过音效。
        ///
        /// 首局会响两遍：面板预制体本身是激活的，Animator 启用时会进入默认状态
        /// CountDown 并从 t=0 起播；OnOpen 里的 Play 又把它从头重启。两者的动画事件
        /// 都发生在随后的 Animator 更新中，会各触发一次 t=0 事件。
        ///
        /// 重置必须放在 OnClose：动画事件与 OnOpen 的先后不可依赖（实测事件不在
        /// Instantiate / OnOpen 的调用栈内），只有"关闭时复位"才能保证无论事件
        /// 落在 OnOpen 之前还是之后，每次打开都恰好播一次
        /// </summary>
        bool sfxPlayed = false;

        public override void OnOpen(object args = null)
        {
            base.OnOpen(args);
            GetComponent<Animator>().Play("CountDown",0,0f);
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            coroutine = StartCoroutine(animationTime());
        }
        /// <summary>
        /// 倒计时音效。由 CountDown.anim 在 t=0 的动画事件调用。
        ///
        /// 音频素材本身长 3.6 秒，已经把 3 / 2 / 1 / GO 四个音全部录在里面，
        /// 因此整段倒计时只需在开头播一次，不要按帧去逐个数触发。
        ///
        /// 用 PlayOneShot 而非循环音源：声音播完即止，不受面板关闭影响
        /// </summary>
        public void PlayCountDownSfx()
        {
            if (sfxPlayed) return;
            sfxPlayed = true;
            AudioManager.Instance.PlaySfx("CountDown");
        }
        IEnumerator animationTime()
        {
            yield return new WaitForSecondsRealtime(4f);
            UIManager.Instance.Close<CountDown>();
        }
        public override void OnClose()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
            /** 关闭时复位，为下一次倒计时留出一次播放机会 */
            sfxPlayed = false;
            base.OnClose();
        }
    }
}