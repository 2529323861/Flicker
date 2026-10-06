using Flicker.Module.CharacterEvent;

namespace Flicker.Module.Buff.PropBuff
{
    /// <summary>
    /// 空转轴：道具获取冷却上升20%，持续一个完整周期，
    /// 到下一次抽到道具时自动移除（清空CD导致的那次即时抽卡不计）
    /// </summary>
    public class IdleShaftBuff : BaseBuffStrategy
    {
        /// <summary>
        /// 道具获取冷却倍率
        /// </summary>
        public const float GetPropCDMultiplier = 1.2f;

        BaseEffectStrategy baseEffectStrategy;
        /// <summary>
        /// 是否已抽到过因清空CD而即时发放的那一张
        /// </summary>
        bool drawnOnce = false;
        public IdleShaftBuff()
        {
            name = "空转轴";
            /** 暂无美术资源，补齐 BuffIcon/IdleShaft.png 后改为 "IdleShaft" */
            path = "IdleShaft";
            description = "道具获取冷却上升20%，下次抽到道具后解除";
            isStackable = true;
        }
        public override void OnAdd()
        {
            this.baseEffectStrategy = new BaseEffectStrategy();
            baseEffectStrategy.apply = (b, m) => m.Get(ModifierSet.Key.GetPropCD).AddMultiplier(GetPropCDMultiplier);
            baseEffectStrategy.onAdd = (b) => b.Listen<AfterAddPropEvent>(OnGotProp);
            effectContainer.AddEffect(baseEffectStrategy, this);
        }
        public override void OnRemove()
        {
            effectContainer.RemoveEffect(baseEffectStrategy);
        }
        private void OnGotProp(AfterAddPropEvent afterAddPropEvent)
        {
            /** 第一张是清空CD换来的，不计入惩罚周期 */
            if (!drawnOnce)
            {
                drawnOnce = true;
                return;
            }
            buffContainer.RemoveBuff(this);
        }
    }
}
