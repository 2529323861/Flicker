using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using Flicker.Core;
using Flicker.Module.UIEvent;
using Flicker.UI;
using Mirror;
using TMPro;
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
        void Awake()
        {
            quitButton.onClick.AddListener(QuitButtonCallback);
            readyButton.onClick.AddListener(ReadyButtonCallback);
            startButton.onClick.AddListener(StartButtonCallback);

            EventBus.Subscribe<LobbyPanelReadySnapshotEvent>(LobbyPanelReadySnapshotHandler);
            EventBus.Subscribe<LobbyPanelPlayerNameUpdateEvent>(LobbyPanelPlayerNameUpdateHandler);
        }
        public override void OnOpen(object args = null)
        {
            base.OnOpen(args);
            startButton.interactable = NetworkServer.active;
            EventBus.Publish<LobbyPanelReadySnapshotRequest>(new());
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
    }
}
