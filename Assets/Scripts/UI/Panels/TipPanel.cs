using System.Collections;
using System.Collections.Generic;
using Flicker.Core;
using Flicker.Module.UIEvent;
using Flicker.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Flicker.UI
{   public struct TipMessage{
        public string title;
        public string content;
    }
    public class TipPanel : UIPanel
    {
        public Button confirm;
        public GameObject root;
        public GameObject tray;
        public TMP_Text title;
        public TMP_Text content;
        private List<TipMessage> tipMessages=new();
        private bool isOpen=false;

        void Awake()
        {
            confirm.onClick.AddListener(()=>SelfClose());
        }
        public override void OnOpen(object args = null)
        {
            base.OnOpen(args);

            tipMessages.Add((TipMessage)args);
            if (!isOpen){
                SelfOpen();
            }
        }
        private void SelfOpen(){
            isOpen=true;

            title.text = tipMessages[0].title;
            content.text = tipMessages[0].content;

            float lineHeight = content.GetPreferredValues(559.1f,50f).y;
            var rt = tray.GetComponent<RectTransform>();
            rt.sizeDelta = new(rt.sizeDelta.x,lineHeight+100);

            //播放打开动画
            GetComponent<Animator>().Play("UIShow",0,0f);
        }
        private void SelfClose(){
            tipMessages.RemoveAt(0);
            if (tipMessages.Count!=0){
                SelfOpen();
            }else{
                isOpen=false;
                UIManager.Instance.Close<TipPanel>();
            }
        }
    }
}
