using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SunsetSystems.UI.Utils
{
    public class DefaultConfirmationPopup : SerializedMonoBehaviour, IConfirmationPopup
    {
        [SerializeField]
        private TextMeshProUGUI _title;
        [SerializeField]
        private TextMeshProUGUI _popupText;

        private Action _confirmationDelegate;

        public virtual void Show(ConfirmationViewData viewData, Action onConfirmDelegate)
        {
            _title.text = viewData.Title;
            _popupText.text = viewData.Text;
            _confirmationDelegate = onConfirmDelegate;
            gameObject.SetActive(true);
            SunsetInputHandler.Instance.OverrideInput(this, SunsetInputHandler.UI_MAP, SunsetInputHandler.SHORTCUTS_POPUP_MAP);
            SunsetInputHandler.OnPopupCancel += OnInputCancel;
        }

        private void OnInputCancel(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnCancel();
        }

        public virtual void OnConfirm()
        {
            _confirmationDelegate?.Invoke();
            Hide();
        }

        public virtual void OnCancel()
        {
            Hide();
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            Cleanup();
        }

        private void Cleanup()
        {
            _confirmationDelegate = null;
            SunsetInputHandler.Instance.ClearInputOverride(this);
            SunsetInputHandler.OnPopupCancel -= OnInputCancel;
        }
    }
}
