using UnityEngine;

namespace Flicker.Module
{   
    /// <summary>
    /// 生命值处理器，负责记录生命值，临时生命值，护盾值，临时护盾值，并处理掉血，护盾代偿逻辑
    /// </summary>
    public class HealthController: MonoBehaviour
    {
        /// <summary>
        /// 生命值
        /// </summary>
        public int health = 3;
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