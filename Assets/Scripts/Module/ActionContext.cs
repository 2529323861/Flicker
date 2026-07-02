namespace Flicker.Module.Context
{
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