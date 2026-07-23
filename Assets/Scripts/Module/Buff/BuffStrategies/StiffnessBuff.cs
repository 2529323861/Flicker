using Flicker.Module.Effect;

namespace Flicker.Module.Buff
{
    public class StiffnessBuff: BaseBuffStrategy
    {
        private BaseEffectStrategy temp1;
        private BaseEffectStrategy temp2;
        public StiffnessBuff(){
            name = "僵直";
            description = "无法跳跃且减速50%";
        }
        public override void OnAdd()
        {
            temp1 = new JumpingProhibitionEffect();
            temp2 = new AccelerateEffect(0.5f);
            effectContainer.AddEffect(temp1,this);
            effectContainer.AddEffect(temp2,this);
        }
        public override void OnRemove()
        {
            effectContainer.RemoveEffect(temp1);
            effectContainer.RemoveEffect(temp2);
        }
    }
}