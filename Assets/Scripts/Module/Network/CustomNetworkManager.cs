using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Flicker.Core;
using Flicker.Data;
using Flicker.Module.UIEvent;
using Flicker.Module.ViewEvent;
using Flicker.UI;
using Mirror;
using Newtonsoft.Json;
using UnityEngine;

namespace Flicker.Module.Network
{
    public enum NetworkConnectionState{
        NoneNetwork,   // 无网络
        Connecting,   // 连接中
        ProxyCreating, // 连接代理创建中
        Ready,         // 准备中
        Exiting,       // 主动离开
        Gaming,        // 游戏中

    }
    class ConnectionProxyWrapper
    {
        public NetworkConnectionToClient conn;
        public ConnectionProxy connectionProxy;
        public bool isReady;
        public string playerName;
        public List<string> deck;
        /// <summary>
        /// 本局创建的角色，结算时用于判定死亡者，对局结束后置空
        /// </summary>
        public GameCharacterController character;
    }
    public class CustomNetworkManager: NetworkManager
    {
        public GameObject gameCharacterPrefab;
        public GameObject mockCharacterPrefab;
        private List<ConnectionProxyWrapper> connectionProxyWrappers=new();
        /// <summary>
        /// 当前的连接状态，仅客户端自身维护，不走rpc
        /// </summary>
        public NetworkConnectionState networkConnectionState = NetworkConnectionState.NoneNetwork;
        /// <summary>
        /// 是否暂停，有别于是否被限制，因为剧情和倒数也能被暂停
        /// </summary>
        public bool IsPaused {get;private set;} = false;
        /// <summary>
        /// 是否被限制，准备倒数，剧情，演出时用于停止逻辑
        /// </summary>
        public bool IsLimited {get;private set;} = false;
        /// <summary>
        /// 顿帧剩余时间。用非缩放时间计时：timeScale 归零后 deltaTime 也为零，
        /// 用普通 deltaTime 将永远走不完
        /// </summary>
        private float hitStopRemaining = 0f;
        /// <summary>
        /// 是否禁用逻辑
        /// </summary>
        public bool IsLogicDisabled {get{return IsPaused||IsLimited;}}

        /// <summary>
        /// 结束锁，首位玩家胜利后，防止接下来的场景演出造成重复的结算调用
        /// </summary>
        private bool endLock = false;

        /// <summary>
        /// 人机模式：单人房间，跳过准备检查并额外生成一个机器人。
        /// 机器人不进 connectionProxyWrappers，因此所有 UI 转发对它天然无效
        /// </summary>
        public bool hasBot = false;


        //———————————————————————————————————钩子—————————————————————————————————————————————
        new void Awake()
        {
            base.Awake();
            EventBus.Subscribe<HostPlayClickedEvent>(HostPlayClickedHandler);
            EventBus.Subscribe<JoinUrlEvent>(JoinUrlHandler);
            EventBus.Subscribe<LobbyPanelQuitEvent>(LobbyPanelQuitHandler);
            EventBus.Subscribe<LobbyPanelAddBotEvent>(LobbyPanelAddBotHandler);
            EventBus.Subscribe<LobbyPanelRemoveBotEvent>(LobbyPanelRemoveBotHandler);
            EventBus.Subscribe<HitStopEvent>(HitStopHandler);
            
        }
        new void OnDestroy() {
            base.OnDestroy();
            EventBus.Unsubscribe<HostPlayClickedEvent>(HostPlayClickedHandler);
            EventBus.Unsubscribe<JoinUrlEvent>(JoinUrlHandler);
            EventBus.Unsubscribe<LobbyPanelQuitEvent>(LobbyPanelQuitHandler);
            EventBus.Unsubscribe<LobbyPanelAddBotEvent>(LobbyPanelAddBotHandler);
            EventBus.Unsubscribe<LobbyPanelRemoveBotEvent>(LobbyPanelRemoveBotHandler);
            EventBus.Unsubscribe<HitStopEvent>(HitStopHandler);
            /** 销毁时务必恢复，否则编辑器里会停在暂停状态 */
            Time.timeScale = 1f;
        }
        public override void Update()
        {
            base.Update();
            if (hitStopRemaining <= 0f) return;

            /** 非缩放计时而 deltaTime 已被自己冻成 0 */
            hitStopRemaining -= Time.unscaledDeltaTime;
            Time.timeScale = hitStopRemaining > 0f ? 0f : 1f;
        }
        /// <summary>
        /// 触发顿帧。取较大值，同时命中不会叠加成明显更长的冻结
        /// </summary>
        public void HitStop(float duration)
        {
            if (duration > hitStopRemaining) hitStopRemaining = duration;
        }
        public void HitStopHandler(HitStopEvent evt)
        {
            HitStop(evt.duration);
        }
        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            if (NetworkServer.connections.Count > 2)
            {
                /** 留空做通知 */
                conn.Disconnect();
                return;
            }
            /** 人机房间只允许一台机器：机器人已占位时，真实玩家一律踢出。
             *  机器人不占 NetworkServer.connections，因此上面的上限拦不住它 */
            if (hasBot)
            {
                conn.Disconnect();
            }
        }
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);
            //注册连接，便于后期管理
            ConnectionProxyWrapper connectionProxyWrapper = new();
            connectionProxyWrapper.conn = conn;
            connectionProxyWrapper.connectionProxy = conn.identity.GetComponent<ConnectionProxy>();
            connectionProxyWrapper.isReady = false;
            connectionProxyWrapper.playerName = "暂无玩家";
            connectionProxyWrapper.deck = new();
            connectionProxyWrappers.Add(connectionProxyWrapper);

            //UI切换
            connectionProxyWrapper.connectionProxy.SwitchToLobbyUI();
            PublishBotState();

        }
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            connectionProxyWrappers.Remove(connectionProxyWrappers.Find((i)=>i.conn==conn));
            if (connectionProxyWrappers.Count!=0)
            {
                PublishLobbyPanelReadySnapshot();
                PublishLobbyPanelPlayerNameUpdate();
            }
        }
        public override void OnClientConnect()
        {
            base.OnClientConnect();
            networkConnectionState = NetworkConnectionState.ProxyCreating;
        }
        /// <summary>
        /// 这个不是Mirror框架的钩子，而是供连接代理创建后调用的钩子，补齐Mirror的生命周期
        /// </summary>
        public void OnClientAddedPlayer(){
            networkConnectionState = NetworkConnectionState.Ready;
            UIManager.Instance.Close<JoinInputPanel>();
        }
        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            switch (networkConnectionState){
                case NetworkConnectionState.Connecting:{
                    UIManager.Instance.Open<TipPanel>(new TipMessage(){title="连接失败",content="可能是服务器地址错误，或服务端已关闭"});
                }break;
                case NetworkConnectionState.ProxyCreating:{
                    UIManager.Instance.Open<TipPanel>(new TipMessage(){title="连接终止",content="目标服务器人数达到上限"});
                }break;
                case NetworkConnectionState.Ready:{
                    UIManager.Instance.Open<TipPanel>(new TipMessage(){title="连接终止",content="目标服务器关闭或你已被踢出"});
                }break;
                case NetworkConnectionState.Gaming:{
                    UIManager.Instance.Open<TipPanel>(new TipMessage(){title="连接终止",content="目标服务器关闭"});
                }break;
                default: {

                }break;
            }
            networkConnectionState = NetworkConnectionState.NoneNetwork;
            UIManager.Instance.CloseAll(UILayer.Normal);
            UIManager.Instance.CloseAll(UILayer.HUD);
            UIManager.Instance.Open<MainPanel>();

        }
        //——————————————————————————————————————————————————————————————————————————
        
        /// <summary>
        /// 服务端事件转发
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="evt"></param>
        public void EventBroadcast<T>(T evt)
        {
            foreach(var wrapper in connectionProxyWrappers)
            {
                wrapper.connectionProxy.EventTransmit(evt);
            }
        }
        /// <summary>
        /// 对局开始时刻，用于结算时计算时长。
        /// 用缩放时间而非非缩放时间：顿帧期间 timeScale 为 0，
        /// 冻结的时间不会计入，得到的时长与实际可操作的帧数一致
        /// </summary>
        private float matchStartTime;

        Coroutine countDownCoroutine;
        public void PrepareMultiPlay()
        {
            endLock = false;
            IsLimited = true;
            /** 倒计时结束后再开算，这样时长只包含真正能操作的时间 */
            matchStartTime = 0f;
            int i = 0;
            foreach(var wrapper in connectionProxyWrappers)
            {
                wrapper.connectionProxy.SwitchToGameUI();
                wrapper.connectionProxy.ShowCountDown();
                wrapper.connectionProxy.ChangeNetworkConnectionState(NetworkConnectionState.Gaming);

                GameObject gameChar = CharacterControllerManager.Instance.Create(gameCharacterPrefab);
                var gcc = gameChar.GetComponent<PlayerController>();
                gcc.teamTag = i == 0 ? "TeamA" : "TeamB";
                gcc.onDestroy = (dead)=>{
                    GameManager.Instance.MultiPlayEnd(dead);
                };
                wrapper.connectionProxy.BindGameObject(gameChar);
                wrapper.character = gcc;

                gameChar.GetComponent<PlayerController>().SetPlayerName(wrapper.playerName);

                if (i == 0)
                {
                    gcc.SetPosition(4.0f,false);
                }
                else
                {
                    gcc.SetPosition(-4.0f,true);
                }
                /** 注入大厅提交的携带道具 */
                gameChar.GetComponent<PropContainer>().Init(ResolveLoadout(wrapper.deck));
                i++;
            }

            if (hasBot)
            {
                GameObject botChar = CharacterControllerManager.Instance.Create(mockCharacterPrefab);
                var bot = botChar.GetComponent<GameCharacterController>();
                /** 机器人没有队伍标签则与玩家同阵营，攻击与弹反都会跳过对方 */
                bot.teamTag = "TeamB";
                /** 不绑死亡回调，机器人死亡不会触发结算，对局永不结束 */
                bot.onDestroy = (dead)=>{
                    GameManager.Instance.MultiPlayEnd(dead);
                };
                bot.SetPosition(-4.0f,true);
                /** 不初始化携带，道具的 CanUse 与 UseProp 会空引用。
                 *  人机从预制卡组中随机挑一套 */
                botChar.GetComponent<PropContainer>().Init(BotLoadouts.PickRandom());
            }

            /** 角色全部生成完再统一上色，bot 与玩家走同一套规则 */
            ApplyTeamColors();

            if (countDownCoroutine != null)
            {
                StopCoroutine(countDownCoroutine);
            }
            countDownCoroutine = StartCoroutine(StartCountDown());
        }
        /// <summary>
        /// 按队伍上色。TeamB（玩家二 / 机器人）为绿色，其余保持预制体原色。
        /// 颜色属于表现，不走逻辑层的事件，而是由角色自己的 ClientRpc 下发
        /// </summary>
        static readonly Color TeamBColor = new(0.388f, 1f, 0f);
        void ApplyTeamColors()
        {
            foreach (var gcc in CharacterControllerManager.Instance.GetAll())
            {
                gcc.SetTeamColor(gcc.teamTag == "TeamB" ? TeamBColor : Color.white);
            }
        }
        /// <summary>
        /// 结算携带道具。大厅已提交合法携带时直接使用，
        /// 否则（调试直开等未提交场景）回退到本地默认携带
        /// </summary>
        List<string> ResolveLoadout(List<string> loadout)
        {
            if (loadout != null && loadout.Count == PropContainer.EquippedPropCount) return loadout;
            return JsonConvert.DeserializeObject<List<string>>(SettingsManager.Instance.GetSetting<string>("pvpDeck"));
        }
        IEnumerator StartCountDown()
        {
            yield return new WaitForSeconds(3f);
            IsLimited = false;
            /** 此处才开始计时：倒计时属于准备阶段，不计入对局时长。
             *  用缩放时间，顿帧期间 timeScale 为 0，冻结的时间自然不计入 */
            matchStartTime = Time.time;
        }
        public void MultiPlayEnd(GameCharacterController dead)
        {
            if (endLock) return;
            endLock = true;
            IsLimited = true;

            if (countDownCoroutine != null)
            {
                StopCoroutine(countDownCoroutine);
            }

            /** 死亡者对应哪个容器序位，另一个即胜者 */
            int deadIndex = connectionProxyWrappers.FindIndex(w => w.character == dead);
            if (deadIndex < 0)
            {
                /** 死亡者是机器人，不在玩家列表里属正常情况。
                 *  deadIndex 保持 -1 向下传，i != -1 恒成立，即全部玩家获胜 */
                if (!hasBot)
                {
                    Debug.LogError("[!] 结算失败：死亡角色既不是玩家也不是机器人");
                    /** 回滚锁，否则对局永久卡在受限状态 */
                    endLock = false;
                    IsLimited = false;
                    return;
                }
            }

            /** 对局结束，回到大厅需要重新准备，卡组保留 */
            foreach(var wrapper in connectionProxyWrappers){
                wrapper.isReady = false;
                wrapper.connectionProxy.ChangeNetworkConnectionState(NetworkConnectionState.Ready);
            }
            PublishLobbyPanelReadySnapshot();
            StartCoroutine(MultiPlayEnd_(deadIndex));
        }
        IEnumerator MultiPlayEnd_(int deadIndex){
            /** 统计数据必须在移除角色之前采集，否则组件已经没了 */
            SettlementArgs[] settlementArgs = new SettlementArgs[connectionProxyWrappers.Count];
            float duration = matchStartTime > 0f ? Time.time - matchStartTime : 0f;
            for (int i = 0; i < connectionProxyWrappers.Count; i++)
            {
                CharacterStats stats = connectionProxyWrappers[i].character != null
                    ? connectionProxyWrappers[i].character.GetComponent<CharacterStats>()
                    : null;

                settlementArgs[i] = new SettlementArgs
                {
                    win = i != deadIndex,
                    duration = duration,
                    jumpCount = stats != null ? stats.jumpCount : 0,
                    parryCount = stats != null ? stats.parryCount : 0,
                    propUseCount = stats != null ? stats.propUseCount : 0
                };
            }

            //这里主要是做1秒提供给场景完成剩余表演，也好让玩家看清是怎么结束的
            yield return new WaitForSeconds(1f);
            CharacterControllerManager.Instance.RemoveAll();
            //弹出结算UI
            for (int i = 0; i < connectionProxyWrappers.Count; i++)
            {
                var wrapper = connectionProxyWrappers[i];
                wrapper.character = null;
                wrapper.connectionProxy.OpenSettlementUI(settlementArgs[i]);
            }
        }
        public void HostStartGame()
        {
            if (hasBot) {
                /** 人机模式：机器人恒准备，只需主机点开始 */
                GameManager.Instance.PrepareMultiPlay();
                return;
                }
            if (connectionProxyWrappers.Count == 2)
            {
                if (connectionProxyWrappers[0].isReady && connectionProxyWrappers[1].isReady)
                {
                    GameManager.Instance.PrepareMultiPlay();
                }
                else
                {
                    UIManager.Instance.Open<TipPanel>(new TipMessage(){title="还不能开始！",content="还有玩家未准备！"});
                }
            }
            else
            {
                UIManager.Instance.Open<TipPanel>(new TipMessage(){title="还不能开始！",content="玩家人数不足！"});
            }
            
        }
        public void SwitchReady(List<string> deck, NetworkConnectionToClient sender)
        {
            foreach (var i in connectionProxyWrappers)
            {
                if (i.conn == sender)
                {
                    i.isReady = !i.isReady;
                    i.deck = deck;
                }
            }
            PublishLobbyPanelReadySnapshot();
        }
        public void PublishLobbyPanelReadySnapshot()
        {
            print("快照被调用");
            LobbyPanelReadySnapshotEvent lobbyPanelReadySnapshotEvent = new();
            /** hasBot 必须排在人数判断之前：机器人占着客户端位但不进玩家列表 */
            if (hasBot)
            {
                lobbyPanelReadySnapshotEvent.client = true;
            }
            else if (connectionProxyWrappers.Count == 1)
            {    
                lobbyPanelReadySnapshotEvent.client = false;
            }
            else
            {
                lobbyPanelReadySnapshotEvent.client = connectionProxyWrappers[1].isReady;
            }
            lobbyPanelReadySnapshotEvent.host = connectionProxyWrappers[0].isReady;

            EventBroadcast(lobbyPanelReadySnapshotEvent);
        }
        public void UpdatePlayerName(string name, NetworkConnectionToClient sender)
        {
            foreach (var i in connectionProxyWrappers)
            {
                if (i.conn == sender)
                {
                    i.playerName = name;
                }
            }
            PublishLobbyPanelPlayerNameUpdate();
        }
        public void PublishLobbyPanelPlayerNameUpdate()
        {
            LobbyPanelPlayerNameUpdateEvent lobbyPanelPlayerNameUpdateEvent = new();
            /** hasBot 必须排在人数判断之前：机器人占着客户端位但不进玩家列表 */
            if (hasBot)
            {    
                lobbyPanelPlayerNameUpdateEvent.client = "机器人";
            }
            else if (connectionProxyWrappers.Count == 1)
            {    
                lobbyPanelPlayerNameUpdateEvent.client = "暂无玩家";
            }
            else
            {
                lobbyPanelPlayerNameUpdateEvent.client = connectionProxyWrappers[1].playerName;
            }
            lobbyPanelPlayerNameUpdateEvent.host = connectionProxyWrappers[0].playerName;
            EventBroadcast(lobbyPanelPlayerNameUpdateEvent);
        }
        //—————————————————————————————————Handler—————————————————————————————————————————————
        public void HostPlayClickedHandler(HostPlayClickedEvent hostPlayClickedEvent)
        {
            try
            {
                print("[+] 启动host服务器");
                StartHost();
            }catch(SocketException se)
            {
                Debug.LogError($"[!] 服务器启动失败：{se}");
                UIManager.Instance.Open<TipPanel>(new TipMessage(){title="服务器启动失败",content="请检查7777端口是否已被其它进程占用"});
                return;
            }
            UIManager.Instance.Close<MainPanel>();
            UIManager.Instance.Open<LobbyPanel>();
        }
        public void JoinUrlHandler(JoinUrlEvent joinUrlEvent)
        {
            networkConnectionState = NetworkConnectionState.Connecting;
            string url = joinUrlEvent.url.Trim();
            url = $"kcp://{url}";
            Debug.Log($"尝试与 {url} 建立连接");
            CustomNetworkManager.singleton.StartClient(new Uri(url));
        }
        public void LobbyPanelQuitHandler(LobbyPanelQuitEvent lobbyPanelQuitEvent){
            networkConnectionState = NetworkConnectionState.Exiting;
            
            if (NetworkServer.active){
                StopHost();
            }else{
                StopClient();
            }
        }
        //—————————————————————————————————机器人—————————————————————————————————————————————
        /// <summary>
        /// 添加机器人。仅大厅内、且没有第二位真实玩家时可用
        /// </summary>
        public void AddBot()
        {
            if (!NetworkServer.active) return;
            if (hasBot) return;
            /** 已有第二位真实玩家，不再插入机器人 */
            if (connectionProxyWrappers.Count != 1)
            {
                UIManager.Instance.Open<TipPanel>(new TipMessage(){title="无法添加",content="房间内已有其他玩家"});
                return;
            }
            /** 对局进行中不允许增删机器人 */
            if (connectionProxyWrappers.Exists(w => w.character != null)) return;

            hasBot = true;
            PublishBotState();
            PublishLobbyPanelReadySnapshot();
            PublishLobbyPanelPlayerNameUpdate();
        }
        /// <summary>
        /// 移除机器人。仅大厅内可用
        /// </summary>
        public void RemoveBot()
        {
            if (!NetworkServer.active) return;
            if (!hasBot) return;
            /** 对局进行中不允许增删机器人 */
            if (connectionProxyWrappers.Exists(w => w.character != null)) return;

            hasBot = false;
            PublishBotState();
            PublishLobbyPanelReadySnapshot();
            PublishLobbyPanelPlayerNameUpdate();
        }
        /// <summary>
        /// 广播机器人存在状态，供大厅按钮刷新文字
        /// </summary>
        public void PublishBotState()
        {
            EventBroadcast(new LobbyPanelBotStateEvent{hasBot = hasBot});
        }
        public void LobbyPanelAddBotHandler(LobbyPanelAddBotEvent lobbyPanelAddBotEvent)
        {
            AddBot();
        }
        public void LobbyPanelRemoveBotHandler(LobbyPanelRemoveBotEvent lobbyPanelRemoveBotEvent)
        {
            RemoveBot();
        }
    }
}