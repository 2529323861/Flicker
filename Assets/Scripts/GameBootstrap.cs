using Flicker.Data;
using Flicker.Module;
using Flicker.UI;
using Flicker.View;
using Unity.VisualScripting;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {   
        SettingsManager.Instance.Init();
        //注册屏幕振动服务
        VibrationManager.Instance.Init();
        //注册音频服务
        AudioManager.Instance.Init();
        PropFactory.Instance.Init();
    }
    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.Open<MainPanel>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
