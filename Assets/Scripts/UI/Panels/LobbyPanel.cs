using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using Flicker.Core;
using Flicker.Module.UIEvent;
using Flicker.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Flicker.UI
{    
    public class LobbyPanel : UIPanel
    {
        public Button readyButton;
        public Button startButton;
        public Button quitButton;
        void Awake()
        {
            startButton.onClick.AddListener(StartButtonCallback);
            quitButton.onClick.AddListener(QuitButtonCallback);
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
    }
}
