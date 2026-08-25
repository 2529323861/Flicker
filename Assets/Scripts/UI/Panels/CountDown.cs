using System.Collections;
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