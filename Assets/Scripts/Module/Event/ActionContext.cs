using UnityEngine;

namespace Flicker.Module.Context
{
    /// <summary>
    /// 行为上下文基类，三相共用同一实例：
    /// Before 只读 + 写 cancelled ／ Result 写随事件产生的量 ／ After 全只读
    ///
    /// 字段规则：只放「行为参数」与「随事件产生的量」。
    /// 固有属性一律经 attributes 读，不复制成字段。
    /// </summary>
    public class ActionContext
    {   
        /// <summary>
        /// 实体属性快照，三个相位都只读
        /// </summary>
        public ModifierSet attributes;
        /// <summary>
        /// 仅在 Before 相位可写，置 true 则本次行为不发生
        /// </summary>
        public bool cancelled = false;
    }

    /// <summary>
    /// 攻击上下文
    /// </summary>
    public class AttackContext: ActionContext
    {
        /// <summary>
        /// 被攻击者
        /// </summary>
        public GameObject victim;
        /// <summary>
        /// 本次伤害，基值 = 攻击力属性
        /// </summary>
        public ModifiableValue damage;
    }

    /// <summary>
    /// 受击上下文
    /// </summary>
    public class BeAttackedContext: ActionContext
    {
        /// <summary>
        /// 攻击者
        /// </summary>
        public GameObject attacker;
        /// <summary>
        /// 本次承受的伤害，基值 = 攻击方传入值
        /// </summary>
        public ModifiableValue damage;
    }
}