
namespace Flicker.Module
{
    public struct BasePropStrategyMetaData
    {
        /// <summary>
        /// 道具实例唯一id，代替引用跨层传递
        /// </summary>
        public int propId;
        /// <summary>
        /// 道具名称
        /// </summary>
        public string name;
        /// <summary>
        /// 道具id
        /// </summary>
        public string factoryId;
        /// <summary>
        /// 道具资源路径
        /// </summary>
        public string path;
        /// <summary>
        /// 道具描述
        /// </summary>
        public string description;
    }
    /// <summary>
    /// 道具卡策略体基类
    /// 包含元数据和策略钩子
    /// </summary>
    public class BasePropStrategy
    {
        private static int propIder = 0;
        public BasePropStrategy()
        {
            this.propId = propIder;
            propIder++;
        }
        /// <summary>
        /// 道具实例唯一id，代替引用跨层传递
        /// </summary>
        public int propId {get;private set;}
        /// <summary>
        /// 道具名称
        /// </summary>
        public string name = "Unknown";
        /// <summary>
        /// 工厂用于创建时映射的id
        /// </summary>
        public string factoryId = "00000";
        /// <summary>
        /// 道具资源路径
        /// </summary>
        public string path = "Unknown";
        /// <summary>
        /// 道具描述
        /// </summary>
        public string description = "默认描述";
        public virtual void OnUse(BuffContainer buffContainer, EffectContainer effectContainer){}
        public BasePropStrategyMetaData GetMetaData()
        {
            BasePropStrategyMetaData basePropStrategyMetaData = new();
            basePropStrategyMetaData.propId = propId;
            basePropStrategyMetaData.name = name;
            basePropStrategyMetaData.factoryId = factoryId;
            basePropStrategyMetaData.path = path;
            basePropStrategyMetaData.description = description;
            return basePropStrategyMetaData;
        }
    }
}