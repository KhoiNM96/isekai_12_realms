using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Isekai12Realms.Adventure
{
    public class AdventureHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public Action Pressed;
        public Action Released;
        public Action Clicked;

        public void OnPointerDown(PointerEventData eventData)
        {
            Pressed?.Invoke();
            Clicked?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Released?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Released?.Invoke();
        }
    }
}
