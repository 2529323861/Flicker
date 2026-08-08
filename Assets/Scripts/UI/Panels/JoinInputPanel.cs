using System.Collections;
using System.Collections.Generic;
using Flicker.Core;
using Flicker.Module.UIEvent;
using Flicker.UI;
using Mirror.Transports.Encryption;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Flicker.UI
{
    public class JoinInputPanel : UIPanel
    {
        public Button join;
        public Button quit;
        public TMP_InputField tMP_InputField;
        void Awake()
        {
            quit.onClick.AddListener(()=>UIManager.Instance.Close<JoinInputPanel>());
            join.onClick.AddListener(JoinHandler);
        }
        public override void OnOpen(object args = null)
        {
            base.OnOpen(args);
            GetComponent<Animator>().Play("UIShow",0,0f);
        }
        void JoinHandler()
        {
            EventBus.Publish<JoinUrlEvent>(new (){url=tMP_InputField.text});
        }

    }
}
