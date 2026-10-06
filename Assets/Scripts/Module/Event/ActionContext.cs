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
    /// 跳跃上下文。飞行期间持有全部轨迹状态，由 Coroutine 逐帧推进，
    /// 效果层通过修改上下文字段来干预轨迹
    /// </summary>
    public class JumpContext: ActionContext
    {
        /// <summary>
        /// 本次飞行的终点墙。起点墙为其反面，二者必然相反
        /// </summary>
        public bool toRightWall;

        /// <summary>
        /// 本次滞空时长，起跳时快照，仅用于把时间增量换算为进度增量
        /// </summary>
        public float hangTime;
        /// <summary>
        /// 飞行进度，0 为出发墙，1 为终点墙。
        /// 由飞行协程逐帧推进，效果层可直接改写以改变轨迹
        /// </summary>
        public float progress;

        /// <summary>
        /// 折返：进度翻折并调转终点墙，落点随之回到出发墙。
        ///
        /// 这是纯状态操作，不做任何通知——调用两次即完全还原。
        /// 因此效果可以"提前折返"来抵消弹反帧的折返（不偏矢即如此实现）。
        /// 需要广播"发生了折返"时应另立事件，不要在此处添加副作用
        /// </summary>
        public void FoldBack()
        {
            progress = 1f - progress;
            toRightWall = !toRightWall;
        }
    }

    /// <summary>
    /// 僵直上下文
    /// </summary>
    public class StiffenContext: ActionContext
    {
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