using System;
using System.Collections.Generic;
using Flicker.Core;
using Flicker.Module;
using Flicker.Module.CharacterEvent;
using Flicker.Module.UIEvent;
using Mirror.BouncyCastle.Bcpg.Sig;
using Mirror.BouncyCastle.Crypto.Modes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Flicker.UI
{
    /// <summary>
    /// UI buff的包装类，包含基准位置，buff策略体和ui对象
    /// </summary>
    public class BuffWrapper
    {
        public BaseBuffStrategyMetaData baseBuffStrategyMetaData;
        public GameObject buffUI;
        public Vector3 reference;
    }
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
        public GameObject buff;
        public GameObject buffParent;
        public GameObject buffDescription;

        private GameObject[] slots=new GameObject[5];
        private GameObject[] props=new GameObject[5];
        private Image[] propBars = new Image[5];

        /** 缓存每个槽位是否有道具 */
        private bool[] propCache=new bool[5]{false,false,false,false,false};
        /** 缓存每个槽位CD */
        private float[] propCDCache = new float[5]{0f,0f,0f,0f,0f};
        private float jumpCDProgressCache=0;

        /// <summary>
        /// UI层BUff缓存
        /// </summary>
        private List<BuffWrapper> buffWrappers=new();
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
            EventBus.Subscribe<AddNewBuffEvent>(AddNewBuffHandler);
            EventBus.Subscribe<RemoveBuffEvent>(RemoveBuffHandler);
            EventBus.Subscribe<BuffStackChangeEvent>(BuffStackChangeHandler);
            EventBus.Subscribe<ShowBuffDescriptionEvent>(ShowBuffDescriptionHandler);
            EventBus.Subscribe<HideBuffDescriptionEvent>(HideBuffDescriptionHandler);

        }
        void Start()
        {
        }
        void Update()
        {
            UpdateJumpCDBar(1-jumpCDProgressCache);
            UpdatePropCDBar();
            ResetBuffPos();
        }
        public override void OnOpen(object args = null)
        {
            base.OnOpen(args);
            ClearCache();
            InitBuffIcon();
            foreach (var i in props)
            {
                i.SetActive(false);
            }
        }
        /// <summary>
        /// 清除缓存
        /// </summary>
        public void ClearCache()
        {
            propCache=new bool[5]{false,false,false,false,false};
            jumpCDProgressCache=0;
        }
        /// <summary>
        /// 初始化buff图标，移除所有残留图标
        /// </summary>
        public void InitBuffIcon()
        {
            foreach (var i in buffWrappers)
            {
                Destroy(i.buffUI);
            }
            buffWrappers.Clear();
        }
        //—————————————————————————————————跳跃CD部分———————————————————————————————————————————
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
        //—————————————————————————————————道具槽部分———————————————————————————————————————————
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
            image.sprite = Resources.Load<Sprite>("Images/UI/PropIcon/"+getPropEvent.basePropStrategyMetaData.path);

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
        //—————————————————————————————————buff图标部分———————————————————————————————————————————
        /// <summary>
        /// buff图标间距
        /// </summary>
        private float spacing = 70;
        public void AddNewBuffHandler(AddNewBuffEvent addNewBuffEvent)
        {
            if (!addNewBuffEvent.baseBuffStrategyMetaData.iconVisible) return;
            GameObject buffUI = Instantiate(buff);
            buffUI.transform.SetParent(buffParent.transform);
            buffUI.transform.localPosition = new(buffWrappers.Count*spacing,0,0);
            
            Image image = buffUI.GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>("Images/UI/BuffIcon/"+addNewBuffEvent.baseBuffStrategyMetaData.path);

            var description =  buffUI.GetComponent<BuffDescription>();
            description.buffName = addNewBuffEvent.baseBuffStrategyMetaData.name;
            description.description = addNewBuffEvent.baseBuffStrategyMetaData.description;

            GameObject stack = buffUI.transform.Find("Stack").gameObject;
            if (addNewBuffEvent.baseBuffStrategyMetaData.stacks > 1)
            {
                stack.SetActive(true);
                stack.GetComponent<TextMeshProUGUI>().text = addNewBuffEvent.baseBuffStrategyMetaData.stacks.ToString();
            }
            else
            {
                stack.SetActive(false);
            }

            BuffWrapper buffWrapper = new()
            {
                baseBuffStrategyMetaData = addNewBuffEvent.baseBuffStrategyMetaData,
                buffUI = buffUI,
                reference = new(buffWrappers.Count*spacing,0,0)
            };
            buffWrappers.Add(buffWrapper);
        }
        public void RemoveBuffHandler(RemoveBuffEvent removeBuffEvent)
        {
            int i=0;
            while (i<buffWrappers.Count && buffWrappers[i].baseBuffStrategyMetaData.buffId != removeBuffEvent.baseBuffStrategyMetaData.buffId)
            {
                i++;
            }
            if (i>=buffWrappers.Count) return;
            Destroy(buffWrappers[i].buffUI);
            buffWrappers.RemoveAt(i);
            for (int j = 0; j < buffWrappers.Count; j++)
            {
                buffWrappers[j].reference.x = j*spacing;
            }
        }
        public void BuffStackChangeHandler(BuffStackChangeEvent buffStackChangeEvent)
        {
            int i=0;
            while (i<buffWrappers.Count && buffWrappers[i].baseBuffStrategyMetaData.buffId != buffStackChangeEvent.baseBuffStrategyMetaData.buffId)
            {
                i++;
            }
            if (i>=buffWrappers.Count) return;
            GameObject stack = buffWrappers[i].buffUI.transform.Find("Stack").gameObject;
            if (buffStackChangeEvent.baseBuffStrategyMetaData.stacks > 1)
            {
                stack.SetActive(true);
                stack.GetComponent<TextMeshProUGUI>().text = buffStackChangeEvent.baseBuffStrategyMetaData.stacks.ToString();
            }
            else
            {
                stack.SetActive(false);
            }
        }
        /// <summary>
        /// 复位buff图标坐标
        /// </summary>
        public void ResetBuffPos()
        {
            foreach (var i in buffWrappers)
            {
                var rt = i.buffUI.GetComponent<RectTransform>();
                rt.anchoredPosition = NonlinearDamper.Vector3Damp(rt.anchoredPosition,i.reference);
            }
        }
        //—————————————————————————————————buff描述框———————————————————————————————————————————
        /// <summary>
        /// 显示buff描述框
        /// </summary>
        /// <param name="showBuffDescriptionEvent"></param>
        public void ShowBuffDescriptionHandler(ShowBuffDescriptionEvent showBuffDescriptionEvent)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                GetComponent<RectTransform>(),
                Input.mousePosition,
                null,
                out var localPoint);
            buffDescription.GetComponent<RectTransform>().anchoredPosition = localPoint;


            var descriptionTray = buffDescription.transform.Find("Root/BuffDescriptionTray");
            descriptionTray.Find("name").GetComponent<TextMeshProUGUI>().text = showBuffDescriptionEvent.name;

            var i = descriptionTray.Find("description").GetComponent<TextMeshProUGUI>();
            i.text = showBuffDescriptionEvent.description;
            var lineHeight = i.GetPreferredValues(i.text,i.GetComponent<RectTransform>().sizeDelta.x,0).y;
            var rt = descriptionTray.GetComponent<RectTransform>();
            rt.sizeDelta = new(rt.sizeDelta.x,lineHeight+50f);

            buffDescription.SetActive(true);
            buffDescription.GetComponent<Animator>().Play("UIShow");
        }
        /// <summary>
        /// 隐藏buff描述框
        /// </summary>
        /// <param name="hideBuffDescriptionEvent"></param>
        public void HideBuffDescriptionHandler(HideBuffDescriptionEvent hideBuffDescriptionEvent)
        {
            buffDescription.SetActive(false);
        }
    }
}