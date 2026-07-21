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
            buffContainer.onBuffAdd = (baseBuffStrategy)=> EventBus.Publish<AddNewBuffEvent>(new AddNewBuffEvent{baseBuffStrategy = baseBuffStrategy});
            buffContainer.onBuffRemove = (baseBuffStrategy)=> EventBus.Publish<RemoveBuffEvent>(new RemoveBuffEvent{baseBuffStrategy = baseBuffStrategy});
            buffContainer.onBuffStackChange = (baseBuffStrategy)=> EventBus.Publish<BuffStackChangeEvent>(new BuffStackChangeEvent{baseBuffStrategy = baseBuffStrategy});
            /** 注入道具UI发布委托 */
            PropContainer propContainer = GetComponent<PropContainer>();
            propContainer.onPropAdded = (basePropStrategy,i) => EventBus.Publish<GetPropEvent>(new GetPropEvent{basePropStrategy = basePropStrategy,slotIndex = i,});
            propContainer.onPropUse = (propSlotIndex) => EventBus.Publish<UsePropEvent>(new UsePropEvent{propSlotIndex = propSlotIndex});
            propContainer.onGetPropNumber = (propSlot)=>EventBus.Publish<GetPropNumberEvent>(new GetPropNumberEvent{propSlot = propSlot});
            propContainer.onUpdatePropCDProgress = (p)=>EventBus.Publish<UpdatePropCDProgressEvent>(new UpdatePropCDProgressEvent{propCDProgress=p});
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
    }
}