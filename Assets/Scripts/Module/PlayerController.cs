using Flicker.Core.Service;
using UnityEngine;
using Flicker.Module;
using Flicker.Module.Buff;

namespace Flicker.Module
{   
    public class PlayerController : GameCharacterController
    {
        void Start()
        {
            GetComponent<BuffContainer>().AddBuff(new AccelerateBuff(),5);
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