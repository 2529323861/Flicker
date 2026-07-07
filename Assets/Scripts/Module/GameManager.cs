using Flicker.Core;
using UnityEngine;

namespace Flicker.Module
{
    public class GameManager: Singleton<GameManager>
    {
        /// <summary>
        /// 玩家引用
        /// </summary>
        GameObject playerRef = null;

        public void SetPlayerRef(GameObject player)
        {
            playerRef = player;
        }
        public GameObject GetPlayerRef()
        {
            return playerRef;
        }
    }
}