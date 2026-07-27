using System.Collections.Generic;
using System.Net.Sockets;
using Flicker.Core;
using Flicker.Module.UIEvent;
using Flicker.UI;
using Mirror;
using UnityEngine;

namespace Flicker.Module.Network
{
    public class CustomNetworkManager: NetworkManager
    {
        private List<NetworkConnectionToClient> networkConnectionToClients=new();
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

        }
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);

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
            UIManager.Instance.Open<LobbyPanel>();
        }
    }
}