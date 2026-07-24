using Flicker.Core;

namespace Flicker.Module
{
    public enum LevelEdge
    {
        /// <summary>
        /// 左上
        /// </summary>
        LeftTop,
        /// <summary>
        /// 右上
        /// </summary>
        RightTop,
        /// <summary>
        /// 左下
        /// </summary>
        LeftBottom,
        /// <summary>
        /// 右下
        /// </summary>
        RightBottom,
    }
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
        protected override void OnDestroy()
        {
        }
        /// <summary>
        /// 玩家进入边缘
        /// </summary>
        /// <param name="levelEdge"></param>
        public void OnPlayerIntoEdge(LevelEdge levelEdge)
        {
            
        }
    }
}