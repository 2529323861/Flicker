using Flicker.Module;
using UnityEngine;
using UnityEngine.UI;

namespace Flicker.UI
{
    public class PlayerHUD: UIPanel
    {
        public GameObject mask;
        public float maxMaskHeight;

        Mask maskComponent;
        GameObject player = null;
        GameCharacterController gcc;
        void Awake()
        {
            maskComponent =  mask.GetComponent<Mask>();
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
            }
            maskComponent.rectTransform.sizeDelta = new Vector2(maskComponent.rectTransform.sizeDelta.x,maxMaskHeight*(1-gcc.nowJumpingCDMV.value/gcc.maxJumpingCDMV.FinalValue));
        }
    }
}