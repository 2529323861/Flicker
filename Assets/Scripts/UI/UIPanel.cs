using UnityEngine;
using UnityEngine.UI;

namespace Flicker.UI
{
    /// <summary>
    /// UI 面板基类。
    /// 给所有的独立 UI 面板制定统一的生命周期。
    /// </summary>
    public abstract class UIPanel : MonoBehaviour
    {
        [Header("Panel Settings")]
        [Tooltip("面板所属层级")]
        public UILayer Layer = UILayer.Normal;

        [Tooltip("是否全屏面板。若为全屏，打开时将暂停/隐藏下层堆栈对象")]
        public bool IsFullScreen = false;

        protected object _openArgs;
        
        /// <summary>
        /// 是否仍在UIManager堆栈内管理。
        /// </summary>
        public bool IsOpened { get; internal set; }

        /// <summary>
        /// 面板被打开时调用（只调用一次）。
        /// 可以在此处执行动画（如淡入、缩放）。
        /// </summary>
        public virtual void OnOpen(object args = null)
        {
            _openArgs = args;
            gameObject.SetActive(true);
            /** 在激活之后扫描：未激活的子物体默认查不到 */
            AttachButtonSfx();
        }

        /// <summary>
        /// 给面板内所有按钮挂上点击音。
        /// 放在基类而不是逐个按钮手工拖拽，是因为按钮会随面板增删，
        /// 手工挂载必然漏。每次打开都扫一遍，已挂的跳过，代价是十几次 GetComponent。
        ///
        /// 子类若在 OnOpen 里动态生成按钮（如大厅的卡组槽位），
        /// 需在生成完后再调一次——基类的扫描发生在动态生成之前。
        /// </summary>
        protected void AttachButtonSfx()
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (Button button in buttons)
            {
                if (button.GetComponent<UIButtonSfx>() != null) continue;
                button.gameObject.AddComponent<UIButtonSfx>();
            }
        }

        /// <summary>
        /// 面板被关闭时调用。
        /// </summary>
        public virtual void OnClose()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 被上层全屏面板遮挡时调用。
        /// 可以用来暂停动画、屏蔽输入，甚至隐藏自身对象节省开销。
        /// </summary>
        public virtual void OnPause()
        {
            // 默认采取隐藏策略（根据项目美术管线需求决定）
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 上层全屏面板关闭，重新显示此面板时调用。
        /// </summary>
        public virtual void OnResume()
        {
            gameObject.SetActive(true);
        }

        /// <summary>
        /// 快捷关闭自身（调用 UIManager）。
        /// </summary>
        protected void CloseSelf()
        {
            UIManager.Instance.Close(this.GetType());
        }
    }
}
