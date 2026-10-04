using Unity.Mathematics;
using UnityEngine;

namespace Flicker.Core
{
    /// <summary>
    /// 上限 / 比例换算。集中处理"上限为 0"的退化情况。
    /// 比例语义：0 = 无剩余（就绪），1 = 剩余拉满（刚使用）
    /// </summary>
    public static class Ratio
    {
        /// <summary>
        /// 无剩余（就绪）
        /// </summary>
        public const float Empty = 0f;
        /// <summary>
        /// 剩余拉满（刚使用）
        /// </summary>
        public const float Full = 1f;

        /// <summary>
        /// 已知上限，求比例
        /// </summary>
        /// <param name="value">当前值</param>
        /// <param name="max">上限，非正数视为无剩余</param>
        /// <returns>比例，[0,1]</returns>
        public static float ToRatio(float value, float max)
        {
            return max > 0f ? Mathf.Clamp01(value / max) : Empty;
        }

        /// <summary>
        /// 已知上限与比例，求实际值
        /// </summary>
        /// <param name="ratio">比例</param>
        /// <param name="max">上限，非正数视为无值</param>
        /// <returns>实际值</returns>
        public static float ToValue(float ratio, float max)
        {
            return max > 0f ? ratio * max : 0f;
        }

        /// <summary>
        /// 已知上限与比例，按增量推进比例
        /// </summary>
        /// <param name="ratio">当前比例</param>
        /// <param name="max">上限，非正数视为无剩余</param>
        /// <param name="delta">增量，以"值"为单位（如 速率 * 时间）</param>
        /// <returns>推进后的比例，[0,1]</returns>
        public static float Advance(float ratio, float max, float delta)
        {
            return max > 0f ? Mathf.Clamp01(ratio + delta / max) : Empty;
        }
    }

    public static class NonlinearDamper
    {
        /// <summary>
        /// 浮点数非线性阻尼
        /// </summary>
        /// <param name="current">当前值</param>
        /// <param name="target">目标值</param>
        /// <param name="tau">系数，[0,1)，越大越快，超出属于未定义行为</param>
        /// <returns>当前帧的值</returns>
        public static float FloatDamp(float current, float target, float tau=0.1f)
        {
            float factor = 1f - math.exp(-Time.deltaTime / tau);
            return Mathf.Lerp(current,target,factor);
        }

        /// <summary>
        /// 三维向量非线性阻尼
        /// </summary>
        /// <param name="current">当前值</param>
        /// <param name="target">目标值</param>
        /// <param name="tau">系数，[0,1)，越大越快，超出属于未定义行为</param>
        /// <returns>当前帧的值</returns>
        public static Vector3 Vector3Damp(Vector3 current, Vector3 target, float tau=0.1f)
        {
            float factor = 1f - math.exp(-Time.deltaTime / tau);
            return Vector3.Lerp(current,target,factor);
        }

        /// <summary>
        /// 欧拉角非线性阻尼
        /// </summary>
        /// <param name="current">当前值</param>
        /// <param name="target">目标值</param>
        /// <param name="tau">系数，[0,1)，越大越快，超出属于未定义行为</param>
        /// <returns>当前帧的值</returns>
        public static Vector3 EulerAngleDamp(Vector3 current, Vector3 target, float tau=0.1f)
        {
            float factor = 1f - math.exp(-Time.deltaTime / tau);
            float x = Mathf.LerpAngle(current.x, target.x, factor);
            float y = Mathf.LerpAngle(current.y, target.y, factor);
            float z = Mathf.LerpAngle(current.z, target.z, factor);
            return new Vector3(x, y, z);
        }
    }
}