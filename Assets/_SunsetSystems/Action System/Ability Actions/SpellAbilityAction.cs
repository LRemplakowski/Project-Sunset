using System.Collections;
using Sirenix.OdinInspector;
using SunsetSystems.Abilities;
using SunsetSystems.Combat;
using SunsetSystems.Entities;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
        private ICombatContext _targetContext;

        [ShowInInspector]
        private Coroutine _attackRoutine;
        [ShowInInspector]
        private FaceTarget _faceTargetSubaction;
        [ShowInInspector]
        private bool _projectileHit;

        public SpellAbilityAction(SpellAbility spellAbility, IAbilityContext context) : base(context.TargetObject, context.SourceCombatBehaviour)
        {
            _spellAbility = spellAbility;
            _abilityContext = context;
            _attackFinished = new() { Value = false };
            if (context.TargetObject is IContextProvider<ICombatContext> targetContextSource)
                _targetContext = targetContextSource.GetContext();
            conditions.Add(new WaitForFlag(_attackFinished));
        }

        public override void Cleanup()
        {
            base.Cleanup();
            Attacker.References.AnimationManager.OnAnimationEvent -= OnAnimationEvent;
            if (_attackRoutine != null)
            {
                Attacker.CoroutineRunner.StopCoroutine(_attackRoutine);
            }
        }

        public override void Begin()
        {
            Attacker.References.AnimationManager.OnAnimationEvent += OnAnimationEvent;
            _attackRoutine = Attacker.CoroutineRunner.StartCoroutine(ResolveAttack());
        }

        private IEnumerator ResolveAttack()
        {
            if (_spellAbility.GetTargetingData(_abilityContext).GetAbilityTargetingType() != AbilityTargetingType.Self)
            {
                _faceTargetSubaction = new(Attacker, _targetContext.Transform, 180f);
                _faceTargetSubaction.Begin();
                while (_faceTargetSubaction.EvaluateAction() is false)
                    yield return null;
            }
            Attacker.References.AnimationManager.SetTrigger(_spellAbility.CastAnimationHash);
            yield return new WaitUntil(() => _projectileHit);
            var effects = _spellAbility.GetEffects();
            foreach (IAbilityEffect effect in effects)
            {
                effect.ResolveEffect(_spellAbility, _abilityContext);
            }
            _attackFinished.Value = true;
        }

        private async void OnAnimationEvent(string eventArg)
        {
            if (eventArg == _spellAbility.GetProjectileLaunchEventArg())
            {
                if (!_spellAbility.ExecutionVfxPrefab?.RuntimeKeyIsValid() ?? true)
                {
                    _projectileHit = true;
                    return;
                }

                var handPos = Attacker.References.AnimationManager.GetBonePosition(HumanBodyBones.LeftHand);
                var loadingOp = Addressables.InstantiateAsync(_spellAbility.ExecutionVfxPrefab, handPos, Quaternion.identity);
                await loadingOp.Task;
                var projectileGO = loadingOp.Result;
                if (projectileGO.TryGetComponent(out IProjectile projectile))
                {
                    projectile.Launch(Target, OnProjectileHit);
                }
                else
                {
                   Addressables.ReleaseInstance(projectileGO);
                    _projectileHit = true;
                }
            }
        }

        private void OnProjectileHit()
        {
            _projectileHit = true;
        }
    }
}
