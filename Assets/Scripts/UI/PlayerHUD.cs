using System.Collections.Generic;
using Flicker.Core;
using Flicker.Module;
using Flicker.Module.CharacterEvent;
using UnityEngine;
using UnityEngine.UI;

namespace Flicker.UI
{
    /// <summary>
    /// 玩家HUD，游戏时始终显示
    /// </summary>
    public class PlayerHUD: UIPanel
    {
        public GameObject jumpCDProgress;
        public float maxMaskHeight;

        Image jumpCDImage;
        GameObject player = null;
        GameCharacterController gcc;
        void Awake()
        {
            jumpCDImage =  jumpCDProgress.GetComponent<Image>();
        }
        void Start()
        {
            Layer = UILayer.HUD;
        }
        void Update()
        {
            if (player == null)
            {
                player = GameManager.Instance.GetPlayerRef();
                if (player == null) return;
                gcc = player.GetComponent<GameCharacterController>();
                miniEventBus = player.GetComponent<MiniEventBus>();
            }
            SetJumpCDBar(1-gcc.nowJumpingCDMV.value/gcc.maxJumpingCDMV.FinalValue);
        }
        /// <summary>
        /// 设置跳跃CD条
        /// </summary>
        /// <param name="per"></param>
        public void SetJumpCDBar(float per)
        {
            var nowAmount = NonlinearDamper.Vector3Damp(new(jumpCDImage.fillAmount,0,0),new(per,0,0)).x;
            jumpCDImage.fillAmount = nowAmount;
        }
            }
        }
    }
}