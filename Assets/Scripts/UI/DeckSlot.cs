using Flicker.Core;
using Flicker.Module;
using Flicker.Module.UIEvent;
using UnityEngine;
using UnityEngine.UI;

namespace Flicker.UI
{
    /// <summary>
    /// 卡组中的一格。与 SectionSlot 的区别：可能为空，且以 index 作为身份。
    /// 不持有卡组数据，只做视图与点击上报，数据由面板暂存
    /// </summary>
    public class DeckSlot: MonoBehaviour
    {
        /// <summary>
        /// 托盘
        /// </summary>
        public Image tray;
        /// <summary>
        /// 图标，空槽位时禁用
        /// </summary>
        public Image icon;
        public Button button;
        /// <summary>
        /// 托盘底图，0 = 普通，1 = 选中
        /// </summary>
        public Sprite[] traySprites;

        /// <summary>
        /// 本格在卡组中的下标，由面板调用 Bind 注入
        /// </summary>
        public int Index {get;private set;}
        /// <summary>
        /// 当前道具的工厂 id，空槽位为 null
        /// </summary>
        public string FactoryId {get;private set;}
        void Awake()
        {
            button.onClick.AddListener(()=>
            {
                EventBus.Publish<DeckSlotClickedEvent>(new(){index=Index});
            });
        }
        /// <summary>
        /// 绑定本格在卡组中的位置，面板加载时对每一格调用一次
        /// </summary>
        /// <param name="index">卡组下标</param>
        public void Bind(int index)
        {
            Index = index;
        }
        /// <summary>
        /// 填入道具
        /// </summary>
        /// <param name="basePropStrategyMetaData">道具元信息</param>
        public void SetProp(BasePropStrategyMetaData basePropStrategyMetaData)
        {
            FactoryId = basePropStrategyMetaData.factoryId;
            icon.sprite = Resources.Load<Sprite>("Images/UI/PropIcon/"+basePropStrategyMetaData.path);
            icon.enabled = true;
        }
        /// <summary>
        /// 清空槽位，保留托盘占位
        /// </summary>
        public void Clear()
        {
            FactoryId = null;
            icon.sprite = null;
            icon.enabled = false;
        }
        /// <summary>
        /// 设置选中表现
        /// </summary>
        /// <param name="selected">是否选中</param>
        public void SetSelected(bool selected)
        {
            tray.sprite = traySprites[selected ? 1 : 0];
        }
    }
}
