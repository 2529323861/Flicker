using Flicker.Core;
using Flicker.Module.CharacterEvent;

namespace Flicker.Module.Effect
{
    /// <summary>
    /// 护盾效果：在受击前取消本次受击，抵挡够次数后移除所属buff
    /// </summary>
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
            /** 已被其它护盾取消，本次不消耗层数 */
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