using UnityEngine;

namespace Flicker.Module
{
    public class FalseInput : IInput
    {
        public bool Up => false;
        public bool Down => false;
        public bool Jump => false;
        public bool Number(int key)
        {
            return false;
        }
    }
}