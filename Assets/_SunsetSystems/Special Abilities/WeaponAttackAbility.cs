using Sirenix.OdinInspector;
using SunsetSystems.Abilities.Execution;
using SunsetSystems.Abilities.Targeting;
using SunsetSystems.Combat;
using SunsetSystems.Inventory;
using UnityEngine;

namespace SunsetSystems.Abilities
{

    [CreateAssetMenu(fileName = "New Weapon Ability", menuName = "Sunset Abilities/Weapon Attack")]
    public class WeaponAttackAbility : AbstractAbilityConfig, IAmmoAbility
    {
        [SerializeField, BoxGroup("Ability Range"), MinValue(0)]
        private float _baseAbilityRange = 1;
        [SerializeField, BoxGroup("Ability Range"), MinValue(0)]
        private float _rangeFalloff = 1;
        [BoxGroup("Weapon Ability")]
        [SerializeField, MinValue(1)]
        private int _numberOfAttacks = 1;
        [BoxGroup("Weapon Ability")]
        [SerializeField, PropertyRange(0, 5)]
        private float _delayBetweenAttacks = .2f;
        [BoxGroup("Weapon Ability")]
        [SerializeField, MinValue(0)]
        private int _ammoPerAttack = 0;
        [BoxGroup("Weapon Ability")]
        [SerializeField]
        private int _damageBonus = 0;
        [BoxGroup("Weapon Ability")]
        [SerializeField]
        private AttackType _attackType = AttackType.WeaponMelee;
        [BoxGroup("Weapon Ability")]
        [SerializeField]
        private DamageType _damageType = DamageType.Piercing;

        private IAbilityExecutionStrategy _executionStrategy;
        private IAbilityTargetingStrategy _targetingStrategy;

        public bool UsesAmmo => _ammoPerAttack > 0;

        protected override bool ValidateAbilityTarget(IAbilityContext context)
        {
            return IsTargetDamageable(context);

            static bool IsTargetDamageable(IAbilityContext context)
            {
                return context.TargetObject is IDamageable;
            }
        }

        protected override RangeData GetAbilityRangeData(IAbilityContext context)
        {
            return new()
            {
                OptimalRange = _baseAbilityRange,
                MaxRange = _baseAbilityRange + (2 * _rangeFalloff),
                ShortRange = _baseAbilityRange - _rangeFalloff
            };
        }

        public int GetAmmoPerUse() => _ammoPerAttack;
        public int GetUsesPerExecution() => _numberOfAttacks;
        public int GetDamageBonus() => _damageBonus;
        public float GetDelayBetweenAttacks() => _delayBetweenAttacks;
        public AttackType GetAttackType() => _attackType;
        public DamageType GetDamageType() => _damageType;

        public override IAbilityExecutionStrategy GetExecutionStrategy() => _executionStrategy ??= new AttackStrategyFromWeaponAbility(this);
        public override IAbilityTargetingStrategy GetTargetingStrategy() => _targetingStrategy ??= new TargetCreatureStrategy(this);
    }
}
