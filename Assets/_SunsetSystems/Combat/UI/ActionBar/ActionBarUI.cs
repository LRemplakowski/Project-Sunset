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
        [SerializeField]
        private Transform _disciplineButtonsParent;
        [SerializeField, AssetsOnly]
        private IAbilityButtonFactory _buttonFactory;

        private readonly ICollection<IAbilityButton> _abilityButtons = new List<IAbilityButton>();

        public static event Action<IAbilityConfig> OnAbilitySelected;

        private void Start()
        {
            CleanupPreviousView();
        }

        public void RefreshAvailableActions()
        {
            CleanupPreviousView();
            RefreshCoreAbilities();
            RefreshDisciplineAbilities();
        }
        
        public void UpdateAmmoCounter(in WeaponAmmoData ammoData)
        {
            foreach (var abilityButton in _abilityButtons)
            {
                abilityButton.UpdateAmmoData(in ammoData);
            }
        }

        private void CleanupPreviousView()
        {
            _abilityButtons.Clear();
            _coreButtonsParent.DestroyChildren();
            _disciplineButtonsParent.DestroyChildren();
        }

        private void RefreshCoreAbilities()
        {
            var coreAbilities = GetCoreAbilities();
            foreach (var abilityData in coreAbilities)
            {
                var button = _buttonFactory.Create(_coreButtonsParent, abilityData.AbilityConfig, abilityData.AbilitySource, SelectAbility);
                _abilityButtons.Add(button);
            }
        }

        private void RefreshDisciplineAbilities()
        {
            var disciplines = GetDisciplineAbilities();
            foreach (var abilityData in disciplines)
            {
                var button = _buttonFactory.Create(_disciplineButtonsParent, abilityData.AbilityConfig, abilityData.AbilitySource, SelectAbility);
                _abilityButtons.Add(button);
            }
        }

        private void SelectAbility(IAbilityConfig ability)
        {
            OnAbilitySelected?.Invoke(ability);
        }

        private IEnumerable<AbilityRuntimeData> GetCoreAbilities()
        {
            return CombatManager.Instance.CurrentActiveActor.GetContext().AbilityUser.GetCoreAbilities();
        }

        private IEnumerable<AbilityRuntimeData> GetDisciplineAbilities()
        {
            return CombatManager.Instance.CurrentActiveActor.GetContext().AbilityUser.GetNonCoreAbilities();
        }
    }
}