using Unity.Mathematics;
using UnityEngine;

namespace Flicker.Core
{
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