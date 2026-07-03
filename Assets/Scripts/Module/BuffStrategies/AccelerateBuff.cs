using Flicker.Module.Effect;

namespace Flicker.Module.Buff
{
    public class AccelerateBuff: BaseBuffStrategy
    {
        private BaseEffectStrategy temp;
        public override void OnAdd()
        {
            temp = new AccelerateEffect(5);
            gameObject.GetComponent<EffectContainer>().AddEffect(temp);
        }
        public override void OnRemove()
        {
            gameObject.GetComponent<EffectContainer>().RemoveEffect(temp);
        }
    }
}