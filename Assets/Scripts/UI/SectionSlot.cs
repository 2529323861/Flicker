using Flicker.Core;
using Flicker.Module;
using Flicker.Module.UIEvent;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UI;

namespace Flicker.UI
{
    /// <summary>
    /// 道具一栏中的格子
    /// </summary>
    public class SectionSlot: MonoBehaviour
    {
        /// <summary>
        /// 托盘
        /// </summary>
        public Image tray;
        /// <summary>
        /// 图标
        /// </summary>
        public Image icon;
        public Sprite[] traySprites;
        public Button button;
        private BasePropStrategyMetaData basePropStrategyMetaData;
        void Awake()
        {
            button.onClick.AddListener(() =>
            {
                EventBus.Publish<UpdatePropDescriptionEvent>(new(){metaData=basePropStrategyMetaData});
            });
        }
        public void SetProp(BasePropStrategyMetaData basePropStrategyMetaData)
        {
            this.basePropStrategyMetaData = basePropStrategyMetaData;
            icon.sprite = Resources.Load<Sprite>("Images/UI/PropIcon/"+basePropStrategyMetaData.path);
        }
    }
}