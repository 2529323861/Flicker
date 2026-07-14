
namespace Flicker.Module
{
    /// <summary>
    /// 道具卡策略体基类
    /// 包含元数据和策略钩子
    /// </summary>
    public class BasePropStrategy
    {
        /// <summary>
        /// 道具名称
        /// </summary>
        public string name = "默认卡牌";
        /// <summary>
        /// 道具id
        /// </summary>
        public string id = "00000";
        /// <summary>
        /// 道具资源路径
        /// </summary>
        public string path = "RushGel";
        /// <summary>
        /// 道具描述
        /// </summary>

        public string description = "默认描述";
        public virtual void OnUse(){}
    }
}