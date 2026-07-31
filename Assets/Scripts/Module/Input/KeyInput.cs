using UnityEngine;

namespace Flicker.Module
{
    public class KeyInput : IInput
    {
        public bool Up => Input.GetKey(KeyCode.UpArrow);
        public bool Down => Input.GetKey(KeyCode.DownArrow);
        public bool Jump => Input.GetKeyDown(KeyCode.Space);
        public bool Number(int key)
        {
            return Input.GetKeyDown((KeyCode)(48+key));
        }
    }
}