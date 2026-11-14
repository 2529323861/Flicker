using System.Collections;
using Flicker.View;
using UnityEngine;

namespace Flicker.UI
{
    public class CountDown: UIPanel
    {
        Coroutine coroutine;
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
            base.OnClose();
        }
    }
}