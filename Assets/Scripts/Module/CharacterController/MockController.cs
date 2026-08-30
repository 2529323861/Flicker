using UnityEngine;
using Flicker.Module;
using Flicker.Module.Buff;
using Flicker.Core;
using Flicker.Module.UIEvent;
using TMPro;
using Mirror;

namespace Flicker.Module
{   
    public enum MockState
    {
        targeting,
        movingTo,
        attack
    }
    public class MockController : GameCharacterController
    {
        public TMP_Text playerName;
        private GameObject target;
        PropContainer propContainer;
        private MockState mockState;

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

        }
        void Start()
        {
            // 生命周期正常走，客户端空壳不做事
        }
        protected override void Update()
        {
            base.Update();   // GCC 内部已处理 _isLogicActive 守卫
            if (!isServer) return;  // 客户端空壳：逻辑空转
            if (target == null)
            {
                mockState = MockState.targeting;
                var gccs = CharacterControllerManager.Instance.GetAll();
                if (gccs.Count==1) return;
                foreach (var i in gccs)
                {
                    if (i.teamTag != teamTag)
                    {
                        target = i.gameObject;
                        ExitMove(true);
                        ExitMove(false);
                        break;
                    }
                }
                if (target==null) return;
            }
            float deltaY = target.gameObject.transform.localPosition.y-transform.localPosition.y;
            
            if (deltaY > 1)
            {
                if (mockState != MockState.movingTo)
                {
                    ExitMove(true);
                    ExitMove(false);
                    EnterMove(true);
                    mockState = MockState.movingTo;
                }
            }else if (deltaY < -1)
            {
                if (mockState != MockState.movingTo)
                {
                    ExitMove(true);
                    ExitMove(false);
                    EnterMove(false);
                    mockState = MockState.movingTo;
                }
            }
            else
            {
                if (mockState == MockState.movingTo)
                {
                    mockState = MockState.attack;
                }
            }
            if (mockState == MockState.attack)
            {
                Jump();
            }
        }
        protected override void OnIntoEdge(LevelEdge levelEdge)
        {
            LevelManager.Instance.OnPlayerIntoEdge(levelEdge);
        }
        [ClientRpc]
        public void SetPlayerName(string name)
        {
            playerName.text = name;
        }
    }
}