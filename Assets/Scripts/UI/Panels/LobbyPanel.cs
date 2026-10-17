using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using Flicker.Core;
using Flicker.Module;
using Flicker.Module.UIEvent;
using Flicker.UI;
using Mirror;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Flicker.UI
{    
    public class LobbyPanel : UIPanel
    {
        public Button readyButton;
        public Button startButton;
        public Button quitButton;
        public Image self;
        public Image other;
        public Sprite prepared;
        public Sprite unprepared;
        public TMP_Text selfName;
        public TMP_Text otherName;

        private bool opened = false;
        void Awake()
        {
            quitButton.onClick.AddListener(QuitButtonCallback);
            readyButton.onClick.AddListener(ReadyButtonCallback);
            startButton.onClick.AddListener(StartButtonCallback);

            EventBus.Subscribe<LobbyPanelReadySnapshotEvent>(LobbyPanelReadySnapshotHandler);
            EventBus.Subscribe<LobbyPanelPlayerNameUpdateEvent>(LobbyPanelPlayerNameUpdateHandler);
            EventBus.Subscribe<UpdatePropDescriptionEvent>(UpdatePropDescriptionHandler);
        }
        public override void OnOpen(object args = null)
        {
            base.OnOpen(args);
            startButton.interactable = NetworkServer.active;
            EventBus.Publish<LobbyPanelReadySnapshotRequest>(new());

            if (!opened)
            {
                EventBus.Subscribe<PropPoolEvent>(PropPoolHandler);
                EventBus.Publish<PropPoolRequestEvent>(new());
                opened = true;
            }
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
            EventBus.Publish<LobbyPanelReadyEvent>(new());
        }
        public void LobbyPanelReadySnapshotHandler(LobbyPanelReadySnapshotEvent lobbyPanelReadySnapshotEvent)
        {
            if (NetworkServer.active)
            {
                self.sprite = lobbyPanelReadySnapshotEvent.host ? prepared : unprepared;
                other.sprite = lobbyPanelReadySnapshotEvent.client ? prepared : unprepared;
            }
            else
            {
                self.sprite = lobbyPanelReadySnapshotEvent.client ? prepared : unprepared;
                other.sprite = lobbyPanelReadySnapshotEvent.host ? prepared : unprepared;
            }
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
        public void PropPoolHandler(PropPoolEvent propPoolEvent)
        {
            foreach (var i in propPoolEvent.metaDatas)
            {
                AddSectionSlot(i);
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
        }
    }
}
