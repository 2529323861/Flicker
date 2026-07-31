namespace Flicker.Module
{
    public interface IInput
    {
        public bool EnterUp {get;}
        public bool ExitUp {get;}
        public bool EnterDown {get;}
        public bool ExitDown {get;}
        public bool Jump {get;}
        public bool Number(int key);
    }
}