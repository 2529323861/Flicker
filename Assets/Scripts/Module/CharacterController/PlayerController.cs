using UnityEngine;
using Flicker.Module;
using Flicker.Module.Buff;
using Flicker.Core;
using Flicker.Module.UIEvent;

namespace Flicker.Module
{   
    public class PlayerController : GameCharacterController
    {
        PropContainer propContainer;

        protected override void Awake()
        {
            base.Awake();   // GCC 内部已处理 _isLogicActive 守卫
            propContainer = GetComponent<PropContainer>();
            InjectUIEvent();
        }
        /// <summary>
        /// 注入UI事件
        /// </summary>
        public void InjectUIEvent()
        {
            /** 注入buffUI发布委托 */
            BuffContainer buffContainer = GetComponent<BuffContainer>();
            buffContainer.onBuffAdd = (evt)=> EventBus.Publish<AddNewBuffEvent>(evt);
            buffContainer.onBuffRemove = (evt)=> EventBus.Publish<RemoveBuffEvent>(evt);
            buffContainer.onBuffStackChange = (evt)=> EventBus.Publish<BuffStackChangeEvent>(evt);
            /** 注入道具UI发布委托 */
            PropContainer propContainer = GetComponent<PropContainer>();
            propContainer.onPropAdded = (evt) => EventBus.Publish<GetPropEvent>(evt);
            propContainer.onPropUse = (evt) => EventBus.Publish<UsePropEvent>(evt);
            propContainer.onGetPropNumber = (evt)=>EventBus.Publish<GetPropNumberEvent>(evt);
            propContainer.onUpdatePropCDProgress = (evt)=>EventBus.Publish<UpdatePropCDProgressEvent>(evt);
            /** 注入跳跃CD发布委托 */
            onUpdateJumpCDProgress = (evt)=>EventBus.Publish<UpdateJumpCDProgressEvent>(evt);
        }
        void Start()
        {
            // 生命周期正常走，客户端空壳不做事
        }
        protected override void Update()
        {
            base.Update();   // GCC 内部已处理 _isLogicActive 守卫
            if (!isServer) return;  // 客户端空壳：逻辑空转
        }
        protected override void OnIntoEdge(LevelEdge levelEdge)
        {
            LevelManager.Instance.OnPlayerIntoEdge(levelEdge);
        }
    }
}