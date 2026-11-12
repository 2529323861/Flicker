using Flicker.Data;
using Flicker.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Flicker.UI
{
    public class SettingsPanel: UIPanel
    {
        public Slider musicVolume;
        public Slider sfxVolume;
        public TMP_InputField nameInput;
        public Button confirm;

        void Awake()
        {
            musicVolume.onValueChanged.AddListener(
                (value) =>
                {
                    AudioManager.Instance.musicVolume = value;
                }
            );
            sfxVolume.onValueChanged.AddListener(
                (value) =>
                {
                    AudioManager.Instance.sfxVolume = value;
                }
            );
            confirm.onClick.AddListener(Confirm);
        }
        public override void OnOpen(object args = null)
        {
            base.OnOpen(args);
            GetComponent<Animator>().Play("UIShow",0,0f);

            musicVolume.value = SettingsManager.Instance.GetSetting<float>("musicVolume");
            sfxVolume.value = SettingsManager.Instance.GetSetting<float>("sfxVolume");

            nameInput.text = SettingsManager.Instance.GetSetting<string>("playerName");
        }
        public void Confirm()
        {
            SettingsManager.Instance.SetSetting<float>("musicVolume",musicVolume.value);
            SettingsManager.Instance.SetSetting<float>("sfxVolume",sfxVolume.value);

            SettingsManager.Instance.SetSetting<string>("playerName",nameInput.text);

            UIManager.Instance.Close<SettingsPanel>();
        }
    }
}