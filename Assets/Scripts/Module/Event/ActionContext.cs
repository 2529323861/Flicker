namespace Flicker.Module.Context
{
    /// <summary>
    /// 时间上限文，供游戏角色控制器传给触发类效果，包装值类型为引用类型，回传事件取消
    /// </summary>
    public class ActionContext
    {   
        /// <summary>
        /// 事件是否被取消
        /// </summary>
        public bool cancelled = false;
    }
    public class DamageContext: ActionContext
    {
        public float damage;
    }
}