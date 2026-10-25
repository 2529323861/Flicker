
namespace Flicker.Module
{
    public enum Quality
    {
        Ordinary,
        Rare,
        Epic,
        Legend
    }
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
        /// <summary>
        /// 稀有度
        /// </summary>
        public Quality quality;
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
        /// 稀有度
        /// </summary>
        public Quality quality = Quality.Ordinary;
        /// <summary>
        /// 道具资源路径
        /// </summary>
        public string path = "Unknown";
        /// <summary>
        /// 道具描述
        /// </summary>
        public string description = "默认描述";
        /// <summary>
        /// 道具冷却时长（秒）。冷却比例 = 剩余 / 该值，该值不参与修饰
        /// </summary>
        public float cooldown = 4f;
        /// <summary>
        /// 使用者的游戏角色控制器绑定
        /// </summary>
        protected GameCharacterController gameCharacterController;
        /// <summary>
        /// buff容器绑定
        /// </summary>
        protected BuffContainer buffContainer;
        /// <summary>
        /// 效果容器绑定
        /// </summary>
        protected EffectContainer effectContainer;
        /// <summary>
        /// 道具容器绑定
        /// </summary>
        protected PropContainer propContainer;
        /// <summary>
        /// 使用道具，容器已在道具入槽时完成控制器与容器绑定
        /// </summary>
        public virtual void OnUse(){}
        /// <summary>
        /// 道具自身的使用条件。不满足则本次使用不发生，也不进入冷却
        /// </summary>
        public virtual bool CanUse(){ return true; }
        /// <summary>
        /// 绑定游戏角色控制器，道具进入容器时调用
        /// </summary>
        /// <param name="gameCharacterController">使用者的游戏角色控制器</param>
        public void BindGameCharacterController(GameCharacterController gameCharacterController)
        {
            this.gameCharacterController = gameCharacterController;
        }
        /// <summary>
        /// 绑定buff容器，道具进入容器时调用
        /// </summary>
        /// <param name="buffContainer">使用者身上的buff容器</param>
        public void BindBuffContainer(BuffContainer buffContainer)
        {
            this.buffContainer = buffContainer;
        }
        /// <summary>
        /// 绑定效果容器，道具进入容器时调用
        /// </summary>
        /// <param name="effectContainer">使用者身上的效果容器</param>
        public void BindEffectContainer(EffectContainer effectContainer)
        {
            this.effectContainer = effectContainer;
        }
        /// <summary>
        /// 绑定道具容器，道具进入容器时调用
        /// </summary>
        /// <param name="propContainer">使用者身上的道具容器</param>
        public void BindPropContainer(PropContainer propContainer)
        {
            this.propContainer = propContainer;
        }
        public BasePropStrategyMetaData GetMetaData()
        {
            BasePropStrategyMetaData basePropStrategyMetaData = new();
            basePropStrategyMetaData.propId = propId;
            basePropStrategyMetaData.name = name;
            basePropStrategyMetaData.factoryId = factoryId;
            basePropStrategyMetaData.path = path;
            basePropStrategyMetaData.description = description;
            basePropStrategyMetaData.quality = quality;
            return basePropStrategyMetaData;
        }
    }
}