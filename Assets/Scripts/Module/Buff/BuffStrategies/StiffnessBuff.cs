using Flicker.Module.Effect;

namespace Flicker.Module.Buff
{
    public class StiffnessBuff: BaseBuffStrategy
    {
        private BaseEffectStrategy temp1;
        private BaseEffectStrategy temp2;
        public StiffnessBuff(){
            description = "僵直！无法跳跃且减速50%";
        }
        public override void OnAdd()
        {
            temp1 = new JumpingProhibitionEffect();
            temp2 = new AccelerateEffect(0.5f);
            gameObject.GetComponent<EffectContainer>().AddEffect(temp1,this);
            gameObject.GetComponent<EffectContainer>().AddEffect(temp2,this);
        }
        public override void OnRemove()
        {
            gameObject.GetComponent<EffectContainer>().RemoveEffect(temp1);
            gameObject.GetComponent<EffectContainer>().RemoveEffect(temp2);
        }
    }
}