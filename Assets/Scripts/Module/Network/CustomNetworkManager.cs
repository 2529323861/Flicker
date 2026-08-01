using System.Collections.Generic;
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
        //———————————————————————————————————钩子—————————————————————————————————————————————
        new void Awake()
        {
            base.Awake();
            EventBus.Subscribe<HostPlayClickedEvent>(HostPlayClickedHandler);
            
        }
        new void OnDestroy() {
            base.OnDestroy();
            EventBus.Unsubscribe<HostPlayClickedEvent>(HostPlayClickedHandler);
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
            ConnectionProxyWrapper connectionProxyWrapper = new();
            connectionProxyWrapper.conn = conn;
            connectionProxyWrapper.connectionProxy = conn.identity.GetComponent<ConnectionProxy>();
            connectionProxyWrappers.Add(connectionProxyWrapper);

        }
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            connectionProxyWrappers.Remove(connectionProxyWrappers.Find((i)=>i.conn==conn));
        }
        //——————————————————————————————————————————————————————————————————————————
        public void StartGame()
        {
            int i = 0;
            foreach(var wrapper in connectionProxyWrappers)
            {
                wrapper.connectionProxy.SwitchToGameUI();

                GameObject gameChar = Instantiate(gameCharacterPrefab);
                if (i == 0)
                {
                    gameChar.GetComponent<PlayerController>().SetPosition(4.0f,false);
                }
                else
                {
                    gameChar.GetComponent<PlayerController>().SetPosition(-4.0f,true);
                }
                wrapper.connectionProxy.BindGameObject(gameChar);
                NetworkServer.Spawn(gameChar, wrapper.conn);
                i++;
            }
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
                return;
            }
            UIManager.Instance.Close<MainPanel>();
            UIManager.Instance.Open<LobbyPanel>();
        }
    }
}