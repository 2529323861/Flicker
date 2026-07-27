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
        public GameObject readyButton;
        public GameObject startButton;
        public GameObject quitButton;
        void Awake()
        {
            startButton.GetComponent<Button>().onClick.AddListener(StartButtonCallback);
        }
        //————————————————————————————————Handler——————————————————————————————————————————
        public void StartButtonCallback()
        {
            EventBus.Publish<LobbyPanelStartEvent>(new());
        }
    }
}
