namespace Flicker.Module
{
    public interface IInput
    {
        public bool Up {get;}
        public bool Down {get;}
        public bool Jump {get;}
        public bool Number(int key);
    }
}