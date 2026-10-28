using System;
using System.Collections.Generic;

namespace Flicker.Module
{
    /// <summary>
    /// 实体固有属性的修饰集。
    /// 由实体自身注册，效果只做修饰，不注册。
    /// </summary>
    public class ModifierSet
    {
        /// <summary>
        /// 修饰键。新增属性时：加成员 → 在实体 Awake 里注册
        /// </summary>
        public enum Key
        {
            /// <summary>
            /// 移动速度
            /// </summary>
            MoveSpeed,
            /// <summary>
            /// 滞空时间
            /// </summary>
            HangTime,
            /// <summary>
            /// 最大跳跃冷却
            /// </summary>
            MaxJumpingCD,
            /// <summary>
            /// 跳跃冷却速度
            /// </summary>
            JumpingCDRate,
            /// <summary>
            /// 攻击伤害
            /// </summary>
            Damage,
            /// <summary>
            /// 道具冷却速率。只修饰速率，不修饰冷却上限，比例语义因此恒定
            /// </summary>
            PropCDRate,
            /// <summary>
            /// 僵直时间
            /// </summary>
            StiffenTime,
        }

        /// <summary>
        /// 属性槽位，包含惰性补足的
        /// </summary>
        readonly Dictionary<Key, ModifiableValue> values = new();

        /// <summary>
        /// 实体显式注册过的键，供 Has 与完整性校验使用
        /// </summary>
        readonly HashSet<Key> declared = new();

        /// <summary>
        /// 注册固有属性，仅由实体自身在 Awake 中调用
        /// </summary>
        /// <param name="key">修饰键</param>
        /// <param name="value">可修饰数据</param>
        public void Register(Key key, ModifiableValue value)
        {
            if (!declared.Add(key))
                throw new InvalidOperationException($"属性 {key} 重复注册，检查实体 Awake");
            values[key] = value;
        }

        /// <summary>
        /// 取得可修饰数据供修饰。
        /// 实体没有该属性时补一个基值 0 的槽位，读写都不报错，效果自然无效
        /// </summary>
        /// <param name="key">修饰键</param>
        public ModifiableValue Get(Key key)
        {
            if (!values.TryGetValue(key, out var value))
            {
                value = new ModifiableValue(0f);
                values.Add(key, value);
            }
            return value;
        }

        /// <summary>
        /// 读取最终值，与 Get 同一路径
        /// </summary>
        /// <param name="key">修饰键</param>
        public float Final(Key key)
        {
            return Get(key).FinalValue;
        }

        /// <summary>
        /// 实体是否显式注册过该属性，供条件类效果守卫
        /// </summary>
        /// <param name="key">修饰键</param>
        public bool Has(Key key)
        {
            return declared.Contains(key);
        }

        /// <summary>
        /// 清空所有修饰，供管线每帧重算前调用
        /// </summary>
        public void ClearModify()
        {
            foreach (var value in values.Values)
            {
                value.ClearModify();
            }
        }
    }
}