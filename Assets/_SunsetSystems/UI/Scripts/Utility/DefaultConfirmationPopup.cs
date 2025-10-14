using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

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
        }

        public virtual void OnConfirm()
        {
            Hide();
            _confirmationDelegate?.Invoke();
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
        }
    }
}
