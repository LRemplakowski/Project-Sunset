namespace SunsetSystems.Persistence.UI
{
    public interface ISaveView
    {
        void Initialize(SaveLoadScreenManager saveLoadScreenManager);
        void Show(SaveMetaData saveMetaData);
    }
}
