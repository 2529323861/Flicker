using Flicker.Core;
using Flicker.Core.Service;
using Flicker.Module;
using Flicker.Module.CharacterEvent;
using UnityEngine;

namespace Flicker.View
{
    public class PlayerActor: SpriteActor
    {
        Animator animator;
        new void Awake()
        {
            base.Awake();
            animator = GetComponent<Animator>();
            MiniEventBus miniEventBus = GetComponent<MiniEventBus>();
            miniEventBus.Subscribe<StateChangedEvent>(ChangeAnimation);
        }
        public void ChangeAnimation(StateChangedEvent evt)
        {
            _spriteRenderer.flipY = evt.isRightWall;
            if (evt.state == GameCharacterState.Moving)
            {
                _spriteRenderer.flipX = evt.towardUp;
            }
            else
            {
                SetFacing(false);
            }
            animator.SetInteger("State",(int)evt.state);
            print(evt.towardUp);
        }
    }
}