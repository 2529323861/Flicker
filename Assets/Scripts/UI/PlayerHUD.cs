using System.Collections.Generic;
using Flicker.Core;
using Flicker.Module;
using Flicker.Module.CharacterEvent;
using Flicker.Module.UIEvent;
using Mirror.BouncyCastle.Bcpg.Sig;
using Mirror.BouncyCastle.Crypto.Modes;
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
        Image jumpCDImage;
        public GameObject slotPrefab;
        public GameObject slotParent;
        public Sprite[] slotsKey;

        private GameObject[] slots=new GameObject[5];
        private GameObject[] props=new GameObject[5];
        private Image[] propBars = new Image[5];
        /** 缓存每个槽位是否有道具 */
        private bool[] propCache=new bool[5]{false,false,false,false,false};
        /** 缓存每个槽位CD */
        private float[] propCDCache = new float[5]{0f,0f,0f,0f,0f};
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
            EventBus.Subscribe<GetPropEvent>(GetPropHandler);
            EventBus.Subscribe<GetPropNumberEvent>(SetPropSlotNumberHandler);
            EventBus.Subscribe<UpdateJumpCDProgressEvent>(SetJumpCDHandler);
            EventBus.Subscribe<UpdatePropCDProgressEvent>(SetPropCDBarHandler);
            EventBus.Subscribe<UsePropEvent>(UsePropHandler);
        }
        void Start()
        {
            Layer = UILayer.HUD;
        }
        void Update()
        {
            UpdateJumpCDBar(1-jumpCDProgressCache);
            UpdatePropCDBar();
        }
        /// <summary>
        /// 设置jumpdCD的目标值
        /// </summary>
        /// <param name="updateJumpCDProgressEvent"></param>
        public void SetJumpCDHandler(UpdateJumpCDProgressEvent updateJumpCDProgressEvent)
        {
            jumpCDProgressCache = updateJumpCDProgressEvent.jumpCDProgress;
        }
        /// <summary>
        /// 设置跳跃CD条Image
        /// </summary>
        /// <param name="per"></param>
        public void UpdateJumpCDBar(float per)
        {
            var nowAmount = NonlinearDamper.Vector3Damp(new(jumpCDImage.fillAmount,0,0),new(per,0,0)).x;
            jumpCDImage.fillAmount = nowAmount;
        }
        /// <summary>
        /// 设置道具
        /// </summary>
        /// <param name="getPropEvent"></param>
        public void GetPropHandler(GetPropEvent getPropEvent)
        {
            propCache[getPropEvent.slotIndex] = true;
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
        public void SetPropSlotNumberHandler(GetPropNumberEvent getPropNumberEvent)
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
        /// <summary>
        /// 设置道具槽位CD条
        /// </summary>
        /// <param name="updatePropCDProgressEvent"></param>
        public void SetPropCDBarHandler(UpdatePropCDProgressEvent updatePropCDProgressEvent)
        {
            for (int i = 0; i < 5; i++)
            {
                if (propCache[i])
                {
                    propCDCache[i] = 1f;
                }
                else
                {
                    propCDCache[i] = 0f;
                }
            }
            int j=0;
            while (propCache[j] == true && j < 5)
            {
                j++;
            }
            if (j < 5)
            {
                propCDCache[j] = 1-updatePropCDProgressEvent.propCDProgress;
            }
        }
        /// <summary>
        /// 更新道具槽进度条Bar
        /// </summary>
        public void UpdatePropCDBar()
        {
            for (int i = 0; i < 5; i++)
            {
                propBars[i].fillAmount = NonlinearDamper.FloatDamp(propBars[i].fillAmount,propCDCache[i]);
            }
        }
        public void UsePropHandler(UsePropEvent usePropEvent)
        {
            propCache[usePropEvent.propSlotIndex] = false;
            props[usePropEvent.propSlotIndex].SetActive(false);
        }
    }
}