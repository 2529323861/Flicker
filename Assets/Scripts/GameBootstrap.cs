using Flicker.Core.Service;
using Flicker.UI;
using Flicker.View;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {   
        //注册屏幕振动服务
        ServiceLocator.RegisterService<VibrationMgrInterface>(VibrationMgr.Instance);
    }
    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.Open<PlayerHUD>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
