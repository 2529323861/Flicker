using Flicker.Module.Context;
using UnityEngine;

namespace Flicker.Module.CharacterEvent
{
    /// <summary>
    /// 跳跃前事件
    /// </summary>
    public struct BeforeJumpEvent
    {
        public ActionContext context;
        public bool toRightWall;
    }

    /// <summary>
    /// 跳跃后事件
    /// </summary>
    public struct AfterJumpEvent
    {
        public bool toRightWall;
    }

    /// <summary>
    /// 弹反前事件
    /// </summary>
    public struct BeforeParryEvent
    {
        public ActionContext context;
        public GameObject opponent;
    }
    /// <summary>
    /// 弹反后事件
    /// </summary>
    public struct AfterParryEvent
    {
        public GameObject opponent;
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
    /// 移动前事件
    /// </summary>
    public struct BeforeMoveEvent
    {
        public ActionContext context;
        public bool isUp;
    }

    /// <summary>
    /// 移动后事件
    /// </summary>
    public struct AfterMoveEvent
    {
        public bool isUp;
    }
    /// <summary>
    /// 传递引擎更新事件,用于dot效果自治间隔触发
    /// </summary>
    public struct UpdateEvent{}
    /// <summary>
    /// 死亡前事件
    /// </summary>
    public struct BeforeDie
    {
        public ActionContext context;
    }
    /// <summary>
    /// 死亡后事件
    /// </summary>
    public struct AfterDie{}

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
        public ActionContext context;
        public BaseBuffStrategy baseBuffStrategy;
    }
    /// <summary>
    /// 获得buff后事件
    /// </summary>
    public struct AfterAddBuffEvent{}
    /// <summary>
    /// buff叠层前事件
    /// </summary>
    public struct BeforeBuffStackEvent
    {
        public ActionContext context;
        public BaseBuffStrategy baseBuffStrategy;
    }
    /// <summary>
    /// buff叠层后事件
    /// </summary>
    public struct AfterBuffStackEvent{}
    /// <summary>
    /// 移除buff前事件
    /// </summary>
    public struct BeforeRemoveBuffEvent
    {
        public ActionContext context;
        public BaseBuffStrategy baseBuffStrategy;
    }
    /// <summary>
    /// 移除buff后事件
    /// </summary>
    public struct AfterRemoveBuffEvent{}
    /// <summary>
    /// 获得道具前事件
    /// </summary>
    public struct BeforeAddPropEvent
    {
        public ActionContext context;
        public BasePropStrategy basePropStrategy;
    }
    /// <summary>
    /// 获得道具后事件
    /// </summary>
    public struct AfterAddPropEvent{}
    /// <summary>
    /// 使用道具前事件
    /// </summary>
    public struct BeforeUsePropEvent
    {
        public ActionContext context;
        public BasePropStrategy basePropStrategy;
    }
    /// <summary>
    /// 使用道具后事件
    /// </summary>
    public struct AfterUsePropEvent{}
}