using UnityEngine;

namespace Flicker.Module
{
    /// <summary>
    /// 用户输入路由，用于集中获取键盘输入，便于后期解耦
    /// </summary>
    public class InputRouter
    {
        public static float Vertical => Input.GetAxis("Vertical");

        public static bool Up => Input.GetKey(KeyCode.UpArrow);
        public static bool Down => Input.GetKey(KeyCode.DownArrow);


        public static bool Jump => Input.GetKeyDown(KeyCode.Space);

    }
}