using System.Collections;
using System.Collections.Generic;
using Flicker.Core;
using Flicker.Module.UIEvent;
using Flicker.UI;
using Flicker.View;
using UnityEngine;
using UnityEngine.UI;

namespace Flicker.UI
{    
    public class MainPanel : UIPanel
    {
        public GameObject hostPlayButton;
        public GameObject joinPlayButton;
        public GameObject settingsButton;
        void Awake()
        {
            hostPlayButton.GetComponent<Button>().onClick.AddListener(HostPlayButtonCallback);
            joinPlayButton.GetComponent<Button>().onClick.AddListener(JoinPlayButtonCallback);
            settingsButton.GetComponent<Button>().onClick.AddListener(SettingsButtonCallback);
        }
        /// <summary>
        /// 主菜单与大厅共用一首曲子。PlayMusic 会忽略同一首的重复请求，
        /// 因此在各处 OnOpen 里无条件调用不会打断正在播放的音乐
        /// </summary>
        public override void OnOpen(object args = null)
        {
            base.OnOpen(args);
            AudioManager.Instance.PlayMusic("BG");
        }
        public void HostPlayButtonCallback()
        {
            EventBus.Publish<HostPlayClickedEvent>(new());
        }
        public void JoinPlayButtonCallback()
        {
            UIManager.Instance.Open<JoinInputPanel>();
        }
        public void SettingsButtonCallback()
        {
            UIManager.Instance.Open<SettingsPanel>();
        }
    }
}
