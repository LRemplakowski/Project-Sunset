using SunsetSystems.Combat;
using SunsetSystems.Inventory;
using UltEvents;

namespace SunsetSystems.Equipment
{
    public interface IWeaponManager
    {
        UltEvent<ICombatant> OnWeaponChanged { get; set; }
        UltEvent<ICombatant, WeaponAmmoData> OnAmmoChanged { get; set; }

        ITargetable WeaponTarget { get; set; }

        void SetSelectedWeapon(SelectedWeapon weapon);
        IWeapon GetSelectedWeapon();
        IWeapon GetPrimaryWeapon();
        IWeapon GetSecondaryWeapon();
        WeaponAmmoData GetSelectedWeaponAmmoData();

        bool HasEnoughAmmoInSelectedWeapon(int ammo);
        bool UseAmmoFromSelectedWeapon(int count);
        void ReloadSelectedWeapon();
    }

    public enum SelectedWeapon
    {
        None = 0, Primary = 1, Secondary = 2
    }
}
