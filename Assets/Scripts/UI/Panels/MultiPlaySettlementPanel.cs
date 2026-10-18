using TMPro;
using UnityEngine.UI;

namespace Flicker.UI
{
    public class MultiPlaySettlementPanel: UIPanel
    {
        public Button quit;
        public TMP_Text text;
        void Awake()
        {
            quit.onClick.AddListener(() =>
            {
                UIManager.Instance.Close<MultiPlaySettlementPanel>();
                UIManager.Instance.CloseAll(UILayer.HUD);
                UIManager.Instance.Open<LobbyPanel>();
            });
        }
        /// <summary>
        /// 打开时接收本局胜负
        /// </summary>
        /// <param name="args">本局是否获胜</param>
        public override void OnOpen(object args = null)
        {
            base.OnOpen(args);
            bool win = (bool)args;
            text.text = win ? "游戏胜利！" : "游戏失败！";
        }
    }
}