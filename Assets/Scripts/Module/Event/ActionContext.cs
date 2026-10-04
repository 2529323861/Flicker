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
    /// 跳跃上下文
    /// </summary>
    public class JumpContext: ActionContext
    {
        /// <summary>
        /// 本次冲向的墙
        /// </summary>
        public bool toRightWall;
        /// <summary>
        /// 是否无视跳跃冷却
        /// </summary>
        public bool ignoreCD;
        /// <summary>
        /// 弹反后是否反向继续，由弹反相位修改
        /// </summary>
        public bool reverse = true;
    }

    /// <summary>
    /// 移动上下文
    /// </summary>
    public class MoveContext: ActionContext
    {
        /// <summary>
        /// 移动方向，true 向上
        /// </summary>
        public bool isUp;
    }

    /// <summary>
    /// 弹反上下文。弹反发生在跳跃过程中，其结果体现为对发起方跳跃上下文的修改
    /// </summary>
    public class ParryContext: ActionContext
    {
        /// <summary>
        /// 对手
        /// </summary>
        public GameObject opponent;
        /// <summary>
        /// 触发本次弹反的跳跃上下文
        /// </summary>
        public JumpContext jump;
    }

    /// <summary>
    /// 死亡上下文
    /// </summary>
    public class DieContext: ActionContext
    {
    }

    /// <summary>
    /// 添加buff上下文
    /// </summary>
    public class AddBuffContext: ActionContext
    {
        /// <summary>
        /// 本次操作的buff
        /// </summary>
        public BaseBuffStrategy buff;
        /// <summary>
        /// 本次持续时间，基值 = 传入值
        /// </summary>
        public ModifiableValue duration;
    }

    /// <summary>
    /// buff叠层上下文
    /// </summary>
    public class BuffStackContext: ActionContext
    {
        /// <summary>
        /// 本次操作的buff
        /// </summary>
        public BaseBuffStrategy buff;
        /// <summary>
        /// 本次持续时间，基值 = 传入值
        /// </summary>
        public ModifiableValue duration;
    }

    /// <summary>
    /// 移除buff上下文
    /// </summary>
    public class RemoveBuffContext: ActionContext
    {
        /// <summary>
        /// 本次操作的buff
        /// </summary>
        public BaseBuffStrategy buff;
    }

    /// <summary>
    /// 添加道具上下文
    /// </summary>
    public class AddPropContext: ActionContext
    {
        /// <summary>
        /// 本次操作的道具
        /// </summary>
        public BasePropStrategy prop;
    }

    /// <summary>
    /// 使用道具上下文
    /// </summary>
    public class UsePropContext: ActionContext
    {
        /// <summary>
        /// 本次操作的道具
        /// </summary>
        public BasePropStrategy prop;
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