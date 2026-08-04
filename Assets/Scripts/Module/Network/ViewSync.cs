using System;
using Flicker.Core;
using Flicker.Module.CharacterEvent;
using Mirror;
using Newtonsoft.Json;
using UnityEngine;

namespace Flicker.Module.Network
{
    /// <summary>
    /// 同步视图，用于侵入性更改网络游戏对象的行为，并充当对象级事件转发中枢
    /// </summary>
    public class ViewSync: NetworkBehaviour
    {
        /// <summary>
        /// 所有游戏角色的逻辑层组件，用于客户端时统一禁用
        /// </summary>
        public MiniEventBus miniEventBus;
        private bool _isReplayed=false;
        public override void OnStartClient()
        {
            var playerController = GetComponent<PlayerController>();
            if (playerController!=null){
                playerController.input = new FalseInput();
            }
        }
        public override void OnStartServer()
        {
            miniEventBus.publish = (t, evt) =>
            {
                if (!_isReplayed){
                    EventTransmit(t,evt);
                }
            };
        }

        public void EventTransmit(Type t, object evt)
        {
            if (t!=typeof(StateChangedEvent)) return;
            string json = JsonConvert.SerializeObject(evt);
            EventTransmitInner(t.AssemblyQualifiedName,json);
        }
        [ClientRpc]
        private void EventTransmitInner(string typeName, string json)
        {
            _isReplayed = true;
            var type = Type.GetType(typeName);
            var evt = JsonConvert.DeserializeObject(json,type);
            miniEventBus.Publish(type,evt);
            _isReplayed = false;
        }
    }
}