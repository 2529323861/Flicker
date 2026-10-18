using System;
using System.Collections.Generic;
using Flicker.Core;
using Flicker.Data;
using Flicker.Module.UIEvent;
using Flicker.UI;
using Mirror;
using Mirror.BouncyCastle.Bcpg;
using Newtonsoft.Json;
using UnityEngine;

namespace Flicker.Module.Network
{
    /// <summary>
    /// 连接代理，网络连接期间一直存在的网络对象，用于代表玩家，并进行全局事件转发
    /// </summary>
    public class ConnectionProxy: NetworkBehaviour
    {   
        [HideInInspector]
        public IInput input=new KeyInput();
        private PlayerController playerController;
        private PropContainer propContainer;
        private PropCardDeck propCardDeck;
        private BuffContainer buffContainer;

        void Awake()
        {
            
        }
        void Update()
        {
            if (isClient&&isOwned)
            {
                if (input.EnterUp)
                    CmdEnterMove(true);
                if (input.EnterDown)
                    CmdEnterMove(false);
                if (input.ExitUp)
                    CmdExitMove(true);
                if (input.ExitDown)
                    CmdExitMove(false);
                if (input.Jump) CmdJump();
                for (int i = 1; i <= 5; i++)
                {
                    if (input.Number(i))
                    {
                        CmdUseProp(i-1);
                    }
                }
            }
        }
        void OnDestroy()
        {
            if (isLocalPlayer)
            {                
                EventBus.Unsubscribe<LobbyPanelStartEvent>(LobbyPanelStartHandler);
                EventBus.Unsubscribe<LobbyPanelReadyEvent>(LobbyPanelReadyHandler);
                EventBus.Unsubscribe<LobbyPanelReadySnapshotRequest>(LobbyPanelReadySnapshotRequestHandler);
            }
        }
        //—————————————————————————————————————客户端方法—————————————————————————————————————————————
        public override void OnStartLocalPlayer()
        {
            if (isLocalPlayer)
            {
                EventBus.Subscribe<LobbyPanelStartEvent>(LobbyPanelStartHandler);
                EventBus.Subscribe<LobbyPanelReadyEvent>(LobbyPanelReadyHandler);
                EventBus.Subscribe<LobbyPanelReadySnapshotRequest>(LobbyPanelReadySnapshotRequestHandler);

                CmdUpdatePlayerName(SettingsManager.Instance.GetSetting<string>("playerName"));

                ((CustomNetworkManager)CustomNetworkManager.singleton).OnClientAddedPlayer();
            }
            base.OnStartLocalPlayer();

        }
        /// <summary>
        /// 服务端事件转发
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="evt"></param>
        public void EventTransmit<T>(T evt)
        {
            string json = JsonConvert.SerializeObject(evt);
            EventTransmitInner(typeof(T).AssemblyQualifiedName,json);
        }
        [TargetRpc]
        private void EventTransmitInner(string typeName, string json)
        {
            var type = Type.GetType(typeName);
            var evt = JsonConvert.DeserializeObject(json,type);
            EventBus.Publish(type,evt);
        }
        [TargetRpc]
        public void SwitchToGameUI()
        {
            UIManager.Instance.CloseAll(UILayer.Normal);
            UIManager.Instance.Open<PlayerHUD>();
        }
        [TargetRpc]
        public void SwitchToLobbyUI()
        {
            UIManager.Instance.CloseAll(UILayer.HUD);
            UIManager.Instance.Open<LobbyPanel>();
        }
        /// <summary>
        /// 打开结算面板
        /// </summary>
        /// <param name="win">本局是否获胜</param>
        [TargetRpc]
        public void OpenSettlementUI(bool win)
        {
            UIManager.Instance.Open<MultiPlaySettlementPanel>(win);
        }
        [TargetRpc]
        public void ChangeNetworkConnectionState(NetworkConnectionState networkConnectionState)
        {
            ((CustomNetworkManager)CustomNetworkManager.singleton).networkConnectionState = networkConnectionState;
        }
        [TargetRpc]
        public void ShowCountDown()
        {
            UIManager.Instance.Open<CountDown>();
        }
        /// <summary>
        /// 服务端驳回卡组，仅通知提交者本人
        /// </summary>
        [TargetRpc]
        public void RpcDeckRejected()
        {
            UIManager.Instance.Open<TipPanel>(new TipMessage(){title="卡组非法",content="服务端校验未通过，请重新配置卡组后再试"});
        }
        //———————————————————————————————————服务端控制命令————————————————————————————————————————————
        [Command]
        public void CmdUpdatePlayerName(string name, NetworkConnectionToClient sender = null)
        {
            if (sender==null) return;
            ((CustomNetworkManager)CustomNetworkManager.singleton).UpdatePlayerName(name,sender);
        }
        [Command]
        public void CmdReadySnapshotRequest()
        {
            ((CustomNetworkManager)CustomNetworkManager.singleton).PublishLobbyPanelReadySnapshot();
        }
        [Command]
        public void CmdClientReady(List<string> deck, NetworkConnectionToClient sender = null)
        {
            if (sender==null) return;
            /** 卡组合法性由服务端权威校验，不通过则不进入准备状态 */
            if (!PropFactory.Instance.IsLegalDeck(deck, out string reason))
            {
                print($"[!] 卡组非法：{reason}");
                RpcDeckRejected();
                return;
            }
            print("服务端收到准备指令");
            ((CustomNetworkManager)CustomNetworkManager.singleton).SwitchReady(deck,sender);
        }
        [Command]
        public void CmdHostStartGame()
        {
            ((CustomNetworkManager)CustomNetworkManager.singleton).HostStartGame();
        }

        public void BindGameObject(GameObject gameObject)
        {
            if (isClientOnly) return;
            playerController = gameObject.GetComponent<PlayerController>();
            propContainer = gameObject.GetComponent<PropContainer>();
            propCardDeck = gameObject.GetComponent<PropCardDeck>();
            buffContainer = gameObject.GetComponent<BuffContainer>();
            
            buffContainer.onBuffAdd = (evt)=> EventTransmit<AddNewBuffEvent>(evt);
            buffContainer.onBuffRemove = (evt)=> EventTransmit<RemoveBuffEvent>(evt);
            buffContainer.onBuffStackChange = (evt)=> EventTransmit<BuffStackChangeEvent>(evt);
            /** 注入道具UI发布委托 */
            propContainer.onPropAdded = (evt) => EventTransmit<GetPropEvent>(evt);
            propContainer.onPropUse = (evt) => EventTransmit<UsePropEvent>(evt);
            propContainer.onGetPropNumber = (evt)=>EventTransmit<GetPropNumberEvent>(evt);
            propContainer.onUpdatePropCDProgress = (evt)=>EventTransmit<UpdatePropCDProgressEvent>(evt);
            /** 注入跳跃CD发布委托 */
            playerController.onUpdateJumpCDProgress = (evt)=>EventTransmit<UpdateJumpCDProgressEvent>(evt);
        }
        public void ClearBind()
        {
            playerController = null;
            propContainer = null;
            propCardDeck = null;
            buffContainer = null;
        }
        //—————————————————————————————————游戏对象cmd方法———————————————————————————————————————————————
        [Command]
        public void CmdEnterMove(bool isUp)
        {
            playerController?.EnterMove(isUp);
        }
        [Command]
        public void CmdExitMove(bool isUp)
        {
            playerController?.ExitMove(isUp);
        }
        [Command]
        public void CmdJump()
        {
            playerController?.Jump();
        }
        [Command]
        public void CmdUseProp(int i)
        {
            propContainer?.UseProp(i);
        }
        //————————————————————————————————————Handler—————————————————————————————————————————————————
        public void LobbyPanelStartHandler(LobbyPanelStartEvent lobbyPanelStartEvent)
        {
            CmdHostStartGame();
        }
        public void LobbyPanelReadyHandler(LobbyPanelReadyEvent lobbyPanelReadyEvent)
        {
            CmdClientReady(lobbyPanelReadyEvent.deck);
        }
        public void LobbyPanelReadySnapshotRequestHandler(LobbyPanelReadySnapshotRequest lobbyPanelReadySnapshotRequest)
        {
            CmdReadySnapshotRequest();
        }
    }
}