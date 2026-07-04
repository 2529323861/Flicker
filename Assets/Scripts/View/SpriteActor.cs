using UnityEngine;

namespace Flicker.View
{
    /// <summary>
    /// 精灵演员基类，封装 SpriteRenderer 和 Animator 控制。
    /// 提供动画播放、朝向、颜色、排序等通用视觉操作。
    /// 具体业务行为（移动到攻击位、受击反馈等）由子类扩展。
    /// </summary>
    public abstract class SpriteActor : MonoBehaviour
    {
        protected SpriteRenderer _spriteRenderer;

        [SerializeField]
        protected Animator _animator;
        protected bool _isFacingRight = true;
        protected Color _baseColor = Color.white;

        protected virtual void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer != null) _baseColor = _spriteRenderer.color;
        }

        protected virtual void Start()
        {

        }

        protected virtual void OnDestroy()
        {

        }

        private Coroutine _returnAnimationCoroutine;

        public void PlayAnimation(string animName, bool forceRestart = false)
        {
            if (_returnAnimationCoroutine != null)
            {
                StopCoroutine(_returnAnimationCoroutine);
                _returnAnimationCoroutine = null;
            }

            if (_animator != null && _animator.runtimeAnimatorController != null)
            {
                if (_animator.HasState(0, Animator.StringToHash(animName)))
                {
                    if (forceRestart) _animator.Play(animName, 0, 0f);
                    else _animator.Play(animName);
                }
                else
                {
                    string lowerAnim = animName.ToLower();
                    if (_animator.HasState(0, Animator.StringToHash(lowerAnim)))
                    {
                        if (forceRestart) _animator.Play(lowerAnim, 0, 0f);
                        else _animator.Play(lowerAnim);
                    }
                    else
                    {
                        if (forceRestart) _animator.Play(animName, 0, 0f);
                        else _animator.Play(animName);
                    }
                }
            }
        }

        public void PlayAnimationOneShot(string animName, string returnTo, float duration)
        {
            if (_animator == null || !_animator.gameObject.activeInHierarchy) return;

            if (_returnAnimationCoroutine != null)
            {
                StopCoroutine(_returnAnimationCoroutine);
            }

            PlayAnimation(animName, forceRestart: true);
            _returnAnimationCoroutine = StartCoroutine(ReturnAnimationAfter(returnTo, duration));
        }

        private System.Collections.IEnumerator ReturnAnimationAfter(string returnTo, float delay)
        {
            yield return new UnityEngine.WaitForSeconds(delay);
            _returnAnimationCoroutine = null;
            if (_animator != null && _animator.gameObject.activeInHierarchy)
            {
                PlayAnimation(returnTo);
            }
        }

        public void SetFacing(bool faceRight)
        {
            _isFacingRight = faceRight;
            if (_spriteRenderer != null)
                _spriteRenderer.flipX = faceRight; 
        }

        public void SetSortingOrder(int order)
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sortingOrder = order;
            }
        }

        public void SetColor(Color color)
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = color;
            }
        }
    }
}
