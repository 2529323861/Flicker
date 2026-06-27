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
        /// 临时生命值，优先于生命值被消耗
        /// </summary>
        public int temporaryHealth = 1;
        /// <summary>
        /// 护盾值，优先于临时生命值被消耗
        /// </summary>
        public int shield = 3;
        /// <summary>
        /// 临时护盾值，优先于护盾值被消耗
        /// </summary>
        public int temporaryShield = 1;

        public void SetHealth(int value)
        {
            health = value;
            if (health < 0) health = 0;
            if (health == 0) Destroy(gameObject);
        }

    }
}