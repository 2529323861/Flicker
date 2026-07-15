using System.Collections.Generic;
using Flicker.Core;
using Flicker.Module;
using Flicker.Module.CharacterEvent;
using Flicker.Module.UIEvent;
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
        private float jumpCDProgressCache=0;
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
            EventBus.Subscribe<GetPropEvent>(SetProp);
            EventBus.Subscribe<GetPropNumberEvent>(SetPropSlotNumber);
            EventBus.Subscribe<UpdateJumpCDProgressEvent>(SetJumpCD);
        }
        void Start()
        {
            Layer = UILayer.HUD;
        }
        void Update()
        {
            SetJumpCDBar(1-jumpCDProgressCache);
        }
        /// <summary>
        /// 设置jumpdCD的目标值
        /// </summary>
        /// <param name="updateJumpCDProgressEvent"></param>
        public void SetJumpCD(UpdateJumpCDProgressEvent updateJumpCDProgressEvent)
        {
            jumpCDProgressCache = updateJumpCDProgressEvent.jumpCDProgress;
        }
        /// <summary>
        /// 设置跳跃CD条Image
        /// </summary>
        /// <param name="per"></param>
        public void SetJumpCDBar(float per)
        {
            var nowAmount = NonlinearDamper.Vector3Damp(new(jumpCDImage.fillAmount,0,0),new(per,0,0)).x;
            jumpCDImage.fillAmount = nowAmount;
        }
        /// <summary>
        /// 设置道具
        /// </summary>
        /// <param name="getPropEvent"></param>
        public void SetProp(GetPropEvent getPropEvent)
        {
            /** 设置可见并切换材质 */
            props[getPropEvent.slotIndex].SetActive(true);
            Image image = props[getPropEvent.slotIndex].GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>("Images/UI/PropIcon/"+getPropEvent.basePropStrategy.path);

            props[getPropEvent.slotIndex].GetComponent<Animator>().Play("PropPop");

        }
        /// <summary>
        /// 设置道具槽位数
        /// </summary>
        /// <param name="propSlot"></param>
        public void SetPropSlotNumber(GetPropNumberEvent getPropNumberEvent)
        {
            int propSlot = getPropNumberEvent.propSlot;
            for (int i = 0; i < 5; i++)
            {
                if (i >= propSlot)
                {
                    slots[i].SetActive(false);
                }
                else
                {
                    slots[i].SetActive(true);
                }
            }
        }
    }
}