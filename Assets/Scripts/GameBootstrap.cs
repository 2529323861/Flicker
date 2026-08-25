using Flicker.UI;
using Flicker.View;
using Unity.VisualScripting;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    void Awake()
    {   
        //注册屏幕振动服务
        VibrationManager.Instance.init();
    }
    // Start is called before the first frame update
    void Start()
    {
        UIManager.Instance.Open<MainPanel>();
        UIManager.Instance.Open<TipPanel>(new TipMessage(){title="注意？",content="下层菜单不会被触发~下层菜单不会被触发~下层菜单不会被触发~"});
        UIManager.Instance.Open<TipPanel>(new TipMessage(){title="注意！",content="队列里还有一条消息~队列里还有一条消息~队列里还有一条消息~队列里还有一条消息~队列里还有一条消息~队列里还有一条消息~队列里还有一条消息~队列里还有一条消息~队列里还有一条消息~队列里还有一条消息~"});
        UIManager.Instance.Open<CountDown>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
