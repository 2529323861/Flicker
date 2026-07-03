using Flicker.Module.Effect;

namespace Flicker.Module.Buff
{
    public class ShieldBuff: BaseBuffStrategy
    {
        private BaseEffectStrategy temp;
        public override void OnAdd()
        {
            temp = new ShieldEffect(3);
            gameObject.GetComponent<EffectContainer>().AddEffect(temp,this);
        }
        public override void OnRemove()
        {
            gameObject.GetComponent<EffectContainer>().RemoveEffect(temp);
        }
    }
}