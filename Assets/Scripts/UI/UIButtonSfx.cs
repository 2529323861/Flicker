using Flicker.View;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Flicker.UI
{
    /// <summary>
    /// 按钮点击音。挂在任意 Button 上，按下即响。
    ///
    /// 用 OnPointerDown 而不是 onClick：
    ///   - 按下瞬间就发声，手感是"立刻响应"，点击音的价值全在这一下；
    ///   - onClick 要等抬手，且会排在按钮自身逻辑之后，按下与被遮挡时都不响。
    ///
    /// 不响的两种情况：
    ///   - Button 处于非可交互（置灰），此时按下去不会发生任何事；
    ///   - 非左键。
    ///
    /// 由 UIPanel.OnOpen 自动批量挂载，无需手工拖拽。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButtonSfx : MonoBehaviour, IPointerDownHandler
    {
        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            /** 置灰按钮按下去不该有回应，否则会骗玩家"我点到了" */
            if (!button.IsInteractable()) return;
            if (AudioManager.Instance == null) return;

            AudioManager.Instance.PlayButtonClick();
        }
    }
}
