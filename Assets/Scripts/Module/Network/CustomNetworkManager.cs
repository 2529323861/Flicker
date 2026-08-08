using System;
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
    class ConnectionProxyWrapper
    {
        public NetworkConnectionToClient conn;
        public ConnectionProxy connectionProxy;
    }
    public class CustomNetworkManager: NetworkManager
    {
        public GameObject gameCharacterPrefab;
        private List<ConnectionProxyWrapper> connectionProxyWrappers=new();
        public bool isConnected {get;private set;} = false;
        //———————————————————————————————————钩子—————————————————————————————————————————————
        new void Awake()
        {
            base.Awake();
            EventBus.Subscribe<HostPlayClickedEvent>(HostPlayClickedHandler);
            EventBus.Subscribe<JoinUrlEvent>(JoinUrlHandler);
            
        }
        new void OnDestroy() {
            base.OnDestroy();
            EventBus.Unsubscribe<HostPlayClickedEvent>(HostPlayClickedHandler);
            EventBus.Unsubscribe<JoinUrlEvent>(JoinUrlHandler);
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
            connectionProxyWrappers.Add(connectionProxyWrapper);

            //UI切换
            connectionProxyWrapper.connectionProxy.SwitchToLobbyUI();

        }
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            connectionProxyWrappers.Remove(connectionProxyWrappers.Find((i)=>i.conn==conn));
        }
        public override void OnClientConnect()
        {
            base.OnClientConnect();
            isConnected = true;
        }
        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            if (!isConnected)
            {
                UIManager.Instance.Open<TipPanel>(new TipMessage(){title="连接失败",content="可能是服务器地址错误，或服务端已关闭"});
            }
            isConnected = false;
        }
        //——————————————————————————————————————————————————————————————————————————
        public void PrepareMultiPlay()
        {
            
            int i = 0;
            foreach(var wrapper in connectionProxyWrappers)
            {
                wrapper.connectionProxy.SwitchToGameUI();

                GameObject gameChar = CharacterControllerManager.Instance.Create(gameCharacterPrefab);
                var gcc = gameChar.GetComponent<PlayerController>();
                if (i == 0)
                {
                    gcc.SetPosition(4.0f,false);
                }
                else
                {
                    gcc.SetPosition(-4.0f,true);
                }
                gcc.teamTag = i == 0 ? "TeamA" : "TeamB";
                gcc.onDestroy = ()=>{
                    GameManager.Instance.MultiPlayEnd();
                };
                wrapper.connectionProxy.BindGameObject(gameChar);
                NetworkServer.Spawn(gameChar, wrapper.conn);
                i++;
            }
        }
        public void MultiPlayEnd()
        {
            CharacterControllerManager.Instance.RemoveAll();
            foreach(var wrapper in connectionProxyWrappers){
                wrapper.connectionProxy.ClearBind();
                wrapper.connectionProxy.SwitchToLobbyUI();
            }
        }
        public void HostStartGame()
        {
            GameManager.Instance.PrepareMultiPlay();
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
            string url = joinUrlEvent.url.Trim();
            url = $"kcp://{url}";
            Debug.Log($"尝试与 {url} 建立连接");
            CustomNetworkManager.singleton.StartClient(new Uri(url));
        }
    }
}