using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using SunsetSystems.Animation;
using SunsetSystems.Combat;
using SunsetSystems.Combat.UI;
using SunsetSystems.Game;
using SunsetSystems.Inventory;
using SunsetSystems.Inventory.Data;
using UltEvents;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SunsetSystems.Equipment
{
    public class WeaponManager : SerializedMonoBehaviour, IWeaponManager
    {
        private const string FIRE_WEAPON_EVENT = "FIRE_WEAPON";

        [Title("References")]
        [SerializeField, Required]
        private IEquipmentManager equipmentManager;
        [SerializeField, Required]
        private Transform weaponParent;
        [SerializeField, Required]
        private AnimationManager animationController;
        [SerializeField, Required]
        private ICombatant owner;
        [Title("Config")]
        [SerializeField]
        private bool _ignoreAmmo;
        [SerializeField]
        private bool _showWeaponOutsideCombat;
        [Title("Debug Info")]
        [SerializeField, ReadOnly]
        private EquipmentSlotID selectedWeapon = EquipmentSlotID.PrimaryWeapon;
        [ShowInInspector, ReadOnly]
        private IWeaponInstance weaponInstance;
        [ShowInInspector, ReadOnly]
        private Dictionary<string, WeaponAmmoData> weaponsAmmoData = new();

        [field: Title("Events")]
        [field: SerializeField]
        public UltEvent<ICombatant> OnWeaponChanged { get; set; }
        [field: SerializeField]
        public UltEvent<IWeaponInstance> OnWeaponInstanceRebuilt { get; set; }
        [field: SerializeField]
        public UltEvent<ICombatant, WeaponAmmoData> OnAmmoChanged { get; set; }

        public ITargetable WeaponTarget { get; set; }

        private void OnEnable()
        {
            CombatManager.OnCombatStart += OnCombatStart;
            CombatManager.OnCombatEnd += OnCombatEnd;
            WeaponSetSelectorButton.OnWeaponSelected += OnWeaponSelected;
            animationController.OnAnimationEvent += OnAnimationEvent;
        }

        private void OnDisable()
        {
            CombatManager.OnCombatStart -= OnCombatStart;
            CombatManager.OnCombatEnd -= OnCombatEnd;
            WeaponSetSelectorButton.OnWeaponSelected -= OnWeaponSelected;
            animationController.OnAnimationEvent -= OnAnimationEvent;
        }

        private void Start()
        {
            //SetSelectedWeapon(SelectedWeapon.None);
            weaponsAmmoData ??= new();
            if (_showWeaponOutsideCombat)
                OnCombatStart(new List<ICombatant>() { owner });
        }

        private void OnWeaponSelected(SelectedWeapon weapon)
        {
            if (CombatManager.Instance.CurrentActiveActor.Equals(owner))
            {
                SetSelectedWeapon(weapon);
                OnWeaponChanged?.InvokeSafe(owner);
            }
        }

        private void OnCombatStart(IEnumerable<ICombatant> combatants)
        {
            weaponsAmmoData.Clear();
            if (combatants.Contains(owner))
            {
                IWeapon primaryWeapon = GetPrimaryWeapon();
                EnsureAmmoData(primaryWeapon);
                IWeapon secondaryWeapon = GetSecondaryWeapon();
                EnsureAmmoData(secondaryWeapon);
                _ = RebuildWeaponInstance();
            }
        }

        public void OnCombatEnd(IEnumerable<ICombatant> _)
        {
            ReleaseCurrentWeaponInstance();
        }

        [Button]
        public async void SetSelectedWeapon(SelectedWeapon weapon)
        {
            EquipmentSlotID newSelectedWeapon = weapon switch
            {
                SelectedWeapon.Primary => EquipmentSlotID.PrimaryWeapon,
                SelectedWeapon.Secondary => EquipmentSlotID.SecondaryWeapon,
                SelectedWeapon.None => EquipmentSlotID.Invalid,
                _ => EquipmentSlotID.PrimaryWeapon,
            };
            if (newSelectedWeapon != selectedWeapon)
            {
                selectedWeapon = newSelectedWeapon;
                await RebuildWeaponInstance();
            }
        }

        private async Task RebuildWeaponInstance()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying is false)
                return;
#endif
            if (weaponInstance != null)
                ReleaseCurrentWeaponInstance();
            if (GameManager.Instance.IsCurrentState(GameState.Combat) is false && _showWeaponOutsideCombat is false)
                return;
            weaponInstance = await InstantiateCurrentWeapon();
            OnWeaponInstanceRebuilt?.InvokeSafe(weaponInstance);
        }

        private async Task<IWeaponInstance> InstantiateCurrentWeapon()
        {
            if (selectedWeapon != EquipmentSlotID.Invalid && GetSelectedWeapon()?.EquippedInstanceAsset != null)
                return (await Addressables.InstantiateAsync(GetSelectedWeapon().EquippedInstanceAsset, weaponParent).Task).GetComponent<IWeaponInstance>();
            else
                return null;
        }

        private void ReleaseCurrentWeaponInstance()
        {
            if (weaponInstance != null)
                Addressables.ReleaseInstance(weaponInstance.GameObject);
            weaponInstance = null;
        }

        public IWeapon GetSelectedWeapon()
        {
            if (equipmentManager.EquipmentSlots.TryGetValue(selectedWeapon, out IEquipmentSlot slot))
                return slot.GetEquippedItem() as IWeapon;
            return null;
        }

        public IWeapon GetPrimaryWeapon()
        {
            return equipmentManager.EquipmentSlots[EquipmentSlotID.PrimaryWeapon].GetEquippedItem() as IWeapon;
        }

        public IWeapon GetSecondaryWeapon()
        {
            return equipmentManager.EquipmentSlots[EquipmentSlotID.SecondaryWeapon].GetEquippedItem() as IWeapon;
        }

        public WeaponAmmoData GetSelectedWeaponAmmoData()
        {
            if (weaponsAmmoData.TryGetValue(GetSelectedWeapon()?.DatabaseID, out var ammoData))
                return ammoData;
            return new() { Weapon = GetSelectedWeapon() };
        }

        public bool HasEnoughAmmoInSelectedWeapon(int ammo)
        {
            IWeapon selectedWeapon = GetSelectedWeapon();
            return weaponsAmmoData.TryGetValue(selectedWeapon.DatabaseID, out var ammoData) && ammoData.CurrentAmmo >= ammo;
        }

        public bool UseAmmoFromSelectedWeapon(int count)
        {
            IWeapon selectedWeapon = GetSelectedWeapon();
            if (selectedWeapon == null || selectedWeapon.WeaponType == AbilityRange.Melee || _ignoreAmmo)
                return true;
            if (weaponsAmmoData.TryGetValue(selectedWeapon.DatabaseID, out WeaponAmmoData ammoData))
            {
                if (ammoData.CurrentAmmo < count)
                    return false;
                ammoData.CurrentAmmo -= count;
                weaponsAmmoData[selectedWeapon.DatabaseID] = ammoData;
                OnAmmoChanged?.InvokeSafe(owner, ammoData);
                return true;
            }
            return false;
        }

        public void ReloadSelectedWeapon()
        {
            IWeapon selectedWeaponData = GetSelectedWeapon();
            if (selectedWeaponData == null)
                return;
            if (weaponsAmmoData.TryGetValue(selectedWeaponData.DatabaseID, out WeaponAmmoData ammoData))
            {
                ammoData.CurrentAmmo = ammoData.MaxAmmo;
                weaponsAmmoData[selectedWeaponData.DatabaseID] = ammoData;
                weaponInstance?.PlayReloadSFX();
                OnAmmoChanged?.InvokeSafe(owner, ammoData);
            }
        }

        public bool CanReloadSelectedWeapon()
        {
            var selectedWeapon = GetSelectedWeapon();
            return weaponsAmmoData.TryGetValue(selectedWeapon.DatabaseID, out var ammoData) && ammoData.CurrentAmmo < ammoData.MaxAmmo;
        }

        private void EnsureAmmoData(IWeapon weapon)
        {
            if (ValidateWeapon(weapon))
            {
                WeaponAmmoData ammoData = new()
                {
                    Weapon = weapon,
                    MaxAmmo = weapon.MaxAmmo,
                    CurrentAmmo = weapon.MaxAmmo
                };
                weaponsAmmoData[weapon.DatabaseID] = ammoData;
            }

            bool ValidateWeapon(IWeapon weapon)
            {
                return weapon != null && weapon.MaxAmmo > 0 && weaponsAmmoData.ContainsKey(weapon.ReadableID) is false;
            }
        }

        public async void OnItemEquipped(IEquipableItem item)
        {
            if (item is IWeapon weapon)
            {
                switch (selectedWeapon)
                {
                    case EquipmentSlotID.PrimaryWeapon:
                        if (GetPrimaryWeapon() == weapon)
                            await RebuildWeaponInstance();
                        break;
                    case EquipmentSlotID.SecondaryWeapon:
                        if (GetSecondaryWeapon() == weapon)
                            await RebuildWeaponInstance();
                        break;
                }
                EnsureAmmoData(weapon);
                OnWeaponChanged?.Invoke(owner);
            }
        }

        public void OnItemUnequipped(IEquipableItem item)
        {
            if (item is IWeapon)
            {
                switch (selectedWeapon)
                {
                    case EquipmentSlotID.PrimaryWeapon:
                        if (GetPrimaryWeapon() == null && weaponInstance != null)
                            ReleaseCurrentWeaponInstance();
                        break;
                    case EquipmentSlotID.SecondaryWeapon:
                        if (GetSecondaryWeapon() == null && weaponInstance != null)
                            ReleaseCurrentWeaponInstance();
                        break;
                }
                OnWeaponChanged?.Invoke(owner);
            }
        }

        public void OnAnimationEvent(string eventType)
        {
            if (string.Equals(eventType, FIRE_WEAPON_EVENT) && weaponInstance != null)
            {
                weaponInstance.SetWeaponTarget(WeaponTarget);
                weaponInstance.PlayFireWeaponFX();
            }
        }
    }
}
