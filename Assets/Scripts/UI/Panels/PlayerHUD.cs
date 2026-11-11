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

        private GameObject[] slots=new GameObject[PropContainer.EquippedPropCount];
        private GameObject[] props=new GameObject[PropContainer.EquippedPropCount];
        private Image[] propBars = new Image[PropContainer.EquippedPropCount];
        private Animator[] propAnimators = new Animator[PropContainer.EquippedPropCount];

        [Header("道具栏动画")]
        [Tooltip("道具不可用时播放的动画状态名，动画写在 Prop 图标的 Animator 上")]
        public string propUnavailableAnimState = "PropDisable";
        [Tooltip("道具冷却结束时就绪时播放的动画状态名")]
        public string propReadyAnimState = "PropPop";

        /** 缓存每个道具的悬停描述组件，复用 BuffDescription */
        private BuffDescription[] propDescriptions = new BuffDescription[PropContainer.EquippedPropCount];
        /** 每个道具的就绪度目标值，1 = 就绪，0 = 刚使用。只作阻尼目标，不是逻辑状态 */
        private float[] propCDTarget = new float[PropContainer.EquippedPropCount];
        private float jumpCDProgressCache=0;

        //—————————————————————————————————道具栏反馈状态————————————————————————————————————
        /** 图标原始颜色，用于灰化后还原 */
        private Color[] propOrigColor = new Color[PropContainer.EquippedPropCount];
        /// <summary>
        /// 冷却中图标的颜色系数
        /// </summary>
        private static readonly Color DimColor = new(0.35f,0.35f,0.35f,1f);

        /// <summary>
        /// UI层BUff缓存
        /// </summary>
        private List<BuffWrapper> buffWrappers=new();
        void Awake()
        {
            jumpCDImage =  jumpCDProgress.GetComponent<Image>();
            for (int i = 0; i < PropContainer.EquippedPropCount; i++)
            {
                GameObject gameObject =  Instantiate(slotPrefab);
                gameObject.transform.SetParent(slotParent.transform);
                gameObject.transform.localPosition = new(7f+i*134f,32,0);

                slots[i] = gameObject;
                props[i] = gameObject.transform.Find("Prop").gameObject;
                props[i].SetActive(false);
                propBars[i] = gameObject.transform.Find("SlotCD").gameObject.GetComponent<Image>();
                gameObject.transform.Find("Key").gameObject.GetComponent<Image>().sprite = slotsKey[i];
                /** 道具图标复用 BuffDescription 的悬停能力 */
                propDescriptions[i] = props[i].AddComponent<BuffDescription>();
                /** 图标动画器，用于获取道具 Pop 与不可用抖动 */
                propAnimators[i] = props[i].GetComponent<Animator>();
                /** 记录原始颜色，灰化后还原 */
                propOrigColor[i] = props[i].GetComponent<Image>().color;
            }
            EventBus.Subscribe<GetPropEvent>(GetPropHandler);
            EventBus.Subscribe<UpdateJumpCDProgressEvent>(SetJumpCDHandler);
            EventBus.Subscribe<PropStateEvent>(PropStateHandler);
            EventBus.Subscribe<PropUnavailableEvent>(PropUnavailableHandler);
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
            UpdatePropFeedback();
            ResetBuffPos();
        }
        public override void OnOpen(object args = null)
        {
            base.OnOpen(args);
            /** HUD 只在对局中出现，因此这里等价于「进入战斗」这一时刻。
             *  PlayMusic 会忽略同一首的重复请求，重复开 HUD 不会重播 */
            Flicker.View.AudioManager.Instance.PlayMusic("Battle");
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
            propCDTarget=new float[PropContainer.EquippedPropCount];
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
            /** 设置可见并切换材质 */
            props[getPropEvent.slotIndex].SetActive(true);
            Image image = props[getPropEvent.slotIndex].GetComponent<Image>();
            image.sprite = Resources.Load<Sprite>("Images/UI/PropIcon/"+getPropEvent.basePropStrategyMetaData.path);

            /** 同步悬停描述内容 */
            propDescriptions[getPropEvent.slotIndex].buffName = getPropEvent.basePropStrategyMetaData.name;
            propDescriptions[getPropEvent.slotIndex].description = getPropEvent.basePropStrategyMetaData.description;
        }
        /// <summary>
        /// 渲染道具栏。快照是全量状态，同一份调用多次结果一致
        /// </summary>
        public void PropStateHandler(PropStateEvent propStateEvent)
        {
            ApplyPropSnapshot(0, propStateEvent.propCD0);
            ApplyPropSnapshot(1, propStateEvent.propCD1);
            ApplyPropSnapshot(2, propStateEvent.propCD2);
        }
        /// <summary>
        /// 应用单个道具的快照，并检出「刚变就绪」的跳变
        /// </summary>
        private void ApplyPropSnapshot(int index, float cdRatio)
        {
            /** 上一帧是否已就绪。初值为0（未就绪），因此开局首个快照也会触发一次 pop，
             *  正好担当「道具出现」的表现 */
            bool wasReady = propCDTarget[index] >= 1f;

            /** 条显示就绪度：1 = 可用，0 = 刚使用 */
            propCDTarget[index] = 1f - cdRatio;

            /** 就绪跳变：冷却结束，播一次 pop 提示可用，玩家无需盯条 */
            if (!wasReady && cdRatio <= 0f)
            {
                PlayPropReadyAnimation(index);
            }
        }
        /// <summary>
        /// 播放道具就绪动画。留作表现层接口，动画由美术/动画师在 Prop 图标的 Animator 上实现
        /// </summary>
        /// <param name="index">道具所在位置</param>
        public void PlayPropReadyAnimation(int index)
        {
            if (index < 0 || index >= PropContainer.EquippedPropCount) return;
            /** 显式从 0 开始，连续就绪可以重播 */
            propAnimators[index].Play(propReadyAnimState, 0, 0f);
        }
        /// <summary>
        /// 更新道具CD条。只做显示阻尼，目标值由快照给定
        /// </summary>
        public void UpdatePropCDBar()
        {
            for (int i = 0; i < PropContainer.EquippedPropCount; i++)
            {
                propBars[i].fillAmount = NonlinearDamper.FloatDamp(propBars[i].fillAmount,propCDTarget[i]);
            }
        }
        /// <summary>
        /// 道具不可用。冷却中或道具自身条件不满足时由逻辑层回执
        /// </summary>
        public void PropUnavailableHandler(PropUnavailableEvent propUnavailableEvent)
        {
            PlayPropUnavailableAnimation(propUnavailableEvent.propSlotIndex);
        }
        /// <summary>
        /// 播放道具不可用动画。留作表现层接口，动画由美术/动画师在 Prop 图标的 Animator 上实现
        /// </summary>
        /// <param name="index">道具所在位置</param>
        public void PlayPropUnavailableAnimation(int index)
        {
            if (index < 0 || index >= PropContainer.EquippedPropCount) return;
            /** 显式从 0 开始，连按可以重播 */
            propAnimators[index].Play(propUnavailableAnimState, 0, 0f);
        }
        /// <summary>
        /// 道具栏反馈：灰化。缩放、位置、旋转一律交由 Animator
        /// </summary>
        private void UpdatePropFeedback()
        {
            for (int i = 0; i < PropContainer.EquippedPropCount; i++)
            {
                /** 灰化：用已经阻尼过的条值做插值，颜色与冷却条同步 */
                Image icon = props[i].GetComponent<Image>();
                icon.color = Color.Lerp(DimColor, propOrigColor[i], propBars[i].fillAmount);
            }
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