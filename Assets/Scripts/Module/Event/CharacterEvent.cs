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
    /// 僵直前事件
    /// </summary>
    public struct BeforeStiffEvent{
        public ActionContext context;
    }

    /// <summary>
    /// 僵直后事件
    /// </summary>
    public struct AfterStiffEvent{}

    /// <summary>
    /// 攻击前事件
    /// </summary>
    public struct BeforeAttackEvent
    {
        public DamageContext context;
        public GameObject victim;
    }
    /// <summary>
    /// 攻击后事件
    /// </summary>
    public struct AfterAttackEvent
    {
        public GameObject victim;
    }

    /// <summary>
    /// 受击前事件
    /// </summary>
    public struct BeforeBeAttackedEvent
    {
        public DamageContext context;
        public GameObject attacker;
    }
    /// <summary>
    /// 受击后事件
    /// </summary>
    public struct AfterBeAttackedEvent
    {
        public GameObject attacker;
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
}