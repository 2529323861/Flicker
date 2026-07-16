using UnityEngine;

namespace Flicker.Module
{   
    /// <summary>
    /// 生命值处理器，负责记录生命值
    /// </summary>
    public class HealthController: MonoBehaviour
    {
        /// <summary>
        /// 生命值
        /// </summary>
        public int health;
        /// <summary>
        /// 减少生命值
        /// </summary>
        /// <param name="damage"></param>
        public void ReduceHealth(int damage)
        {
            health -= damage;
        }

    }
}