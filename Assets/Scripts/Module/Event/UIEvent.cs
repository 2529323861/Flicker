
namespace Flicker.Module.UIEvent
{    
    public struct UpdateJumpCDProgressEvent
    {
        public float jumpCDProgress;
    }
    /// <summary>
    /// 获取当前槽位数
    /// </summary>
    public struct GetPropNumberEvent
    {
        /** 当前道具槽位 */
        public int propSlot;
    }
    /// <summary>
    /// 获取新道具事件
    /// </summary>
    public struct GetPropEvent
    {
        /** 新道具 */
        public BasePropStrategy basePropStrategy;
        /** 新道具的位置 */
        public int slotIndex;
    }
    /// <summary>
    /// 获取当前道具CD比
    /// </summary>
    public struct UpdatePropCDProgressEvent
    {
        /** CD比，0到1 */
        public float propCDProgress;
    }
    public struct UsePropEvent
    {
        public int propSlotIndex;
    }
}