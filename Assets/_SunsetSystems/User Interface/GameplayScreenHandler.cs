using Sirenix.OdinInspector;
using Sirenix.Utilities;
using SunsetSystems.Entities.Interactable;
using SunsetSystems.Tooltips;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SunsetSystems.UI
{
    public class GameplayScreenHandler : SerializedMonoBehaviour
    {
        [SerializeField]
        private NameplateManager _nameplateManager;
        [SerializeField]
        private GameObject _helpMenu;

        public void OnPointerPositionAction(InputAction.CallbackContext context)
        {

        }

        public void OnHighlightInteractablesAction(InputAction.CallbackContext context)
        {
            if (context.started)
                FindObjectsByType<InteractableEntity>(FindObjectsSortMode.None).ForEach(interactable => interactable.ForceHover = true);
            else if (context.canceled)
                FindObjectsByType<InteractableEntity>(FindObjectsSortMode.None).ForEach(interactable => interactable.ForceHover = false);
        }

        public void OnHelpAction(InputAction.CallbackContext context)
        {
            if (context.started)
                _helpMenu.SetActive(true);
            else if (context.canceled)
                _helpMenu.SetActive(false);
        }
    }
}
