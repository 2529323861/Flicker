using Flicker.Core.Service;
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

        new void Awake()
        {
            base.Awake();
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
            onUpdateJumpCDProgress = (jumpCDProgress)=>EventBus.Publish<UpdateJumpCDProgressEvent>(new UpdateJumpCDProgressEvent{jumpCDProgress = jumpCDProgress});
        }
        void Start()
        {
            GameManager.Instance.SetPlayerRef(gameObject);
        }
        new void Update()
        {
            base.Update();

            if (InputRouter.Up)
                Move(true);
            else if (InputRouter.Down)
                Move(false);
            if (InputRouter.Jump) Jump();
            for (int i = 1; i <= 5; i++)
            {
                if (InputRouter.Number(i))
                {
                    propContainer.UseProp(i-1);
                }
            }
        }
        protected override void OnIntoEdge(LevelEdge levelEdge)
        {
            LevelManager.Instance.OnPlayerIntoEdge(levelEdge);
        }
    }
}