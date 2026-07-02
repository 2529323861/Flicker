using Flicker.Core.Service;
using UnityEngine;
using Flicker.Module;

namespace Flicker.Module
{   
    public class PlayerController : GameCharacterController
    {
        void Start()
        {
            GetComponent<EffectContainer>().AddEffect(new MockStrategy());
        }
        new void Update()
        {
            base.Update();

            if (InputRouter.Up)
                Move(true);
            else if (InputRouter.Down)
                Move(false);
            if (InputRouter.Jump) Jump();
        }
    }
}