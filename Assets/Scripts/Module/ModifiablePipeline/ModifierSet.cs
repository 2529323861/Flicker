using Flicker.Module;

namespace Flicker.Module
{
    /// <summary>
    /// 游戏角色控制器修饰集，
    /// </summary>
    public class ModifierSet
    {
        /// <summary>
        /// 移动速度
        /// </summary>
        public ModifiableValue moveSpeed;
        /// <summary>
        /// 滞空时间
        /// </summary>
        public ModifiableValue  hangTime;
        /// <summary>
        /// 最大跳跃冷却
        /// </summary>
        public ModifiableValue maxJumpingCD;
        /// <summary>
        /// 跳跃冷却速度
        /// </summary>
        public ModifiableValue jumpingCDRate;
        /// <summary>
        /// 攻击伤害
        /// </summary>
        public ModifiableValue damage;
        /// <summary>
        /// 道具栏槽位
        /// </summary>
        public ModifiableValue propSlot;
        /// <summary>
        /// 获取道具卡时间
        /// </summary>
        public ModifiableValue getPropCD;

        /// <summary>
        /// 获取道具卡冷却速率
        /// </summary>
        public ModifiableValue getPropCDRate;

        /// <summary>
        /// 僵直时间
        /// </summary>
        public ModifiableValue stiffenTime;
        /// <summary>
        /// 剩余僵直时间
        /// </summary>
        public MutableValue nowStiffTime;
        /// <summary>
        /// 剩余跳跃冷却时间
        /// </summary>
        public MutableValue nowJumpingCD;
        /// <summary>
        /// 剩余道具获取时间
        /// </summary>
        public MutableValue nowGetPropCD;

        public void ClearModify()
        {
            moveSpeed.ClearModify();
            hangTime.ClearModify();
            maxJumpingCD.ClearModify();
            jumpingCDRate.ClearModify();
            damage.ClearModify();
            propSlot.ClearModify();
            getPropCD.ClearModify();
            getPropCDRate.ClearModify();
            stiffenTime.ClearModify();
        }
    }
}