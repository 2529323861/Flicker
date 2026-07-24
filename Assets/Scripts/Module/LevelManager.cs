using Flicker.Core;

namespace Flicker.Module
{
    public class LevelManager : Singleton<LevelManager>
    {
        /// <summary>
        /// 默认的游戏边界上限
        /// </summary>
        public float gameEdge;
        /// <summary>
        /// 默认的门的判定上限
        /// </summary>
        public float doorJudge;
        public float leftTop {get; private set;}
        public float rightTop {get; private set;}
        public float leftBottom {get; private set;}
        public float rightBottom {get; private set;}

        protected override void OnSingletonInit()
        {
            leftTop = gameEdge;
            rightTop = gameEdge;
            leftBottom = -gameEdge;
            rightBottom = -gameEdge;
        }
    }
}