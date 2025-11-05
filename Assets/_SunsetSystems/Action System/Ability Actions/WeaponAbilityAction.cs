using System;
using System.Collections;
using System.Collections.Generic;
using SunsetSystems.Abilities;
using SunsetSystems.Combat;
using SunsetSystems.DynamicLog;
using SunsetSystems.Entities;
using SunsetSystems.Inventory;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SunsetSystems.ActionSystem
{
    public class WeaponAbilityAction : HostileAction
    {
        public static event Action<ICombatant, ITargetable, AttackResult> OnAttackResolved;

        [SerializeField]
        private FlagWrapper _attackFinished;
        [SerializeField]
        private WeaponAttackAbility _weaponAbility;
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

        public WeaponAbilityAction(WeaponAttackAbility weaponAbility, IAbilityContext context) : base(context.TargetObject, context.SourceCombatBehaviour)
        {
            _weaponAbility = weaponAbility;
            _abilityContext = context;
            _attackFinished = new() { Value = false };
            if (context.SourceCombatBehaviour is IContextProvider<ICombatContext> attackerContextSource)
                _attackerContext = attackerContextSource.GetContext();
            if (context.TargetObject is IContextProvider<ICombatContext> targetContextSource)
                _targetContext = targetContextSource.GetContext();
            _targetDamageable = context.TargetObject as IDamageable;
        }

        public override void Cleanup()
        {
            base.Cleanup();
            if (_attackRoutine != null)
            {
                Attacker.CoroutineRunner.StopCoroutine(_attackRoutine);
            }
        }

        public override void Begin()
        {
            if (_attackRoutine != null)
            {
                return;
            }
            conditions.Add(new WaitForFlag(_attackFinished));
            Debug.Log(_attackerContext.GameObject.name + " attacks " + _targetContext.GameObject.name);
            _attackRoutine = PerformAttack();
            Attacker.CoroutineRunner.StartCoroutine(_attackRoutine);
        }

        private IEnumerator PerformAttack()
        {
            IAttackContext attackContext = new AttackContextFromWeaponAbilityAction(this);
            _faceTargetSubaction = new(Attacker, _targetContext.Transform, 180f);
            _faceTargetSubaction.Begin();
            while (_faceTargetSubaction.EvaluateAction() is false)
                yield return null;
            HandleAttackAnimation();
            HandleAttackSFX();
            AttackResult result;
            for (int i = 0; i < _weaponAbility.GetUsesPerExecution(); i++)
            {
                result = CombatCalculator.CalculateAttackResult(in attackContext);
                LogAttack(Attacker, Target, in result);
                if (result.Successful)
                {
                    _targetDamageable.TakeDamage(result.AdjustedDamage);
                    if (_weaponAbility is IBloodAbility bloodSource && bloodSource.GetBloodGained() > 0)
                    {
                        int bloodGained = bloodSource.GetBloodGained();
                        Attacker.References.BloodPointManager.GainBloodPoints(bloodGained);
                        DynamicLogManager.Instance.PostLogMessage(LogUtility.LogMessageFromFeeding(Attacker, Target, bloodGained));
                    }
                }
                OnAttackResolved?.Invoke(Attacker, Target, result);
                yield return new WaitForSeconds(_weaponAbility.GetDelayBetweenAttacks());
            }
            _attackFinished.Value = true;
            _attackRoutine = null;
            Attacker.References.WeaponManager.WeaponTarget = null;

            static void LogAttack(ICombatant attacker, ITargetable target, in AttackResult result)
            {
                string logMessage = LogUtility.LogMessageFromAttackResult(attacker, target, in result);
                DynamicLogManager.Instance.PostLogMessage(logMessage);
                Debug.Log($"Attack hit? {result.Successful}\n" +
                    $"Attacker hit chance = {result.AttackerHitChance}\n" +
                    $"Defender dodge chance = {result.DefenderDodgeChance}\n" +
                    $"Attack roll: {result.HitRoll} vs difficulty {result.AttackerHitChance - result.DefenderDodgeChance}\n" +
                    $"Damage dealt: {result.Damage} - {result.DamageReduction} = {result.AdjustedDamage}");
            }
        }

        private void HandleAttackAnimation()
        {
            if (_weaponAbility is IAnimatedAbility animatedAbility)
            {
                Attacker.References.AnimationManager.SetTrigger(animatedAbility.CastAnimationHash);
            }
            else
            {
                Attacker.References.WeaponManager.WeaponTarget = Target;
                Attacker.References.AnimationManager.PlayFireWeaponAnimation();
            }
        }

        private void HandleAttackSFX()
        {
            if (_weaponAbility is ISFXAbility sfxAbility)
            {
                if (sfxAbility.ExecutionSFX.RuntimeKeyIsValid())
                {
                    Attacker.CoroutineRunner.StartCoroutine(PlayAudio(sfxAbility.ExecutionSFX));
                }
            }
        }

        private IEnumerator PlayAudio(AssetReferenceAudioClip audio)
        {
            var loadingOP = Addressables.LoadAssetAsync<AudioClip>(audio);
            yield return loadingOP;
            var audioSource = Attacker.References.GetCachedComponent<AudioSource>();
            audioSource.clip = loadingOP.Result;
            audioSource.Play();
            Addressables.Release(loadingOP);
        }

        private readonly struct AttackContextFromWeaponAbilityAction : IAttackContext
        {
            private readonly WeaponAttackAbility _ability;
            private readonly ICombatant _attacker;
            private readonly ITargetable _target;
            private readonly IAbilityContext _abilityContext;
            private readonly IAbilityTargetingData _abilityTargetingData;
            private readonly ICombatContext _attackerContext;
            private readonly ICombatContext _targetContext;

            public AttackContextFromWeaponAbilityAction(WeaponAbilityAction action)
            {
                _ability = action._weaponAbility;
                _attacker = action.Attacker;
                _target = action.Target;
                _abilityContext = action._abilityContext;
                _abilityTargetingData = _ability.GetTargetingData(_abilityContext);
                _attackerContext = action._attackerContext;
                _targetContext = action._targetContext;
            }

            public readonly Vector3 GetAimingPosition(AttackParticipant entity)
            {
                return entity switch
                {
                    AttackParticipant.Attacker => _attackerContext.Transform.position,
                    AttackParticipant.Target => _targetContext.Transform.position,
                    _ => Vector3.zero,
                };
            }

            public readonly int GetAttackDamage()
            {
                int damage = 0;
                bool isFeedingAttack = _ability is FeedAttackAbility;
                float weaponDamageMod = _attackerContext.IsUsingPrimaryWeapon || isFeedingAttack ? 1f : 0.6f;
                if (!isFeedingAttack)
                {
                    damage += _attackerContext.SelectedWeaponDamageBonus;
                }
                damage += _ability.GetDamageBonus();
                damage += GetAttackType() switch
                {
                    AttackType.WeaponMelee => _attackerContext.GetAttributeValue(AttributeType.Strength),
                    AttackType.WeaponRanged => _attackerContext.GetAttributeValue(AttributeType.Composure),
                    _ => throw new NotImplementedException(),
                };
                return Mathf.RoundToInt(damage * weaponDamageMod);
            }

            public readonly float GetCriticalDamageMultiplier()
            {
                return 1.5f;
            }

            public readonly RangeData GetAttackRangeData()
            {
                return _abilityTargetingData.GetRangeData();
            }

            public readonly AttackType GetAttackType()
            {
                return _ability.GetAttackType();
            }

            public readonly int GetAttributeValue(AttackParticipant entity, AttributeType attributeType)
            {
                return entity switch
                {
                    AttackParticipant.Attacker => _attackerContext.GetAttributeValue(attributeType),
                    AttackParticipant.Target => _targetContext.GetAttributeValue(attributeType),
                    _ => 0,
                };
            }

            public readonly AttackModifier GetBaseAttackModifier()
            {
                return new();
            }

            public readonly IEnumerable<ICover> GetCoverSources(AttackParticipant entity)
            {
                return entity switch
                {
                    AttackParticipant.Attacker => _attackerContext.CurrentCoverSources,
                    AttackParticipant.Target => _targetContext.CurrentCoverSources,
                    _ => new List<ICover>(),
                };
            }

            public readonly int GetDamageReduction()
            {
                return GetAttackType() switch
                {
                    AttackType.WeaponMelee => _attackerContext.GetAttributeValue(AttributeType.Stamina),
                    AttackType.WeaponRanged => _targetContext.GetAttributeValue(AttributeType.Dexterity),
                    AttackType.MagicMelee => throw new System.NotImplementedException(),
                    AttackType.MagicRanged => throw new System.NotImplementedException(),
                    AttackType.WeaponAOE => throw new System.NotImplementedException(),
                    AttackType.MagicAOE => throw new System.NotImplementedException(),
                    _ => int.MaxValue,
                };
            }

            public readonly AttackModifier GetHeightAttackModifier()
            {
                AttackModifier heightAttackMod = new();
                float heightDifference = _attackerContext.Transform.position.y - _targetContext.Transform.position.y;
                if (heightDifference > 2f && false)
                {
                    heightAttackMod.HitChanceMod += .1d;
                    heightAttackMod.DamageMod += 2;
                }
                return heightAttackMod;
            }

            public readonly Vector3 GetPosition(AttackParticipant entity)
            {
                return entity switch
                {
                    AttackParticipant.Attacker => _attackerContext.Transform.position,
                    AttackParticipant.Target => _targetContext.Transform.position,
                    _ => Vector3.zero,
                };
            }

            public readonly bool IsInCover(AttackParticipant entity)
            {
                return entity switch
                {
                    AttackParticipant.Attacker => _attackerContext.IsInCover,
                    AttackParticipant.Target => _targetContext.IsInCover,
                    _ => false,
                };
            }

            public readonly bool IsPlayerControlled(AttackParticipant entity)
            {
                return entity switch
                {
                    AttackParticipant.Attacker => _attackerContext.IsPlayerControlled,
                    AttackParticipant.Target => _targetContext.IsPlayerControlled,
                    _ => false,
                };
            }

            public readonly int GetGridDistanceBetweenParticipants()
            {
                var worldDistance = Vector3.Distance(_attackerContext.Transform.position, _targetContext.Transform.position);
                var gridScale = CombatManager.Instance.CurrentEncounter.GridManager.GetGridScale();
                return Mathf.CeilToInt(worldDistance / gridScale);
            }
        }
    }
}
