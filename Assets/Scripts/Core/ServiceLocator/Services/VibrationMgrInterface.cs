using UnityEngine;

namespace Flicker.Core.Service
{
    public interface VibrationMgrInterface
    {
        /// <summary>
        /// 添加振动
        /// </summary>
        /// <param name="maxTime"></param>
        /// <param name="amplitude"></param>
        public void AddVibration(float maxTime, Vector3 amplitude);
        /// <summary>
        /// 移除所有振动
        /// </summary>
        public void RemoveAllVibrations();
    }
}