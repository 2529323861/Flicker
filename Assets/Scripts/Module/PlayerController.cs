using Flicker.Core.Service;
using UnityEngine;
using Flicker.Module;

namespace Flicker.Module
{   
    public class PlayerController : GameCharacterController
    {
        new void Update()
        {
            base.Update();
            GetComponent<Rigidbody2D>().velocity = new Vector2(0,InputRouter.Vertical*3f);
            if (InputRouter.Jump)
            {
                Jump();
            }
        }
    }
}