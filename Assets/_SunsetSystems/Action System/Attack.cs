using System;
using System.Collections;
using System.Collections.Generic;
using SunsetSystems.Combat;
using SunsetSystems.Combat.Grid;
using SunsetSystems.DynamicLog;
using SunsetSystems.Entities;
using SunsetSystems.Inventory;
using UnityEngine;

namespace SunsetSystems.ActionSystem
{
    [Obsolete]
    [Serializable]
    public class Attack : HostileAction, ICloneable
    {
        [SerializeField]
        private AttackModifier _attackModifier;
        [SerializeField]
        private FlagWrapper _attackFinished;
        [SerializeField]
        private IDamageable _targetDamageable;
        [SerializeField]
        private GridManager _combatGrid;
        [SerializeField]
        private bool _enableLogging = true;

        private ICombatContext _attackerContext;
        private ICombatContext _targetContext;
        private FaceTarget _faceTargetSubaction;
        private IEnumerator _attackRoutine;

        public Attack(ITargetable target, ICombatant attacker, AttackModifier attackModifier, GridManager combatGrid) : this(target, attacker, combatGrid)
        {
            _attackModifier = attackModifier;
        }

        public Attack(ITargetable target, ICombatant attacker, GridManager combatGrid) : base(target, attacker)
        {
            _attackFinished = new() { Value = false };
            _targetDamageable = target as IDamageable;
            _combatGrid = combatGrid;
            conditions.Add(new WaitForFlag(_attackFinished));
        }

        private Attack(Attack from) : this(from.Target, from.Attacker, from._attackModifier, from._combatGrid)
        {
            _enableLogging = from._enableLogging;
        }

        public object Clone()
        {
            return new Attack(this);
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
                return;
            if (Attacker is IContextProvider<ICombatContext> attackerContextSource)
                _attackerContext = attackerContextSource.GetContext();
            if (Target is IContextProvider<ICombatContext> targetContextSource)
                _targetContext = targetContextSource.GetContext();
            //Debug.Log(Attacker.References.GameObject.name + " attacks " + TargetObject.References.GameObject.name);
            AttackResult result = CombatCalculator.CalculateAttackResult(new AttackContextFromAttackAction(this, _attackModifier));
            if (_enableLogging)
            {
                LogAttack(Attacker, Target, result);
            }
            _attackRoutine = PerformAttack(Attacker, Target, result);
            Attacker.CoroutineRunner.StartCoroutine(_attackRoutine);

            static void LogAttack(ICombatant attacker, ITargetable target, AttackResult result)
            {
                string logMessage = LogUtility.LogMessageFromAttackResult(attacker, target, result);
                DynamicLogManager.Instance.PostLogMessage(logMessage);
                Debug.Log($"Attack hit? {result.Successful}\n" +
                    $"Attacker hit chance = {result.AttackerHitChance}\n" +
                    $"Defender dodge chance = {result.DefenderDodgeChance}\n" +
                    $"Attack roll: {result.HitRoll} vs difficulty {result.AttackerHitChance - result.DefenderDodgeChance}\n" +
                    $"Damage dealt: {result.Damage} - {result.DamageReduction} = {result.AdjustedDamage}");

            }
        }

        private IEnumerator PerformAttack(ICombatant attacker, ITargetable defender, AttackResult attackResult)
        {
            _faceTargetSubaction = new(attacker, _targetContext.Transform, 180f);
            _faceTargetSubaction.Begin();
            while (_faceTargetSubaction.EvaluateAction() is false)
                yield return null;
            attacker.References.AnimationManager.PlayFireWeaponAnimation();
            //defender.References.AnimationManager.PlayTakeHitAnimation();
            yield return new WaitForSeconds(1f);
            if (attackResult.Successful)
                _targetDamageable.TakeDamage(attackResult.AdjustedDamage);
            _attackFinished.Value = true;
        }

        private readonly struct AttackContextFromAttackAction : IAttackContext
        {
            private readonly ICombatant _attacker;
            private readonly ITargetable _target;
            private readonly AttackModifier _attackModifier;
            private readonly ICombatContext _attackerContext;
            private readonly ICombatContext _targetContext;
            private readonly GridManager _combatGrid;

            public AttackContextFromAttackAction(Attack attackAction, AttackModifier attackModifier)
            {
                _attacker = attackAction.Attacker;
                _target = attackAction.Target;
                _attackModifier = attackModifier;
                _attackerContext = attackAction._attackerContext;
                _targetContext = attackAction._targetContext;
                _combatGrid = attackAction._combatGrid;
            }

            public readonly Vector3 GetAimingPosition(AttackParticipant entity)
            {
                return entity switch
                {
                    AttackParticipant.Attacker => _attackerContext.AimingOrigin,
                    AttackParticipant.Target => _targetContext.AimingOrigin,
                    _ => Vector3.zero,
                };
            }

            public readonly int GetAttackDamage()
            {
                int damage = 0;
                IWeapon selectedWeapon = _attackerContext.WeaponManager.GetSelectedWeapon();
                float weaponDamageMod = _attackerContext.WeaponManager.GetPrimaryWeapon().Equals(selectedWeapon) ? 1f : 0.6f;
                switch (selectedWeapon.WeaponType)
                {
                    case AbilityRange.Melee:
                        damage = Mathf.RoundToInt((_attackerContext.GetAttributeValue(AttributeType.Strength) + selectedWeapon.GetDamageData().DamageModifier) * weaponDamageMod);
                        break;
                    case AbilityRange.Ranged:
                        damage = Mathf.RoundToInt((_attackerContext.GetAttributeValue(AttributeType.Composure) + selectedWeapon.GetDamageData().DamageModifier) * weaponDamageMod);
                        break;
                }
                return damage;
            }

            public readonly float GetCriticalDamageMultiplier()
            {
                return 1.5f;
            }

            public readonly RangeData GetAttackRangeData()
            {
                return _attackerContext.WeaponManager.GetSelectedWeapon().GetRangeData();
            }

            public readonly AttackType GetAttackType()
            {
                return GetAttackRangeData().MaxRange > 1 ? AttackType.WeaponRanged : AttackType.WeaponMelee;
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
                return _attackModifier;
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
                float heightDifference = _attacker.References.Transform.position.y - _targetContext.Transform.position.y;
                if (heightDifference > 2f && false/*_attacker.References.GetCachedComponentInChildren<Abilities.SpellbookManager>().GetIsPowerKnown(PassivePowersHelper.Instance.HeightAttackAndDamageBonus)*/)
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
                var gridScale = _combatGrid.GetGridScale();
                return Mathf.CeilToInt(worldDistance / gridScale);
            }
        }
    } 
}