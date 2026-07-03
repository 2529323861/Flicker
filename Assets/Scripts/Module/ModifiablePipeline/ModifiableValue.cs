using System.Collections.Generic;

namespace Flicker.Module
{
    /// <summary>
    /// 可修饰数据包装类
    /// </summary>
    public class ModifiableValue
    {
        /// <summary>
        /// 原始值
        /// </summary>
        float _baseValue;
        public float BaseValue { get{ return _baseValue; } }

        /// <summary>
        /// 最终值，供管线导出
        /// </summary>
        public float FinalValue
        {
            get
            {                
                float result = BaseValue;
                /** 优先计算加法区 */
                foreach (float i in additives)
                {
                    result += i;
                }
                /** 在加法区的基础上处理乘法区 */
                foreach (float i in multipliers)
                {
                    result *= i;
                }
                return result;

            }

        }
        List<MutableValue> observers = new();

        /// <summary>
        /// 加法区
        /// </summary>
        List<float> additives = new();

        /// <summary>
        /// 乘法区
        /// </summary>
        List<float> multipliers = new();
        public ModifiableValue(float baseValue){
            _baseValue = baseValue;
        }

        /// <summary>
        /// 添加加法区因子
        /// </summary>
        /// <param name="additive">因子，正数加，负数减</param>
        public void AddAdditive(float additive)
        {
            additives.Add(additive);
        }
        /// <summary>
        /// 添加乘法区因子
        /// </summary>
        /// <param name="multiplier">因子，直接相乘不会+1，提升百分比请自行+1</param>
        public void AddMultiplier(float multiplier)
        {
            multipliers.Add(multiplier);
        }
        public void NotifyAllObservers()
        {
            foreach (var i in observers)
            {
                i.onUpdate(BaseValue, FinalValue);
            }
        }
        public void AddObserver(MutableValue observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
        }
        public void RemoveObserver(MutableValue observer)
        {
            if (observers.Contains(observer))
            {
                observers.Remove(observer);
            }
        }
        public void ClearModify()
        {
            additives.Clear();
            multipliers.Clear();
        }
    }
}