using UnityEngine;

namespace Flicker.Module
{
    public class FalseInput : IInput
    {
        public bool Jump => false;

        public bool EnterUp => false;

        public bool ExitUp => false;

        public bool EnterDown => false;

        public bool ExitDown => false;

        public bool Number(int key)
        {
            return false;
        }
    }
}