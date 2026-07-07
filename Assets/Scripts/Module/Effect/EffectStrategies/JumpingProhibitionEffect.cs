using Flicker.Core;
using Flicker.Module.CharacterEvent;

namespace Flicker.Module.Effect
{
    /// <summary>
    /// 跳跃禁止效果
    /// </summary>
    public class JumpingProhibitionEffect : BaseEffectStrategy
    {
        public override void Register(MiniEventBus miniEventBus)
        {
            miniEventBus.Subscribe<BeforeJumpEvent>(CallBack);
        }
        public override void Unregister(MiniEventBus miniEventBus)
        {
            miniEventBus.Unsubscribe<BeforeJumpEvent>(CallBack);
        }
        public void CallBack(BeforeJumpEvent beforeJumpEvent)
        {
            beforeJumpEvent.context.cancelled = true;
        }
    }
}