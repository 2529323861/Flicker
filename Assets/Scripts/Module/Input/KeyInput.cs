using UnityEngine;

namespace Flicker.Module
{
    public class KeyInput : IInput
    {
        public bool Jump => Input.GetKeyDown(KeyCode.Space);

        public bool EnterUp => Input.GetKeyDown(KeyCode.UpArrow);

        public bool ExitUp => Input.GetKeyUp(KeyCode.UpArrow);

        public bool EnterDown => Input.GetKeyDown(KeyCode.DownArrow);

        public bool ExitDown => Input.GetKeyUp(KeyCode.DownArrow);

        public bool Number(int key)
        {
            return Input.GetKeyDown((KeyCode)(48+key));
        }
    }
}