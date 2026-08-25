using Flicker.Core;
using Flicker.Module;
using Flicker.Module.CharacterEvent;
using UnityEngine;

namespace Flicker.View
{
    [RequireComponent(typeof(Animator))]
    public class GameCharacterActor: SpriteActor
    {
        Animator animator;
        new void Awake()
        {
            base.Awake();
            animator = GetComponent<Animator>();
        }
        public void ChangeWallTo(bool right)
        {
            _spriteRenderer.flipY = right;
        }
        public void ChangeTowardTo(bool up)
        {
            _spriteRenderer.flipX = up;
        }
        public void ChangeAnimationStateTo(GameCharacterState gameCharacterState)
        {
            animator.SetInteger("State",(int)gameCharacterState);
        }
    }
}