using System;
using SunsetSystems.ActionSystem;
using UnityEngine;

namespace SunsetSystems.Abilities.Execution
{
    public class AttackStrategyFromWeaponAbility : IAbilityExecutionStrategy
    {
        private readonly WeaponAttackAbility _ability;

        public AttackStrategyFromWeaponAbility(WeaponAttackAbility ability)
        {
            _ability = ability;
        }

        public async Awaitable BeginExecute(IAbilityContext context, Action onCompleted)
        {
            var actionPerformer = context.SourceActionPerformer;
            var shootingAction = new WeaponAbilityAction(_ability, context);
            await actionPerformer.PerformAction(shootingAction);
            onCompleted?.Invoke();
        }
    }
}