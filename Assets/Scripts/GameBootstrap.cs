using Flicker.Data;
using Flicker.Module;
using Flicker.UI;
using Flicker.View;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    /// <summary>
    /// 场景中显示版本号的文本对象名
    /// </summary>
    private const string VersionObjectName = "version";

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
        ApplyVersionLabel();
        UIManager.Instance.Open<MainPanel>();
    }

    /// <summary>
    /// 版本号取 Application.version，读的是 ProjectSettings 的 bundleVersion。
    /// 这样改版本只需改 Player Settings 一处，不用回头手改场景里的文字。
    ///
    /// 放在 Start 而非 Awake：TMP 在自身 Awake 阶段建立材质与字形，
    /// 同一帧更早写入可能被覆盖。
    /// </summary>
    private void ApplyVersionLabel()
    {
        GameObject target = GameObject.Find(VersionObjectName);
        /** 找不到就算了：版本号只是展示，不该因此中断启动 */
        if (target == null) return;
        if (!target.TryGetComponent<TMP_Text>(out var label)) return;

        label.text = "v" + Application.version;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
