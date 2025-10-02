using System;
using SunsetSystems.Abilities.Execution;
using SunsetSystems.Abilities.Targeting;
using SunsetSystems.Combat;
using SunsetSystems.Entities;
using SunsetSystems.Inventory;
using UnityEngine;

namespace SunsetSystems.Abilities
{
    [CreateAssetMenu(fileName = "New Reload Ability", menuName = "Sunset Abilities/Weapon Reload")]
    public class ReloadWeaponAbility : AbstractAbilityConfig
    {
        [SerializeField]
        private IAbilityConfig _reloadedAbility;
        private IAbilityExecutionStrategy _executionStrategy;
        private IAbilityTargetingStrategy _targetingStrategy;

        protected override bool ValidateAbilityTarget(IAbilityContext context)
        {
            return GetTargetHasCombatContext(context, out var combatContext)
                   && IsTargetWeaponUsingAmmo(combatContext);

            static bool GetTargetHasCombatContext(IAbilityContext context, out ICombatContext combatContext)
            {
                combatContext = default;
                if (context.TargetObject is IContextProvider<ICombatContext> combatContextProvider)
                {
                    combatContext = combatContextProvider.GetContext();
                    return true;
                }
                return false;
            }

            static bool IsTargetWeaponUsingAmmo(ICombatContext context)
            {
                return context != null && context.IsSelectedWeaponUsingAmmo;
            }
        }

        protected override RangeData GetAbilityRangeData(IAbilityContext context)
        {
            return new();
        }

        public override IAbilityExecutionStrategy GetExecutionStrategy() => _executionStrategy ??= new ReloadStrategy();
        public override IAbilityTargetingStrategy GetTargetingStrategy() => _targetingStrategy ??= new SelfTargetStrategy(this);

        internal IAbilityConfig GetReloadedAbility() => _reloadedAbility;
    }
}
