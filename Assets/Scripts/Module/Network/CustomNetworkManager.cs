using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Flicker.Core;
using Flicker.Module.UIEvent;
using Flicker.UI;
using Mirror;
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
    }
    public class CustomNetworkManager: NetworkManager
    {
        public GameObject gameCharacterPrefab;
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
        /// 是否禁用逻辑
        /// </summary>
        public bool IsLogicDisabled {get{return IsPaused||IsLimited;}}

        /// <summary>
        /// 结束锁，首位玩家胜利后，防止接下来的场景演出造成重复的结算调用
        /// </summary>
        private bool endLock = false;


        //———————————————————————————————————钩子—————————————————————————————————————————————
        new void Awake()
        {
            base.Awake();
            EventBus.Subscribe<HostPlayClickedEvent>(HostPlayClickedHandler);
            EventBus.Subscribe<JoinUrlEvent>(JoinUrlHandler);
            EventBus.Subscribe<LobbyPanelQuitEvent>(LobbyPanelQuitHandler);
            
        }
        new void OnDestroy() {
            base.OnDestroy();
            EventBus.Unsubscribe<HostPlayClickedEvent>(HostPlayClickedHandler);
            EventBus.Unsubscribe<JoinUrlEvent>(JoinUrlHandler);
            EventBus.Unsubscribe<LobbyPanelQuitEvent>(LobbyPanelQuitHandler);
        }
        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
            base.OnServerConnect(conn);
            if (NetworkServer.connections.Count > 2)
            {
                /** 留空做通知 */
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
            connectionProxyWrappers.Add(connectionProxyWrapper);

            //UI切换
            connectionProxyWrapper.connectionProxy.SwitchToLobbyUI();

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
        Coroutine countDownCoroutine;
        public void PrepareMultiPlay()
        {
            endLock = false;
            IsLimited = true;
            int i = 0;
            foreach(var wrapper in connectionProxyWrappers)
            {
                wrapper.connectionProxy.SwitchToGameUI();
                wrapper.connectionProxy.ShowCountDown();
                wrapper.connectionProxy.ChangeNetworkConnectionState(NetworkConnectionState.Gaming);

                GameObject gameChar = CharacterControllerManager.Instance.Create(gameCharacterPrefab);
                var gcc = gameChar.GetComponent<PlayerController>();
                gcc.teamTag = i == 0 ? "TeamA" : "TeamB";
                gcc.onDestroy = ()=>{
                    GameManager.Instance.MultiPlayEnd();
                };
                wrapper.connectionProxy.BindGameObject(gameChar);
                if (i == 0)
                {
                    gcc.SetPosition(4.0f,false);
                }
                else
                {
                    gcc.SetPosition(-4.0f,true);
                }
                i++;
            }
            if (countDownCoroutine != null)
            {
                StopCoroutine(countDownCoroutine);
            }
            countDownCoroutine = StartCoroutine(StartCountDown());
        }
        IEnumerator StartCountDown()
        {
            yield return new WaitForSeconds(3f);
            IsLimited = false;
        }
        public void MultiPlayEnd()
        {
            if (endLock) return;
            endLock = true;
            IsLimited = true;

            if (countDownCoroutine != null)
            {
                StopCoroutine(countDownCoroutine);
            }

            foreach(var wrapper in connectionProxyWrappers){
                wrapper.connectionProxy.ChangeNetworkConnectionState(NetworkConnectionState.Ready);
            }
            StartCoroutine(MultiPlayEnd_());
        }
        IEnumerator MultiPlayEnd_(){
            //这里主要是做2秒提供给场景完成剩余表演，也好让玩家看清是怎么结束的
            yield return new WaitForSeconds(2f);
            CharacterControllerManager.Instance.RemoveAll();
            //弹出结算UI
            foreach (var i in connectionProxyWrappers)
            {
                i.connectionProxy.OpenSettlementUI();
            }
        }
        public void HostStartGame()
        {
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
        public void SwitchReady(NetworkConnectionToClient sender)
        {
            foreach (var i in connectionProxyWrappers)
            {
                if (i.conn == sender)
                {
                    i.isReady = !i.isReady;
                }
            }
            PublishLobbyPanelReadySnapshot();
        }
        public void PublishLobbyPanelReadySnapshot()
        {
            print("快照被调用");
            LobbyPanelReadySnapshotEvent lobbyPanelReadySnapshotEvent = new();
            if (connectionProxyWrappers.Count == 1)
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
            if (connectionProxyWrappers.Count == 1)
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
    }
}