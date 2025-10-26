using TMPro;
using UnityEngine;

namespace SunsetSystems.UI.Utils
{
    public class StringConfirmationPopup : ConfirmationPopupWithResult<string>
    {
        [SerializeField]
        private TMP_InputField _inputField;
        [SerializeField]
        private bool _requireNonEmptyInput = true;

        private string _defaultText;

        private void Awake()
        {
            _defaultText = _inputField.text;
        }

        private void OnEnable()
        {
            _inputField.Select();
        }

        private void OnDisable()
        {
            _inputField.text = _defaultText;
        }

        protected override bool TryGetReturnValue(out string result)
        {
            result = string.Empty;
            if (IsInputValid())
            {
                result = _inputField.text;
                return true;
            }
            return false;
        }

        private bool IsInputValid()
        {
            return _requireNonEmptyInput && !string.IsNullOrWhiteSpace(_inputField.text);
        }
    }
}
