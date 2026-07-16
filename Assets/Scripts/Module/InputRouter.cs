using Mirror.Transports.Encryption;
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
        public static bool NumBer1 => Input.GetKeyDown(KeyCode.Alpha1);
        public static bool NumBer2 => Input.GetKeyDown(KeyCode.Alpha2);
        public static bool NumBer3 => Input.GetKeyDown(KeyCode.Alpha3);
        public static bool NumBer4 => Input.GetKeyDown(KeyCode.Alpha4);
        public static bool NumBer5 => Input.GetKeyDown(KeyCode.Alpha5);
        public static bool Number(int key) {
            return Input.GetKeyDown((KeyCode)(48+key));
        }
    }
}