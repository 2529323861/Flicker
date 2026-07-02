using Flicker.Core;
using Flicker.Module.CharacterEvent;
using Flicker.Module.Context;
using UnityEngine;

namespace Flicker.Module
{
    // ============================================================
    // 持续性策略（每帧走管线 Apply）
    // ============================================================

    /// <summary>
    /// [持续性] 移速翻倍
    /// </summary>
    public class SpeedBoostStrategy : BaseEffectStrategy
    {
        public override void Apply(ModifierSet set)
        {
            set.moveSpeed.AddMultiplier(2f);
        }
    }

    /// <summary>
    /// [持续性+条件] 血量低于2时攻击伤害翻倍
    /// </summary>
    public class LowHealthRageStrategy : BaseEffectStrategy
    {
        public override void Apply(ModifierSet set)
        {
            var hc = gameObject.GetComponent<HealthController>();
            if (hc == null) return;
            if (hc.health <= 2)
                set.damage.AddMultiplier(2f);
        }
    }

    /// <summary>
    /// [持续性] 跳跃冷却减少30%（乘算）
    /// </summary>
    public class JumpCDReductionStrategy : BaseEffectStrategy
    {
        public override void Apply(ModifierSet set)
        {
            set.maxJumpingCD.AddMultiplier(0.7f);
        }
    }

    /// <summary>
    /// [持续性] 滞空时间翻倍
    /// </summary>
    public class HangTimeExtendStrategy : BaseEffectStrategy
    {
        public override void Apply(ModifierSet set)
        {
            set.hangTime.AddMultiplier(2f);
        }
    }

    /// <summary>
    /// [持续性+条件] 僵直状态下移速惩罚减半
    /// </summary>
    public class StiffSpeedRecoveryStrategy : BaseEffectStrategy
    {
        public override void Apply(ModifierSet set)
        {
            var gcc = gameObject.GetComponent<GameCharacterController>();
            if (gcc == null) return;
            if (gcc.State == GameCharacterState.Stiffness)
                set.moveSpeed.AddMultiplier(2f);  // 僵直时 Move() 里已有 *0.5，这里翻倍抵消
        }
    }

    // ============================================================
    // 触发性策略（通过 MiniEventBus 订阅事件）
    // ============================================================

    /// <summary>
    /// [触发性] 攻击命中后回复1点生命
    /// </summary>
    public class LifeStealOnAttackStrategy : BaseEffectStrategy
    {
        public override void Register(MiniEventBus bus)
        {
            bus.Subscribe<AfterAttackEvent>(OnAfterAttack);
        }
        public override void Unregister(MiniEventBus bus)
        {
            bus.Unsubscribe<AfterAttackEvent>(OnAfterAttack);
        }

        private void OnAfterAttack(AfterAttackEvent evt)
        {
            var hc = gameObject.GetComponent<HealthController>();
            if (hc == null) return;
            hc.SetHealth(hc.health + 1);
        }
    }

    /// <summary>
    /// [触发性+条件] 血量 ≤ 2 时免疫本次伤害
    /// </summary>
    public class LowHealthImmunityStrategy : BaseEffectStrategy
    {
        public override void Register(MiniEventBus bus)
        {
            bus.Subscribe<BeforeBeAttackedEvent>(OnBeforeBeAttacked);
        }
        public override void Unregister(MiniEventBus bus)
        {
            bus.Unsubscribe<BeforeBeAttackedEvent>(OnBeforeBeAttacked);
        }

        private void OnBeforeBeAttacked(BeforeBeAttackedEvent evt)
        {
            var hc = gameObject.GetComponent<HealthController>();
            if (hc == null) return;
            if (hc.health <= 2)
                evt.context.cancelled = true;
        }
    }

    /// <summary>
    /// [触发性] 受击时伤害减半
    /// </summary>
    public class DamageHalveStrategy : BaseEffectStrategy
    {
        public override void Register(MiniEventBus bus)
        {
            bus.Subscribe<BeforeBeAttackedEvent>(OnBeforeBeAttacked);
        }
        public override void Unregister(MiniEventBus bus)
        {
            bus.Unsubscribe<BeforeBeAttackedEvent>(OnBeforeBeAttacked);
        }

        private void OnBeforeBeAttacked(BeforeBeAttackedEvent evt)
        {
            evt.context.damage *= 0.5f;
        }
    }

    /// <summary>
    /// [触发性] 跳跃落地后移速+3，持续2秒（复合：触发性施加子持续性）
    /// </summary>
    public class JumpLandSpeedBurstStrategy : BaseEffectStrategy
    {
        private EffectContainer _container;
        private MiniEventBus _bus;

        public override void Register(MiniEventBus bus)
        {
            _bus = bus;
            _container = gameObject.GetComponent<EffectContainer>();
            bus.Subscribe<AfterJumpEvent>(OnAfterJump);
        }
        public override void Unregister(MiniEventBus bus)
        {
            bus.Unsubscribe<AfterJumpEvent>(OnAfterJump);
        }

        private void OnAfterJump(AfterJumpEvent evt)
        {
            if (_container == null) return;
            var burst = new SpeedBoostStrategy();
            burst.BindGameObject(gameObject);
            _container.AddEffect(burst);
            // TODO: 2秒后自动移除，需要 Buff 管理系统支持计时器
        }
    }

    /// <summary>
    /// [触发性] 免疫僵直
    /// </summary>
    public class StiffImmunityStrategy : BaseEffectStrategy
    {
        public override void Register(MiniEventBus bus)
        {
            bus.Subscribe<BeforeStiffEvent>(OnBeforeStiff);
        }
        public override void Unregister(MiniEventBus bus)
        {
            bus.Unsubscribe<BeforeStiffEvent>(OnBeforeStiff);
        }

        private void OnBeforeStiff(BeforeStiffEvent evt)
        {
            evt.context.cancelled = true;
        }
    }

    /// <summary>
    /// [触发性] 弹反时对对手造成2点伤害
    /// </summary>
    public class ParryCounterStrategy : BaseEffectStrategy
    {
        public override void Register(MiniEventBus bus)
        {
            bus.Subscribe<AfterParryEvent>(OnAfterParry);
        }
        public override void Unregister(MiniEventBus bus)
        {
            bus.Unsubscribe<AfterParryEvent>(OnAfterParry);
        }

        private void OnAfterParry(AfterParryEvent evt)
        {
            var hc = evt.opponent.GetComponent<HealthController>();
            if (hc == null) return;
            hc.SetHealth(hc.health - 2);
        }
    }

    /// <summary>
    /// [触发性] 攻击前伤害+2（固定加值）
    /// </summary>
    public class BonusDamageOnAttackStrategy : BaseEffectStrategy
    {
        public override void Register(MiniEventBus bus)
        {
            bus.Subscribe<BeforeAttackEvent>(OnBeforeAttack);
        }
        public override void Unregister(MiniEventBus bus)
        {
            bus.Unsubscribe<BeforeAttackEvent>(OnBeforeAttack);
        }

        private void OnBeforeAttack(BeforeAttackEvent evt)
        {
            evt.context.damage += 2;
        }
    }
}
