using Flicker.Core.Service;
using UnityEngine;
using Flicker.Module;

namespace Flicker.Module
{   
    /// <summary>
    /// 测试用Mock对象的操作
    /// </summary>
    public class MockrController : GameCharacterController
    {
        new void Update()
        {
            base.Update();

            if (Input.GetKeyDown(KeyCode.Return)) Jump();
        }
    }
}