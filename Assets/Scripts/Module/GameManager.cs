using Flicker.Core;
using Flicker.Module.Network;
using UnityEngine;

namespace Flicker.Module
{
    public enum GameMode
    {
        MultiPlay,
        SinglePlay,
    }
    public class GameManager: Singleton<GameManager>
    {
        public GameMode gameMode {get; private set;}

        public void PrepareMultiPlay()
        {
            gameMode = GameMode.MultiPlay;
            ((CustomNetworkManager)CustomNetworkManager.singleton).PrepareMultiPlay();
        }
        public void MultiPlayEnd()
        {
            ((CustomNetworkManager)CustomNetworkManager.singleton).MultiPlayEnd();
        }
    }
}