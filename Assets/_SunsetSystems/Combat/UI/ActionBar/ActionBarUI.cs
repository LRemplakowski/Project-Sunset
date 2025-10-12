using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using SunsetSystems.Abilities;
using SunsetSystems.Equipment;
using SunsetSystems.Inventory.Data;
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

        private readonly ICollection<IAbilityButton> _abilityButtons = new List<IAbilityButton>();

        public static event Action<IAbilityConfig> OnAbilitySelected;

        public void RefreshAvailableActions()
        {
            RefreshCoreAbilities();
        }
        
        public void UpdateAmmoCounter(in WeaponAmmoData ammoData)
        {
            foreach (var abilityButton in _abilityButtons)
            {
                abilityButton.UpdateAmmoData(in ammoData);
            }
        }

        private void RefreshCoreAbilities()
        {
            _abilityButtons.Clear();
            _coreButtonsParent.DestroyChildren();
            foreach (var abilityData in GetAbilitiesBySource())
            {
                var button = _buttonFactory.Create(_coreButtonsParent, abilityData.AbilityConfig, abilityData.AbilitySource, SelectAbility);
                _abilityButtons.Add(button);
            }
        }

        private void SelectAbility(IAbilityConfig ability)
        {
            OnAbilitySelected?.Invoke(ability);
        }

        private IEnumerable<AbilityRuntimeData> GetAbilitiesBySource()
        {
            return CombatManager.Instance.CurrentActiveActor.GetContext().AbilityUser.GetCoreAbilities();
        }
    }
}