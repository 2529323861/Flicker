using System.Collections;
using System.Collections.Generic;
using Flicker.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Flicker.UI
{    
    public class MainPanel : UIPanel
    {
        public GameObject singlePlayButton;
        public GameObject hostPlayButton;
        public GameObject joinPlayButton;
        public GameObject settingsButton;
        void Awake()
        {
            singlePlayButton.GetComponent<Button>().onClick.AddListener(SinglePlayButtonCallback);
            hostPlayButton.GetComponent<Button>().onClick.AddListener(HostPlayButtonCallback);
            joinPlayButton.GetComponent<Button>().onClick.AddListener(JoinPlayButtonCallback);
            settingsButton.GetComponent<Button>().onClick.AddListener(SettingsButtonCallback);
        }
        public void SinglePlayButtonCallback()
        {
            
        }
        public void HostPlayButtonCallback()
        {
            
        }
        public void JoinPlayButtonCallback()
        {
            
        }
        public void SettingsButtonCallback()
        {
            
        }
    }
}
