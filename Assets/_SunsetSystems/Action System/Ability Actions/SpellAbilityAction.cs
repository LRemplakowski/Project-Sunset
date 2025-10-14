using System.Collections;
using SunsetSystems.Abilities;
using SunsetSystems.Combat;
using SunsetSystems.DynamicLog;
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
        private ICombatContext _attackerContext;
        [SerializeField]
        private ICombatContext _targetContext;
        [SerializeField]
        private IDamageable _targetDamageable;

        private Coroutine _attackRoutine;
        private FaceTarget _faceTargetSubaction;
        private bool _projectileHit;

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
            _faceTargetSubaction = new(Attacker, _targetContext.Transform, 180f);
            _faceTargetSubaction.Begin();
            while (_faceTargetSubaction.EvaluateAction() is false)
                yield return null;
            Attacker.References.AnimationManager.SetTrigger(_spellAbility.CastAnimationHash);
            yield return new WaitUntil(() => _projectileHit);
            int damage = _spellAbility.GetDamage(_abilityContext);
            _targetDamageable.TakeDamage(damage);
            string logMessage = LogUtility.LogMessageFromAttackDamge(Attacker, Target, damage);
            DynamicLogManager.Instance.PostLogMessage(logMessage);
            _attackFinished.Value = true;
        }

        private async void OnAnimationEvent(string eventArg)
        {
            if (!_spellAbility.ProjectileVfxPrefab?.RuntimeKeyIsValid() ?? true) return;

            if (eventArg == _spellAbility.GetProjectileLaunchEventArg())
            {
                var handPos = Attacker.References.AnimationManager.GetBonePosition(HumanBodyBones.LeftHand);
                var loadingOp = Addressables.InstantiateAsync(_spellAbility.ProjectileVfxPrefab, handPos, Quaternion.identity);
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
