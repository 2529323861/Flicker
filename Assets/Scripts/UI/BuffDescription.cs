
using Flicker.Core;
using Flicker.Module.UIEvent;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Flicker.UI
{
    public class BuffDescription: MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
    {
        /// <summary>
        /// buff名称
        /// </summary>
        public string buffName;
        /// <summary>
        /// buff描述
        /// </summary>
        public string description;
        public bool isHovering=false;
        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovering = true;
            EventBus.Publish<ShowBuffDescriptionEvent>(new()
            {
                name = buffName,
                description = description
            });
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering=false;
            EventBus.Publish<HideBuffDescriptionEvent>(new());
        }
        public void OnDestroy()
        {
            if (isHovering)
            {
                EventBus.Publish<HideBuffDescriptionEvent>(new());
            }
        }
    }
}