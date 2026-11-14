
using System.Collections.Generic;

namespace Flicker.Module.UIEvent
{    
    //—————————————————————————————————PlayerHUD——————————————————————————————————
    public struct UpdateJumpCDProgressEvent
    {
        public float jumpCDProgress;
    }
    /// <summary>
    /// 装备道具事件。开局携带时逐格发布，局中不再发布
    /// </summary>
    public struct GetPropEvent
    {
        /** 道具元信息 */
        public BasePropStrategyMetaData basePropStrategyMetaData;
        /** 所在位置 */
        public int slotIndex;
    }
    /// <summary>
    /// 道具栏完整状态。每帧推送一次，UI 只做渲染，不维护影子状态
    /// </summary>
    public struct PropStateEvent
    {
        /** 三个携带位的冷却比例，0 = 就绪，1 = 刚使用 */
        public float propCD0;
        public float propCD1;
        public float propCD2;
    }
    public struct UsePropEvent
    {
        public int propSlotIndex;
    }
    /// <summary>
    /// 道具不可用回执。冷却中或道具自身条件不满足时发布，供 UI 播放拒绝表现
    /// </summary>
    public struct PropUnavailableEvent
    {
        public int propSlotIndex;
    }
    //———————————————————————————————MultiPlaySettlementPanel————————————————————————————————
    /// <summary>
    /// 结算面板的入参。由服务端在移除角色之前采集，
    /// 通过 OpenSettlementUI 逐个发给对应玩家
    /// </summary>
    public struct SettlementArgs
    {
        /** 本局是否获胜 */
        public bool win;
        /** 对局时长（秒） */
        public float duration;
        /** 跳跃次数 */
        public int jumpCount;
        /** 弹反次数 */
        public int parryCount;
        /** 道具使用成功次数 */
        public int propUseCount;
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
    /// <summary>添加机器人，仅主机可用</summary>
    public struct LobbyPanelAddBotEvent{}
    /// <summary>移除机器人，仅主机可用</summary>
    public struct LobbyPanelRemoveBotEvent{}
    /// <summary>机器人存在状态快照，供大厅按钮与提示刷新</summary>
    public struct LobbyPanelBotStateEvent
    {
        public bool hasBot;
    }
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
    //—————————————————————————————————————聊天———————————————————————————————————
    /// <summary>
    /// 玩家发送聊天。客户端只发正文，发送者名由服务端从连接信息填入，
    /// 避免客户端伪造他人身份
    /// </summary>
    public struct SendChatEvent
    {
        public string content;
    }
    /// <summary>
    /// 服务端广播到各端的聊天消息。sender 由服务端填写
    /// </summary>
    public struct ChatMessageEvent
    {
        public string sender;
        public string content;
    }
        //—————————————————————————————————————JoinInputPanel———————————————————————————————————
    public struct JoinUrlEvent
    {
        public string url;
    }
}