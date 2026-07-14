namespace Flicker.Module.Prop
{
    public class RushGelProp: BasePropStrategy
    {
        public RushGelProp()
        {
            name = "急行粘液";
            id = "00001";
            path = "RushGel";
            description = "使用后3秒内，移动速度提高50%。";
        }
        public override void OnUse()
        {
            
        }
    }
}