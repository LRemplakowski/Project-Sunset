using System;
using System.Collections;
using System.Collections.Generic;
using SunsetSystems.Abilities;
using SunsetSystems.Combat;
using SunsetSystems.Entities;
using SunsetSystems.Inventory;
using UnityEngine;

namespace SunsetSystems.ActionSystem
{
    public class SpellAbilityAction : HostileAction
    {
        [SerializeField]
        private FlagWrapper _attackFinished;
        [SerializeField]
        private SpellAbility _spellAbility;
        [SerializeField]
        private IAbilityContext _abilityContext;
        [SerializeField]
        private ICombatContext _attackerContext;
        [SerializeField]
        private ICombatContext _targetContext;
        [SerializeField]
        private IDamageable _targetDamageable;

        private IEnumerator _attackRoutine;
        private FaceTarget _faceTargetSubaction;

        public SpellAbilityAction(SpellAbility spellAbility, IAbilityContext context) : base(context.TargetObject, context.SourceCombatBehaviour)
        {
            _spellAbility = spellAbility;
            _abilityContext = context;
            _attackFinished = new() { Value = false };
            if (context.SourceCombatBehaviour is IContextProvider<ICombatContext> attackerContextSource)
                _attackerContext = attackerContextSource.GetContext();
            if (context.TargetObject is IContextProvider<ICombatContext> targetContextSource)
                _targetContext = targetContextSource.GetContext();
            _targetDamageable = context.TargetObject as IDamageable;
        }

        public override void Begin()
        {
            throw new NotImplementedException();
        }
    }
}
