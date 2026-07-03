using Flicker.Core;
using Flicker.Module.CharacterEvent;

namespace Flicker.Module.Effect
{
    public class ShieldEffect: BaseEffectStrategy
    {
        int num;
        public ShieldEffect(int num)
        {
            this.num = num;
        }
        public override void Register(MiniEventBus miniEventBus)
        {
            miniEventBus.Subscribe<BeforeBeAttackedEvent>(Callback);
        }
        public override void Unregister(MiniEventBus miniEventBus)
        {
            miniEventBus.Unsubscribe<BeforeBeAttackedEvent>(Callback);
        }
        public void Callback(BeforeBeAttackedEvent beforeBeAttackedEvent)
        {
            beforeBeAttackedEvent.context.cancelled = true;
            num--;
            if (num == 0)
            {
                gameObject.GetComponent<BuffContainer>().RemoveBuff(baseBuffStrategy);
            }
        }
    }
}