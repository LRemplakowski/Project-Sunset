using SunsetSystems.Abilities;
using SunsetSystems.Abilities.Execution;
using SunsetSystems.Abilities.Targeting;
using SunsetSystems.Animation;
using SunsetSystems.Combat;
using SunsetSystems.Inventory;
using UnityEngine;

namespace SunsetSystems
{
    public interface IAnimatedAbility
    {
        WeaponAnimationType PreCastAnimationType { get; }
        int CastAnimationHash { get; }
    }

    public interface ISFXAbility
    {
        AudioClip PreparatioSFX { get; }
        AudioClip ExecutionSFX { get; }
    }

    public interface IVFXAbility
    {
        GameObject PreCastVfxPrefab { get; }
    }

    [CreateAssetMenu(fileName = "New Spell Ability", menuName = "Sunset Abilities/Spell Ability")]
    public class SpellAbility : AbstractAbilityConfig, IAnimatedAbility, ISFXAbility, IVFXAbility
    {
        [SerializeField]
        private int _range;
        [SerializeField]
        private WeaponAnimationType _animationType = WeaponAnimationType.SpellCast;
        [SerializeField]
        private string _castAnimationTrigger;
        [SerializeField]
        private GameObject _preCastVfxPrefab;
        [SerializeField]
        private GameObject _projectileVfxPrefab;
        [SerializeField]
        private AudioClip _preparationSFX;
        [SerializeField]
        private AudioClip _executionSFX;

        public AudioClip PreparatioSFX => _preparationSFX;
        public AudioClip ExecutionSFX => _executionSFX;
        public GameObject PreCastVfxPrefab => _preCastVfxPrefab;
        public GameObject ProjectileVfxPrefab => _projectileVfxPrefab;

        public WeaponAnimationType PreCastAnimationType => _animationType;
        public int CastAnimationHash => Animator.StringToHash(_castAnimationTrigger);

        private IAbilityExecutionStrategy _executionStrategy;
        private IAbilityTargetingStrategy _targetingStrategy;

        protected override bool ValidateAbilityTarget(IAbilityContext context)
        {
            return IsTargetDamageable(context);

            static bool IsTargetDamageable(IAbilityContext context)
            {
                return context.TargetObject is IDamageable;
            }
        }

        public override IAbilityExecutionStrategy GetExecutionStrategy() => _executionStrategy ??= new AttackStrategyFromSpellAbility(this);
        public override IAbilityTargetingStrategy GetTargetingStrategy() => _targetingStrategy ??= new TargetCreatureStrategy(this);

        protected override RangeData GetAbilityRangeData(IAbilityContext context)
        {
            return new RangeData(0, _range, _range);
        }
    }
}
