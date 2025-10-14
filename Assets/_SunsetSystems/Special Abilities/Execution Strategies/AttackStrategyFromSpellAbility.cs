using System;
using SunsetSystems.ActionSystem;
using UnityEngine;

namespace SunsetSystems.Abilities.Execution
{
    public class AttackStrategyFromSpellAbility : IAbilityExecutionStrategy
    {
        private readonly SpellAbility _ability;

        public AttackStrategyFromSpellAbility(SpellAbility ability)
        {
            _ability = ability;
        }

        public async Awaitable BeginExecute(IAbilityContext context, Action onCompleted)
        {
            var actionPerformer = context.SourceActionPerformer;
            var spellAction = new SpellAbilityAction(_ability, context);
            await actionPerformer.PerformAction(spellAction);
            onCompleted?.Invoke();
        }
    }
}