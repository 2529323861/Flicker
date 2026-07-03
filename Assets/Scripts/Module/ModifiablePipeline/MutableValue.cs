using System;

namespace Flicker.Module
{
    /// <summary>
    /// 瞬时变化数据包装类
    /// </summary>
    public class MutableValue
    {
        /// <summary>
        /// 当前值
        /// </summary>
        public float value;
        /// <summary>
        /// 观察者回调，第一个参数是可修饰数据的原始值，第二个参数是可修饰参数的最终值
        /// </summary>
        public Action<float,float> onUpdate;

        public MutableValue(float value)
        {
            this.value = value;
        }
    }
}