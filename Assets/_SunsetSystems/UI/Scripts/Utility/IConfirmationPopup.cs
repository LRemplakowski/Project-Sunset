using System;

namespace SunsetSystems.UI.Utils
{
    public interface IConfirmationPopup
    {
        void Hide();
        void Show(ConfirmationViewData viewData, Action onConfirmDelegate);
    }

    public interface IConfirmationPopup<T>
    {
        void Hide();
        void Show(ConfirmationViewData viewData, Action<T> onConfirmDelegate);
    }
}
