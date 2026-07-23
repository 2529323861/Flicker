using Flicker.Module.Effect;

namespace Flicker.Module.Buff
{
    public class AccelerateBuff: BaseBuffStrategy
    {
        private BaseEffectStrategy temp;
        public AccelerateBuff(){
            name = "调试加速";
            description = "移动速度上升400%";
        }
        public override void OnAdd()
        {
            temp = new AccelerateEffect(5);
            effectContainer.AddEffect(temp);
        }
        public override void OnRemove()
        {
            effectContainer.RemoveEffect(temp);
        }
    }
}