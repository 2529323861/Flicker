using Flicker.Core.Service;
using Flicker.Module;
using UnityEngine;

namespace Flicker.View
{
    public class PlayerActor: SpriteActor
    {
        void Update()
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0,InputRouter.Vertical*3f);
            if (InputRouter.Jump)
            {
                Vector3 temp = transform.localPosition;
                transform.localPosition = new Vector3(-temp.x,temp.y,temp.z);
                ServiceLocator.GetService<VibrationMgrInterface>().AddVibration(0.2f,new Vector3(0.2f,0,0));
            }
        }
    }
}