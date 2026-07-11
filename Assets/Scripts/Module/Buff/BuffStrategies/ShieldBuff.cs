using Flicker.Module.Effect;

namespace Flicker.Module.Buff
{
    public class ShieldBuff: BaseBuffStrategy
    {
        private BaseEffectStrategy temp;
        public ShieldBuff(){
            description = "免疫3次攻击";
        }
        public override void OnAdd()
        {
            temp = new ShieldEffect(3);
            effectContainer.AddEffect(temp,this);
        }
        public override void OnRemove()
        {
            effectContainer.RemoveEffect(temp);
        }
    }
}