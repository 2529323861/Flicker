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
        public override void OnAdd()
        {
            miniEventBus.Subscribe<BeforeBeAttackedEvent>(Callback);
        }
        public override void OnRemove()
        {
            miniEventBus.Unsubscribe<BeforeBeAttackedEvent>(Callback);
        }
        public void Callback(BeforeBeAttackedEvent beforeBeAttackedEvent)
        {
            if (beforeBeAttackedEvent.context.cancelled) return;
            beforeBeAttackedEvent.context.cancelled = true;
            num--;
            if (num == 0)
            {
                buffContainer.RemoveBuff(baseBuffStrategy);
            }
        }
    }
}