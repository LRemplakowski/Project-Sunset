using TMPro;
using UnityEngine;

namespace SunsetSystems.UI.Utils
{
    public class StringConfirmationPopup : ConfirmationPopupWithResult<string>
    {
        [SerializeField]
        private TMP_InputField _inputField;

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
            return !string.IsNullOrWhiteSpace(_inputField.text);
        }
    }
}
