using Flicker.Module.CharacterEvent;

namespace Flicker.Module.Buff.PropBuff
{
    /// <summary>
    /// 空转轴：立刻清空全部道具冷却，但之后道具冷却速率降低，
    /// 到自身下一次使用道具后自动移除（清空冷却换来的那次使用不计）
    /// </summary>
    public class IdleShaftBuff : BaseBuffStrategy
    {
        /// <summary>
        /// 道具冷却速率倍率
        /// </summary>
        public const float PropCDRateMultiplier = 0.8f;

        BaseEffectStrategy baseEffectStrategy;
        PropContainer propContainer;
        /// <summary>
        /// 是否已过清空冷却换来的那一次使用
        /// </summary>
        bool usedOnce = false;
        public IdleShaftBuff()
        {
            name = "空转轴";
            /** 暂无美术资源，补齐 BuffIcon/IdleShaft.png 后改为 "IdleShaft" */
            path = "IdleShaft";
            description = "立刻清空全部道具冷却，但道具冷却速率降低20%，下次使用道具后解除";
            isStackable = true;
        }
        public override void OnAdd()
        {
            this.propContainer = effectContainer.GetComponent<PropContainer>();
            /** 先清空冷却，不然后挂的道具会立刻进入冷却 */
            propContainer.ClearAllCD();

            this.baseEffectStrategy = new BaseEffectStrategy();
            baseEffectStrategy.apply = (b, m) => m.Get(ModifierSet.Key.PropCDRate).AddMultiplier(PropCDRateMultiplier);
            baseEffectStrategy.onAdd = (b) => b.Listen<AfterUsePropEvent>(OnUsedProp);
            effectContainer.AddEffect(baseEffectStrategy, this);
        }
        public override void OnRemove()
        {
            effectContainer.RemoveEffect(baseEffectStrategy);
        }
        private void OnUsedProp(AfterUsePropEvent afterUsePropEvent)
        {
            /** 第一次是本次使用自身造成的，不计入惩罚周期 */
            if (!usedOnce)
            {
                usedOnce = true;
                return;
            }
            buffContainer.RemoveBuff(this);
        }
    }
}
