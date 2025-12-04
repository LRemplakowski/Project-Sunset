using System;

namespace SunsetSystems.UI.Utils
{
    public interface IConfirmationPopup
    {
        void Hide();
        void Show() => Show(null);
        void Show(Action onConfirmDelegate);
        void Show(ConfirmationViewData viewData, Action onConfirmDelegate);
    }

    public interface IConfirmationPopup<T>
    {
        void Hide();
        void Show(Action<T> onConfirmDelegate);
        void Show(ConfirmationViewData viewData, Action<T> onConfirmDelegate);
    }
}
