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
    }
}