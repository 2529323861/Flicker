using System;
using Flicker.Core;
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
            }
        }
        public override void OnStartLocalPlayer()
        {
            if (isLocalPlayer)
            {
                EventBus.Subscribe<LobbyPanelStartEvent>(LobbyPanelStartHandler);
            }
            base.OnStartLocalPlayer();

        }
        public void EventTransmit<T>(T evt)
        {
            string json = JsonConvert.SerializeObject(evt);
            EventTransmitInner(typeof(T).AssemblyQualifiedName,json);
        }
        [ClientRpc]
        private void EventTransmitInner(string typeName, string json)
        {
            var type = Type.GetType(typeName);
            var evt = JsonConvert.DeserializeObject(json,type);
            EventBus.Publish(type,evt);
        }
        //————————————————————————————————————————————————————————————————————————————————————
        [Command]
        public void CmdStartGame()
        {
            ((CustomNetworkManager)CustomNetworkManager.singleton).StartGame();
        }

        public void BindGameObject(GameObject gameObject)
        {
            if (isClientOnly) return;
            playerController = gameObject.GetComponent<PlayerController>();
            propContainer = gameObject.GetComponent<PropContainer>();
            propCardDeck = gameObject.GetComponent<PropCardDeck>();
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
            /** 暂时不做什么房主才能启动，先把链路跑通再做细枝末节的处理 */
            CmdStartGame();
            UIManager.Instance.Open<PlayerHUD>();
            UIManager.Instance.Close<LobbyPanel>();
        }
    }
}