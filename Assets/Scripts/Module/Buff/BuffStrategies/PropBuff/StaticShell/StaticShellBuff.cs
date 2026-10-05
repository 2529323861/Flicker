using Flicker.Module.CharacterEvent;
using UnityEngine;

namespace Flicker.Module.Buff.PropBuff
{
    /// <summary>
    /// 静电甲壳：受到弹反时，为自身附加应激，为攻击来源附加触电。
    /// 有效期由 Buff 持续时间控制，期间每次被弹反都会触发
    /// </summary>
    public class StaticShellBuff : BaseBuffStrategy
    {
        BaseEffectStrategy baseEffectStrategy;
        public StaticShellBuff()
        {
            name = "静电甲壳";
            path = "StaticShell";
            description = "受到弹反时，为自身附加应激，为攻击来源附加触电";
            isStackable = true;
        }
        public override void OnAdd()
        {
            this.baseEffectStrategy = new BaseEffectStrategy();
            baseEffectStrategy.onAdd = (b) => b.Listen<AfterParryEvent>(OnParried);
            effectContainer.AddEffect(baseEffectStrategy, this);
        }
        public override void OnRemove()
        {
            effectContainer.RemoveEffect(baseEffectStrategy);
        }
        private void OnParried(AfterParryEvent afterParryEvent)
        {
            /** 自身附加应激 */
            buffContainer.AddBuff(new StressBuff(), -1f);

            /** 攻击来源附加触电 */
            GameObject source = afterParryEvent.context.opponent;
            if (source == null) return;
            source.GetComponent<BuffContainer>().AddBuff(new ElectricShockBuff(), -1f);
        }
    }
}
