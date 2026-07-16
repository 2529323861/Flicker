using Flicker.Core.Service;
using UnityEngine;
using Flicker.Module;
using Flicker.Module.Buff;
using Flicker.Core;
using Flicker.Module.UIEvent;

namespace Flicker.Module
{   
    public class PlayerController : GameCharacterController
    {
        PropContainer propContainer;
        new void Awake()
        {
            base.Awake();
            propContainer = GetComponent<PropContainer>();
        }
        void Start()
        {
            GameManager.Instance.SetPlayerRef(gameObject);
        }
        new void Update()
        {
            base.Update();

            if (InputRouter.Up)
                Move(true);
            else if (InputRouter.Down)
                Move(false);
            if (InputRouter.Jump) Jump();

            EventBus.Publish<UpdateJumpCDProgressEvent>(new UpdateJumpCDProgressEvent
            {
                jumpCDProgress = nowJumpingCDMV.value/maxJumpingCDMV.FinalValue
            });
            for (int i = 1; i <= 5; i++)
            {
                if (InputRouter.Number(i))
                {
                    propContainer.UseProp(i-1);
                }
            }
        }
    }
}