namespace SunsetSystems.UI.Utils
{
    public readonly struct ConfirmationViewData
    {
        public readonly string Title;
        public readonly string Text;

        public ConfirmationViewData(string title, string text)
        {
            Title = title;
            Text = text;
        }
    }
}
