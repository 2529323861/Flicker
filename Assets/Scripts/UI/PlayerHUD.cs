using System.Collections.Generic;
using Flicker.Core;
using Flicker.Module;
using Flicker.Module.CharacterEvent;
using UnityEngine;
using UnityEngine.UI;

namespace Flicker.UI
{
    /// <summary>
    /// 玩家HUD，游戏时始终显示
    /// </summary>
    public class PlayerHUD: UIPanel
    {
        public GameObject jumpCDProgress;
        public float maxMaskHeight;

        Image jumpCDImage;
        GameObject player = null;
        GameCharacterController gcc;
        MiniEventBus miniEventBus;
        public GameObject slotPrefab;
        public GameObject slotParent;
        public Sprite[] slotsKey;

        private GameObject[] slots=new GameObject[5];
        private GameObject[] props=new GameObject[5];
        private Image[] propBars = new Image[5];
        private bool[] propCache=new bool[5]{false,false,false,false,false};
        void Awake()
        {
            jumpCDImage =  jumpCDProgress.GetComponent<Image>();
            for (int i = 0; i < 5; i++)
            {
                GameObject gameObject =  Instantiate(slotPrefab);
                gameObject.transform.SetParent(slotParent.transform);
                gameObject.transform.localPosition = new(7f+i*134f,32,0);

                slots[i] = gameObject;
                props[i] = gameObject.transform.Find("Prop").gameObject;
                props[i].SetActive(false);
                propBars[i] = gameObject.transform.Find("SlotCD").gameObject.GetComponent<Image>();
                gameObject.transform.Find("Key").gameObject.GetComponent<Image>().sprite = slotsKey[i];
            }
        }
        void Start()
        {
            Layer = UILayer.HUD;
        }
        void Update()
        {
            if (player == null)
            {
                player = GameManager.Instance.GetPlayerRef();
                if (player == null) return;
                gcc = player.GetComponent<GameCharacterController>();
                miniEventBus = player.GetComponent<MiniEventBus>();
                miniEventBus.Subscribe<PropSlotUpdateEvent>(SetPropSlot);
            }
            SetJumpCDBar(1-gcc.nowJumpingCDMV.value/gcc.maxJumpingCDMV.FinalValue);
        }
        /// <summary>
        /// 设置跳跃CD条
        /// </summary>
        /// <param name="per"></param>
        public void SetJumpCDBar(float per)
        {
            var nowAmount = NonlinearDamper.Vector3Damp(new(jumpCDImage.fillAmount,0,0),new(per,0,0)).x;
            jumpCDImage.fillAmount = nowAmount;
        }
        public void SetPropSlot(PropSlotUpdateEvent propSlotUpdateEvent)
        {
            for (int i = 0; i < 5; i++)
            {
                if (i >= propSlotUpdateEvent.propslot)
                {
                    slots[i].SetActive(false);
                }
                else
                {
                    if (propSlotUpdateEvent.basePropStrategies[i] == null)
                    {
                        props[i].SetActive(false);
                    }
                    else
                    {
                        props[i].SetActive(true);
                        Image image = props[i].GetComponent<Image>();
                        image.sprite = Resources.Load<Sprite>("Images/UI/PropIcon/"+propSlotUpdateEvent.basePropStrategies[i].path);
                    }
                }
            }
        }
    }
}