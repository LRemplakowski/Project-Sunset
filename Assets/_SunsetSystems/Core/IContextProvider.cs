namespace SunsetSystems.Entities
{
    public interface IContextProvider<out T>
    {
        T GetContext();
    }
}
