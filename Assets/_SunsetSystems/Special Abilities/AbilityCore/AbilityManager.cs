using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using SunsetSystems.ActionSystem;
using SunsetSystems.ActorResources;
using SunsetSystems.Combat;
using SunsetSystems.Combat.Grid;
using SunsetSystems.Entities.Characters;
using SunsetSystems.Input;
using SunsetSystems.Inventory;
using UnityEngine;

namespace SunsetSystems.Abilities
{
    public class AbilityManager : SerializedMonoBehaviour, IAbilityUser
    {
        [SerializeField]
        private ICreatureReferences _references;
        [SerializeField]
        private IMovementPointUser _movementPointUser;
        [SerializeField]
        private IActionPointUser _actionPointUser;
        [SerializeField]
        private IBloodPointUser _bloodPointUser;
        [SerializeField]
        private IAbilitySource _disciplineAbilitySource;
        [OdinSerialize, HideReferenceObjectPicker]
        private IAbilitySource _defaultAbilities = new DefaultAbilitySource();
        private AbilityContext _abilityContext;
        private ITargetable _characterTarget;

        private void Awake()
        {
            _abilityContext = new(this);
            CombatInputHandler.OnTargetingDataUpdate += UpdatePlayerTargetingData;
        }

        private void OnDestroy()
        {
            CombatInputHandler.OnTargetingDataUpdate -= UpdatePlayerTargetingData;
        }

        private void UpdatePlayerTargetingData(ITargetable target)
        {
            if (_references.CreatureData.Faction is not Faction.PlayerControlled)
                return;
            SetCurrentTargetObject(target);
        }

        public void SetCurrentTargetObject(ITargetable targetable)
        {
            _characterTarget = targetable;
        }

        public bool ExecuteAbility(IAbilityConfig ability, Action onCompleted = null)
        {
            if (GetCanAffordAbility(ability) && ConsumeAbilityCost(ability) && ability.IsContextValidForExecution(GetCurrentAbilityContext()))
            {
                ability.GetExecutionStrategy().BeginExecute(GetCurrentAbilityContext(), onCompleted);
                return true;
            }
            return false;
        }

        public async Awaitable<bool> ExecuteAbilityAsync(IAbilityConfig ability)
        {
            if (GetCanAffordAbility(ability) && ConsumeAbilityCost(ability) && ability.IsContextValidForExecution(GetCurrentAbilityContext()))
            {
                await ability.GetExecutionStrategy().BeginExecute(GetCurrentAbilityContext(), null);
                return true;
            }
            return false;
        }

        private bool ConsumeAbilityCost(IAbilityConfig ability)
        {
            var cost = ability.GetAbilityCosts(GetCurrentAbilityContext());
            bool result = true;
            result &= _movementPointUser.UseMovementPoints(cost.MovementCost);
            result &= _actionPointUser.UseActionPoints(cost.ActionPointCost);
            result &= _bloodPointUser.UseBloodPoints(cost.BloodCost);
            if (ability is IAmmoAbility ammoAbility)
                result &= ConsumeAmmo(ammoAbility);
            return result;
        }

        public IAbilityContext GetCurrentAbilityContext()
        {
            return _abilityContext;
        }

        public bool GetCanAffordAbility(IAbilityConfig ability)
        {
            if (ability == null)
                return false;
            var cost = ability.GetAbilityCosts(GetCurrentAbilityContext());
            bool result = true;
            result &= _movementPointUser.GetCurrentMovementPoints() >= cost.MovementCost;
            result &= _actionPointUser.GetCurrentActionPoints() >= cost.ActionPointCost;
            result &= _bloodPointUser.GetCurrentBloodPoints() >= cost.BloodCost;
            if (ability is IAmmoAbility ammoAbility)
                result &= HasEnoughAmmo(ammoAbility);
            return result;
        }

        public bool GetHasValidAbilityContext(IAbilityConfig ability)
        {
            return ability.IsContextValidForExecution(GetCurrentAbilityContext());
        }

        public IEnumerable<AbilityRuntimeData> GetAllAbilities()
        {
            return GetCoreAbilities().Union(GetAbilitiesFromDisciplines()).ToList();
        }

        public IEnumerable<AbilityRuntimeData> GetCoreAbilities()
        {
            if (_defaultAbilities == null)
            {
                Debug.LogWarning($"{nameof(AbilityManager)} >>> No Default Abilities assigned!");
                return Enumerable.Empty<AbilityRuntimeData>();
            }

            return _defaultAbilities.GetAbilities().Select(ability => new AbilityRuntimeData(ability, _defaultAbilities)).Union(GetAbilitiesFromEquipment()).ToList();
        }

        public IEnumerable<AbilityRuntimeData> GetNonCoreAbilities()
        {
            return GetAbilitiesFromDisciplines().ToList();
        }

        private IEnumerable<AbilityRuntimeData> GetAbilitiesFromEquipment()
        {
            var selectedWeapon = _references.WeaponManager.GetSelectedWeapon();
            var equippedItems = _references.EquipmentManager.EquippedItems;
            var result = equippedItems.OfType<IAbilitySource>()
                                .Where(item => item is not IWeapon weapon || weapon == selectedWeapon)
                                .SelectMany(item => item.GetAbilities()
                                                        .Select(ability => new AbilityRuntimeData(ability, item)));
            return result;
        }

        private IEnumerable<AbilityRuntimeData> GetAbilitiesFromDisciplines()
        {
            if (_disciplineAbilitySource == null)
            {
                Debug.LogWarning($"{nameof(AbilityManager)} >>> No Discipline Ability Source assigned!");
                return Enumerable.Empty<AbilityRuntimeData>();
            }

            var result = _disciplineAbilitySource.GetAbilities()
                                           .Select(power => new AbilityRuntimeData(power, _disciplineAbilitySource));
            return result;
        }

        private ITargetable GetCurrentTargetObject()
        {
            return _characterTarget;
        }

        private bool HasEnoughAmmo(IAmmoAbility ammoAbility)
        {
            return _references.WeaponManager.GetSelectedWeaponAmmoData().CurrentAmmo >= ammoAbility.GetAmmoPerUse() * ammoAbility.GetUsesPerExecution();
        }

        private bool ConsumeAmmo(IAmmoAbility ammoAbility)
        {
            var ammoCost = ammoAbility.GetAmmoPerUse() * ammoAbility.GetUsesPerExecution();
            return _references.WeaponManager.UseAmmoFromSelectedWeapon(ammoCost);
        }

        private class AbilityContext : IAbilityContext
        {
            private readonly AbilityManager _abilityManager;
            private readonly Func<ITargetable> _targetCharacter;

            public IActionPerformer SourceActionPerformer => SourceCombatBehaviour;
            public ICombatant SourceCombatBehaviour => _abilityManager._references.CombatBehaviour;
            public ITargetable TargetObject => _targetCharacter.Invoke();
            public GridManager GridManager => CombatManager.Instance.CurrentEncounter.GridManager;

            public AbilityContext(AbilityManager abilityManager)
            {
                _abilityManager = abilityManager;
                _targetCharacter = abilityManager.GetCurrentTargetObject;
            }
        }

        [Serializable]
        private class DefaultAbilitySource : IAbilitySource
        {
            [SerializeField]
            private List<IAbilityConfig> _defaultAbilities = new();

            public IEnumerable<IAbilityConfig> GetAbilities() => _defaultAbilities;
        }
    }
}
