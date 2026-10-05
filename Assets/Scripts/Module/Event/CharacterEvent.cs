using Flicker.Module.Context;
using UnityEngine;

namespace Flicker.Module.CharacterEvent
{
    /// <summary>
    /// 跳跃前事件，决定本次跳跃是否发生
    /// </summary>
    public struct BeforeJumpEvent
    {
        public JumpContext context;
    }

    /// <summary>
    /// 跳跃结果事件，修饰本次跳跃的行为参数
    /// </summary>
    public struct JumpResultEvent
    {
        public JumpContext context;
    }

    /// <summary>
    /// 跳跃后事件，已按 context 执行完毕
    /// </summary>
    public struct AfterJumpEvent
    {
        public JumpContext context;
    }

    /// <summary>
    /// 弹反前事件，决定本次弹反是否发生
    /// </summary>
    public struct BeforeParryEvent
    {
        public ParryContext context;
    }
    /// <summary>
    /// 弹反结果事件，可改写发起方跳跃的上下文
    /// </summary>
    public struct ParryResultEvent
    {
        public ParryContext context;
    }
    /// <summary>
    /// 弹反后事件，已按 context 执行完毕
    /// </summary>
    public struct AfterParryEvent
    {
        public ParryContext context;
    }
    /// <summary>
    /// 攻击前事件，决定本次攻击是否发生
    /// </summary>
    public struct BeforeAttackEvent
    {
        public AttackContext context;
    }
    /// <summary>
    /// 攻击结果事件，修饰本次攻击产生的临时变量
    /// </summary>
    public struct AttackResultEvent
    {
        public AttackContext context;
    }
    /// <summary>
    /// 攻击后事件，已按 context 执行完毕
    /// </summary>
    public struct AfterAttackEvent
    {
        public AttackContext context;
    }

    /// <summary>
    /// 受击前事件，决定本次受击是否发生
    /// </summary>
    public struct BeforeBeAttackedEvent
    {
        public BeAttackedContext context;
    }
    /// <summary>
    /// 受击结果事件，修饰本次承受的伤害
    /// </summary>
    public struct BeAttackedResultEvent
    {
        public BeAttackedContext context;
    }
    /// <summary>
    /// 受击后事件，已按 context 执行完毕
    /// </summary>
    public struct AfterBeAttackedEvent
    {
        public BeAttackedContext context;
    }


    /// <summary>
    /// 移动前事件，决定本次移动是否发生
    /// </summary>
    public struct BeforeMoveEvent
    {
        public MoveContext context;
    }

    /// <summary>
    /// 移动结果事件，修饰本次移动的行为参数
    /// </summary>
    public struct MoveResultEvent
    {
        public MoveContext context;
    }

    /// <summary>
    /// 移动后事件，已按 context 执行完毕
    /// </summary>
    public struct AfterMoveEvent
    {
        public MoveContext context;
    }
    /// <summary>
    /// 落地事件。本次跳跃的全部结算（攻击、僵直、墙面）均已完成。
    /// 单相，不可取消也不可修饰，仅作必然发生的通知
    /// </summary>
    public struct LandEvent
    {
    }

    /// <summary>
    /// 僵直前事件，决定本次僵直是否发生
    /// </summary>
    public struct BeforeStiffenEvent
    {
        public StiffenContext context;
    }

    /// <summary>
    /// 僵直结果事件
    /// </summary>
    public struct StiffenResultEvent
    {
        public StiffenContext context;
    }

    /// <summary>
    /// 僵直后事件，已按 context 执行完毕
    /// </summary>
    public struct AfterStiffenEvent
    {
        public StiffenContext context;
    }

    /// <summary>
    /// 传递引擎更新事件,用于dot效果自治间隔触发
    /// </summary>
    public struct UpdateEvent{}
    /// <summary>
    /// 死亡前事件，决定本次死亡是否发生
    /// </summary>
    public struct BeforeDie
    {
        public DieContext context;
    }
    /// <summary>
    /// 死亡结果事件
    /// </summary>
    public struct DieResultEvent
    {
        public DieContext context;
    }
    /// <summary>
    /// 死亡后事件，已按 context 执行完毕
    /// </summary>
    public struct AfterDie
    {
        public DieContext context;
    }

    public struct StateChangedEvent
    {
        public GameCharacterState state; 
        public bool towardUp;
        public bool isRightWall;
    }

    /// <summary>
    /// 获得buff前事件
    /// </summary>
    public struct BeforeAddBuffEvent
    {
        public AddBuffContext context;
    }
    /// <summary>
    /// 获得buff结果事件，可修饰持续时间
    /// </summary>
    public struct AddBuffResultEvent
    {
        public AddBuffContext context;
    }
    /// <summary>
    /// 获得buff后事件
    /// </summary>
    public struct AfterAddBuffEvent
    {
        public AddBuffContext context;
    }
    /// <summary>
    /// buff叠层前事件
    /// </summary>
    public struct BeforeBuffStackEvent
    {
        public BuffStackContext context;
    }
    /// <summary>
    /// buff叠层结果事件，可修饰持续时间
    /// </summary>
    public struct BuffStackResultEvent
    {
        public BuffStackContext context;
    }
    /// <summary>
    /// buff叠层后事件
    /// </summary>
    public struct AfterBuffStackEvent
    {
        public BuffStackContext context;
    }
    /// <summary>
    /// 移除buff前事件
    /// </summary>
    public struct BeforeRemoveBuffEvent
    {
        public RemoveBuffContext context;
    }
    /// <summary>
    /// 移除buff后事件
    /// </summary>
    public struct AfterRemoveBuffEvent
    {
        public RemoveBuffContext context;
    }
    /// <summary>
    /// 获得道具前事件
    /// </summary>
    public struct BeforeAddPropEvent
    {
        public AddPropContext context;
    }
    /// <summary>
    /// 获得道具结果事件
    /// </summary>
    public struct AddPropResultEvent
    {
        public AddPropContext context;
    }
    /// <summary>
    /// 获得道具后事件
    /// </summary>
    public struct AfterAddPropEvent
    {
        public AddPropContext context;
    }
    /// <summary>
    /// 使用道具前事件
    /// </summary>
    public struct BeforeUsePropEvent
    {
        public UsePropContext context;
    }
    /// <summary>
    /// 使用道具结果事件
    /// </summary>
    public struct UsePropResultEvent
    {
        public UsePropContext context;
    }
    /// <summary>
    /// 使用道具后事件
    /// </summary>
    public struct AfterUsePropEvent
    {
        public UsePropContext context;
    }
}