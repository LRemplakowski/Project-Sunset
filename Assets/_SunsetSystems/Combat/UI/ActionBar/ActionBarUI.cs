using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SunsetSystems.Abilities;
using SunsetSystems.Equipment;
using UnityEngine;

namespace SunsetSystems.Combat.UI
{
    public class ActionBarUI : SerializedMonoBehaviour
    {
        [Title("Config")]
        [SerializeField]
        private Transform _coreButtonsParent;
        [SerializeField, AssetsOnly]
        private IAbilityButtonFactory _buttonFactory;

        private readonly Dictionary<Guid, Action<WeaponAmmoData>> _ammoUpdatesMap = new();

        private IAbilityConfig _cachedLastSelectedAbility;
        public static event Action<IAbilityConfig> OnAbilitySelected;

        public void RefreshAvailableActions()
        {
            RefreshCoreAbilities();
        }
        
        // Updated ability should be passed as argument
        // Weapon should own it's abilities
        public void UpdateAmmoCounter(in WeaponAmmoData ammoData)
        {
            IAbilityConfig abilityToUpdate = _cachedLastSelectedAbility;
            if (abilityToUpdate != null && abilityToUpdate is ReloadWeaponAbility reloadAbility)
                abilityToUpdate = reloadAbility.GetReloadedAbility();
            if (abilityToUpdate != null && _ammoUpdatesMap.TryGetValue(abilityToUpdate.AbilityID, out var onAmmoUpdate))
            {
                onAmmoUpdate?.Invoke(ammoData);
            }
        }

        private void RefreshCoreAbilities()
        {
            _ammoUpdatesMap.Clear();
            _coreButtonsParent.DestroyChildren();
            foreach (var ability in GetCoreAbilities())
            {
                _buttonFactory.Create(_coreButtonsParent, ability, SelectAbility, out var onAmmoUpdate);
                if (ability is IAmmoAbility)
                {
                    _ammoUpdatesMap.TryAdd(ability.AbilityID, onAmmoUpdate);
                }
            }
        }

        private void SelectAbility(IAbilityConfig ability)
        {
            _cachedLastSelectedAbility = ability;
            OnAbilitySelected?.Invoke(ability);
        }

        private IEnumerable<IAbilityConfig> GetCoreAbilities()
        {
            return CombatManager.Instance.CurrentActiveActor.GetContext().AbilityUser.GetCoreAbilities();
        }
    }
}