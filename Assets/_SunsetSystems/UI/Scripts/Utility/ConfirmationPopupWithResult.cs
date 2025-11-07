using System;

namespace SunsetSystems.UI.Utils
{
    public abstract class ConfirmationPopupWithResult<T> : DefaultConfirmationPopup, IConfirmationPopup<T>
    {
        private Action<T> _confirmationDelegate;

        public void Show(ConfirmationViewData viewData, Action<T> onConfirmDelegate)
        {
            base.Show(viewData, null);
            _confirmationDelegate = onConfirmDelegate;
        }

        public override void OnConfirm()
        {
            if (TryGetReturnValue(out T result))
            {
                _confirmationDelegate?.Invoke(result);
                base.OnConfirm();
            }
        }

        public override void Hide()
        {
            base.Hide();
            Cleanup();
        }

        private void Cleanup()
        {
            _confirmationDelegate = null;
        }

        protected abstract bool TryGetReturnValue(out T result);

        public void Show(Action<T> onConfirmDelegate)
        {
            base.Show(null);
            _confirmationDelegate = onConfirmDelegate;
        }
    }
}
