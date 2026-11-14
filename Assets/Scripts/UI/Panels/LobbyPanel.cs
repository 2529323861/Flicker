using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Flicker.Core;
using Flicker.Data;
using Flicker.Module;
using Flicker.Module.UIEvent;
using Flicker.UI;
using Mirror;
using Newtonsoft.Json;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Flicker.UI
{    
    public class LobbyPanel : UIPanel
    {
        public Button readyButton;
        public TMP_Text readyText;
        public Button startButton;
        public Button quitButton;
        public Button botButton;
        public TMP_Text botButtonText;
        public Image self;
        public Image other;
        public Sprite prepared;
        public Sprite unprepared;
        public TMP_Text selfName;
        public TMP_Text otherName;

        [Header("聊天")]
        [Tooltip("发送按钮，对应 Chat/MessageInput/InputField (TMP)/Submit")]
        public Button chatSubmitButton;
        [Tooltip("消息输入框，对应 Chat/MessageInput/InputField (TMP)")]
        public TMP_InputField chatInputField;
        [Tooltip("消息容器，对应 Chat/Scroll View/Viewport/Content。挂 VerticalLayoutGroup + ContentSizeFitter")]
        public RectTransform chatContent;
        [Tooltip("聊天滚动视图，对应 Chat/Scroll View。新增消息后需要把它拉到底")]
        public ScrollRect chatScroll;
        [Tooltip("消息预制体，对应 Resources/UI/Message.prefab")]
        public GameObject messagePrefab;

        private bool opened = false;
        /// <summary>
        /// 机器人存在状态，仅用于决定本次点击发出添加还是移除
        /// </summary>
        private bool hasBot = false;
        void Awake()
        {
            quitButton.onClick.AddListener(QuitButtonCallback);
            readyButton.onClick.AddListener(ReadyButtonCallback);
            startButton.onClick.AddListener(StartButtonCallback);
            botButton.onClick.AddListener(BotButtonCallback);

            EventBus.Subscribe<LobbyPanelReadySnapshotEvent>(LobbyPanelReadySnapshotHandler);
            EventBus.Subscribe<LobbyPanelPlayerNameUpdateEvent>(LobbyPanelPlayerNameUpdateHandler);
            EventBus.Subscribe<LobbyPanelBotStateEvent>(LobbyPanelBotStateHandler);
            EventBus.Subscribe<UpdatePropDescriptionEvent>(UpdatePropDescriptionHandler);
            EventBus.Subscribe<DeckSlotClickedEvent>(DeckSlotClickedHandler);
            EventBus.Subscribe<ChatMessageEvent>(ChatMessageHandler);

            chatSubmitButton.onClick.AddListener(ChatSubmitCallback);
            /** 回车也能发送，聊天框的常规习惯 */
            chatInputField.onSubmit.AddListener(_ => ChatSubmitCallback());
        }
        public override void OnOpen(object args = null)
        {
            base.OnOpen(args);
            /** 与主菜单共用同一首，重复请求会被忽略 */
            Flicker.View.AudioManager.Instance.PlayMusic("BG");
            startButton.interactable = NetworkServer.active;
            readyMask.SetActive(false);
            EventBus.Publish<LobbyPanelReadySnapshotRequest>(new());

            /** 聊天不缓存，每次回到大厅都从空开始。
             *  必须放在 opened 判断之外，否则只有第一次进大厅会清 */
            ClearChat();

            if (!opened)
            {
                EventBus.Subscribe<PropPoolEvent>(PropPoolHandler);
                EventBus.Publish<PropPoolRequestEvent>(new());

                InitDecks();
                sectionChosen.onClick.AddListener(sectionChosenButtonCallBack);
                sectionChosen.interactable = false;

                opened = true;
            }

            /** 补扫一次：卡组槽位与道具一览都是上面才生成的，
             *  基类在 OnOpen 开头扫时它们还不存在 */
            AttachButtonSfx();
        }
        //————————————————————————————————Handler——————————————————————————————————————————
        public void StartButtonCallback()
        {
            EventBus.Publish<LobbyPanelStartEvent>(new());
        }
        public void QuitButtonCallback()
        {
            EventBus.Publish<LobbyPanelQuitEvent>(new());
        }
        public void ReadyButtonCallback()
        {
            EventBus.Publish<LobbyPanelReadyEvent>(new(){deck=deckCache});
        }
        public void LobbyPanelBotStateHandler(LobbyPanelBotStateEvent lobbyPanelBotStateEvent)
        {
            /** 机器人开关只能由主机操作 */
            botButton.interactable = NetworkServer.active;
            botButtonText.text = lobbyPanelBotStateEvent.hasBot ? "移除机器人" : "添加机器人";
            hasBot = lobbyPanelBotStateEvent.hasBot;
        }
        public void BotButtonCallback()
        {
            if (hasBot)
            {
                EventBus.Publish<LobbyPanelRemoveBotEvent>(new());
            }
            else
            {
                EventBus.Publish<LobbyPanelAddBotEvent>(new());
            }
        }
        public void LobbyPanelReadySnapshotHandler(LobbyPanelReadySnapshotEvent lobbyPanelReadySnapshotEvent)
        {
            if (NetworkServer.active)
            {
                self.sprite = lobbyPanelReadySnapshotEvent.host ? prepared : unprepared;
                other.sprite = lobbyPanelReadySnapshotEvent.client ? prepared : unprepared;
                readyMask.SetActive(lobbyPanelReadySnapshotEvent.host);
                SetReadyText(lobbyPanelReadySnapshotEvent.host);
            }
            else
            {
                self.sprite = lobbyPanelReadySnapshotEvent.client ? prepared : unprepared;
                other.sprite = lobbyPanelReadySnapshotEvent.host ? prepared : unprepared;
                readyMask.SetActive(lobbyPanelReadySnapshotEvent.client);
                SetReadyText(lobbyPanelReadySnapshotEvent.client);
            }
        }
        /// <summary>
        /// 准备按钮的文字随自身准备状态切换
        /// </summary>
        /// <param name="isReady">自己是否已准备</param>
        private void SetReadyText(bool isReady)
        {
            readyText.text = isReady ? "取消准备" : "准备";
        }
        public void LobbyPanelPlayerNameUpdateHandler(LobbyPanelPlayerNameUpdateEvent lobbyPanelPlayerNameUpdateEvent)
        {
            if (NetworkServer.active)
            {
                selfName.text = lobbyPanelPlayerNameUpdateEvent.host;
                otherName.text = lobbyPanelPlayerNameUpdateEvent.client;
            }
            else
            {
                selfName.text = lobbyPanelPlayerNameUpdateEvent.client;
                otherName.text = lobbyPanelPlayerNameUpdateEvent.host;
            }
        }
        //——————————————————————————————————卡组相关——————————————————————————————————————————
        //——————————————————————————————————道具一览——————————————————————————————————————————
        /// <summary>
        /// 滚动框内容区节点
        /// </summary>
        public RectTransform scrollViewContent;
        public GameObject sectionSlot;
        private Dictionary<string,BasePropStrategyMetaData> basePropStrategyMetaDatas = new();
        public void PropPoolHandler(PropPoolEvent propPoolEvent)
        {
            foreach (var i in propPoolEvent.metaDatas)
            {
                AddSectionSlot(i);
                basePropStrategyMetaDatas.Add(i.factoryId,i);
            }
            scrollViewContent.sizeDelta = new(){x=scrollViewContent.sizeDelta.x,y=15+(125+15)*Mathf.CeilToInt(propPoolEvent.metaDatas.Count / 6f)};
        }
        private void AddSectionSlot(BasePropStrategyMetaData basePropStrategyMetaData)
        {
            GameObject s = Instantiate(sectionSlot,scrollViewContent,false);
            s.GetComponent<SectionSlot>().SetProp(basePropStrategyMetaData);
        }
        //——————————————————————————————————道具详情——————————————————————————————————————————
        public TMP_Text sectionName;
        public TMP_Text sectionDescription;
        public RectTransform descriptionScrollViewContent;
        public Image sectionIcon;
        public Button sectionChosen;
        private BasePropStrategyMetaData nowChosenProp;
        public void UpdatePropDescriptionHandler(UpdatePropDescriptionEvent updatePropDescriptionEvent)
        {
            nowChosenProp = updatePropDescriptionEvent.metaData;

            sectionName.text = updatePropDescriptionEvent.metaData.name;
            sectionDescription.text = updatePropDescriptionEvent.metaData.description;
            sectionIcon.sprite = Resources.Load<Sprite>("Images/UI/PropIcon/"+updatePropDescriptionEvent.metaData.path);

            float nameHeight = sectionName.GetPreferredValues(sectionName.text,sectionName.GetComponent<RectTransform>().sizeDelta.x,0).y;
            float descriptionHeight = sectionDescription.GetPreferredValues(sectionDescription.text,sectionDescription.GetComponent<RectTransform>().sizeDelta.x,0).y;

            descriptionScrollViewContent.sizeDelta = new(){x=descriptionScrollViewContent.sizeDelta.x,y=nameHeight+descriptionHeight};
            foreach (var i in deckSlots)
            {
                if (i.FactoryId == nowChosenProp.factoryId)
                {
                    //有重复的，禁用选择
                    sectionChosen.interactable = false;
                    return;
                }
            }
            //没有重复的，允许选择
            sectionChosen.interactable = true;
        }
        public void sectionChosenButtonCallBack()
        {
            deckCache[chosenDeckSlot] = nowChosenProp.factoryId;
            deckSlots[chosenDeckSlot].SetProp(nowChosenProp);
            SettingsManager.Instance.SetSetting("pvpDeck",JsonConvert.SerializeObject(deckCache));
            sectionChosen.interactable = false;
        }
        //————————————————————————————————————卡组编辑————————————————————————————————————————————
        public RectTransform deckContent;
        public GameObject deckSlot;
        private List<DeckSlot> deckSlots = new();
        private int chosenDeckSlot = 0;
        private List<string> deckCache = new();
        private void InitDecks()
        {
            string deckString = SettingsManager.Instance.GetSetting<string>("pvpDeck");
            try {
                deckCache = JsonConvert.DeserializeObject<List<string>>(deckString);
            }catch
            {
                SettingsManager.Instance.ResetSetting("pvpDeck");
                deckString = SettingsManager.Instance.GetSetting<string>("pvpDeck");
                deckCache = JsonConvert.DeserializeObject<List<string>>(deckString);
            }
            if (deckCache.Count!=PropContainer.EquippedPropCount){
                SettingsManager.Instance.ResetSetting("pvpDeck");
                deckString = SettingsManager.Instance.GetSetting<string>("pvpDeck");
                deckCache = JsonConvert.DeserializeObject<List<string>>(deckString);
            }
            BasePropStrategyMetaData basePropStrategyMetaData;
            for (int i = 0; i < PropContainer.EquippedPropCount; i++){
                if (!basePropStrategyMetaDatas.TryGetValue(deckCache[i],out basePropStrategyMetaData)){
                    SettingsManager.Instance.ResetSetting("pvpDeck");
                    deckString = SettingsManager.Instance.GetSetting<string>("pvpDeck");
                    deckCache = JsonConvert.DeserializeObject<List<string>>(deckString);
                    break;
                }
            }

            for (int i = 0; i < PropContainer.EquippedPropCount; i++)
            {
                GameObject s = Instantiate(deckSlot,deckContent,false);
                deckSlots.Add(s.GetComponent<DeckSlot>());
                deckSlots[i].Bind(i);
                deckSlots[i].SetSelected(false);
                if (basePropStrategyMetaDatas.TryGetValue(deckCache[i],out basePropStrategyMetaData))
                {                    
                    deckSlots[i].SetProp(basePropStrategyMetaData);
                }
            }
            deckSlots[chosenDeckSlot].SetSelected(true);
        }
        public void DeckSlotClickedHandler(DeckSlotClickedEvent deckSlotClickedEvent)
        {
            deckSlots[chosenDeckSlot].SetSelected(false);
            chosenDeckSlot = deckSlotClickedEvent.index;
            deckSlots[chosenDeckSlot].SetSelected(true);
        }
        //————————————————————————————————————聊天——————————————————————————————————————————
        /// <summary>
        /// 点击发送。只发正文，发送者名由服务端填。
        /// 空消息与纯空白在此拦掉，避免产生空白气泡
        /// </summary>
        public void ChatSubmitCallback()
        {
            string text = chatInputField.text;
            if (string.IsNullOrWhiteSpace(text)) return;

            EventBus.Publish<SendChatEvent>(new() { content = text });
            chatInputField.text = string.Empty;
        }
        /// <summary>
        /// 收到服务端广播的聊天，落到界面上。
        /// 所有端（含发送者本人）走同一条路径，不做本地回显
        /// </summary>
        public void ChatMessageHandler(ChatMessageEvent chatMessageEvent)
        {
            GameObject go = Instantiate(messagePrefab, chatContent, false);
            var message = go.GetComponent<Message>();
            message.SetSender(chatMessageEvent.sender);
            message.SetContent(chatMessageEvent.content);

            ScrollChatToBottom();
        }
        /// <summary>
        /// 滚到最新一条。
        /// 刚创建的节点在本帧尚未参与布局，content 高度还是旧值，
        /// 此刻直接设 normalizedPosition 会按旧高度算而滚不到底。
        /// 因此先强制重建一次，再设位置
        /// </summary>
        private void ScrollChatToBottom()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(chatContent);
            chatScroll.verticalNormalizedPosition = 0f;
        }
        /// <summary>
        /// 清空聊天区。回到大厅时调用，符合"不缓存历史消息"的设计
        /// </summary>
        private void ClearChat()
        {
            if (chatContent == null) return;

            for (int i = chatContent.childCount - 1; i >= 0; i--)
            {
                Destroy(chatContent.GetChild(i).gameObject);
            }
            if (chatInputField != null) chatInputField.text = string.Empty;
        }
        //————————————————————————————————————准备遮罩——————————————————————————————————————————
        public GameObject readyMask;
    }
}
