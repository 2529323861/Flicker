
using System.Collections.Generic;

namespace Flicker.Module.UIEvent
{    
    //—————————————————————————————————PlayerHUD——————————————————————————————————
    public struct UpdateJumpCDProgressEvent
    {
        public float jumpCDProgress;
    }
    /// <summary>
    /// 获取当前槽位数
    /// </summary>
    public struct GetPropNumberEvent
    {
        /** 当前道具槽位 */
        public int propSlot;
    }
    /// <summary>
    /// 获取新道具事件
    /// </summary>
    public struct GetPropEvent
    {
        /** 新道具 */
        public BasePropStrategyMetaData basePropStrategyMetaData;
        /** 新道具的位置 */
        public int slotIndex;
    }
    /// <summary>
    /// 获取当前道具CD比
    /// </summary>
    public struct UpdatePropCDProgressEvent
    {
        /** CD比，0到1 */
        public float propCDProgress;
    }
    public struct UsePropEvent
    {
        public int propSlotIndex;
    }
    /// <summary>
    /// 添加新buff
    /// </summary>
    public struct AddNewBuffEvent
    {
        public BaseBuffStrategyMetaData baseBuffStrategyMetaData;
    }
    /// <summary>
    /// 移除buff
    /// </summary>
    public struct RemoveBuffEvent
    {
        public BaseBuffStrategyMetaData baseBuffStrategyMetaData;
    }
    /// <summary>
    /// buff层数变化
    /// </summary>
    public struct BuffStackChangeEvent
    {
        public BaseBuffStrategyMetaData baseBuffStrategyMetaData;
    }
    /// <summary>
    /// 显示buff详情事件
    /// </summary>
    public struct ShowBuffDescriptionEvent
    {
        public string name;
        public string description;
    }
    /// <summary>
    /// 隐藏buff详情事件
    /// </summary>
    public struct HideBuffDescriptionEvent
    {
        
    }
    //—————————————————————————————————MainPanel——————————————————————————————————
    /// <summary>
    /// 主持游戏被点击
    /// </summary>
    public struct HostPlayClickedEvent{}
    //—————————————————————————————————LobbyPanel——————————————————————————————————
    /// <summary>
    /// 多人游戏房间启动
    /// </summary>
    public struct LobbyPanelStartEvent{}
    public struct LobbyPanelQuitEvent{}
    public struct LobbyPanelReadyEvent{
        public List<string> deck;
    }
    /// <summary>
    /// 多人游戏房间，准备状态快照
    /// </summary>
    public struct LobbyPanelReadySnapshotEvent
    {
        public bool host;
        public bool client;
    }
    public struct LobbyPanelReadySnapshotRequest{}
    public struct LobbyPanelPlayerNameUpdateEvent
    {
        public string host;
        public string client;
    }
    /// <summary>请求全部道具元信息（面板打开时）</summary>
    public struct PropPoolRequestEvent { }

    /// <summary>全部道具元信息（响应）</summary>
    public struct PropPoolEvent
    {
        public List<BasePropStrategyMetaData> metaDatas;
    }

    /// <summary>更新道具一栏的描述框，携带被指向的道具元信息</summary>
    public struct UpdatePropDescriptionEvent
    {
        public BasePropStrategyMetaData metaData;
    }

    /// <summary>卡组格被点击，携带格子下标</summary>
    public struct DeckSlotClickedEvent
    {
        public int index;
    }
        //—————————————————————————————————————JoinInputPanel———————————————————————————————————
    public struct JoinUrlEvent
    {
        public string url;
    }
}