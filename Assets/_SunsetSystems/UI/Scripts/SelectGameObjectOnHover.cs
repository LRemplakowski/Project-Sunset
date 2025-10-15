using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SunsetSystems.UI
{
    public class SelectGameObjectOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData)
        {
            Selectable selectable = default;
            if (eventData.hovered.Any(go => go.TryGetComponent(out selectable)))
                selectable.Select();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Selectable selectable = default;
            if (eventData.hovered.Any(go => go.TryGetComponent(out selectable)))
                EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
