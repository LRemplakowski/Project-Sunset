namespace SunsetSystems.Abilities
{
    public interface IAmmoAbility 
    {
        bool UsesAmmo { get; }
        int GetAmmoPerUse();
        int GetUsesPerExecution();
    }
}
