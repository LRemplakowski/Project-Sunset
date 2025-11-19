using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SunsetSystems.Abilities.Execution;
using SunsetSystems.Abilities.Targeting;
using SunsetSystems.Animation;
using SunsetSystems.Combat;
using SunsetSystems.Inventory;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace SunsetSystems.Abilities
{
    public interface IAOEAbility
    {
        int GetAOERadius(IAbilityContext context);
    }

    public interface IAbilityEffect
    {
        void ResolveEffect(IAbilityConfig ability, IAbilityContext context);
    }

    public interface IAnimatedAbility
    {
        WeaponAnimationType PreCastAnimationType { get; }
        int CastAnimationHash { get; }
    }

    public interface ISFXAbility
    {
        AssetReferenceAudioClip PreparationSFX { get; }
        AssetReferenceAudioClip ExecutionSFX { get; }
    }

    public interface IVFXAbility
    {
        AssetReferenceGameObject PreCastVfxPrefab { get; }
        AssetReferenceGameObject ExecutionVfxPrefab { get; }
    }

    [CreateAssetMenu(fileName = "New Spell Ability", menuName = "Sunset Abilities/Spell Ability")]
    public class SpellAbility : AbstractAbilityConfig, IAnimatedAbility, ISFXAbility, IVFXAbility, IDisciplinePower
    {
        [BoxGroup("Discipline Power Data")]
        [SerializeField]
        private IDiscipline _discipline;
        [BoxGroup("Discipline Power Data")]
        [SerializeField, PropertyRange(1, 5)]
        private int _powerLevel = 1;
        [TabGroup("Spell Ability")]
        [SerializeField]
        private float _range;
        [TabGroup("Spell Ability")]
        [SerializeField]
        private string _projectileLaunchEvent = "SPELL_1H_PROJECTILE_LAUNCH";
        [TabGroup("Spell Ability")]
        [SerializeField]
        private WeaponAnimationType _animationType = WeaponAnimationType.SpellCast;
        [TabGroup("Spell Ability")]
        [SerializeField]
        private string _castAnimationTrigger;
        [TabGroup("Spell Ability")]
        [SerializeField]
        private AssetReferenceGameObject _preCastVfxPrefab;
        [TabGroup("Spell Ability")]
        [SerializeField, FormerlySerializedAs("_projectileVfxPrefab")]
        private AssetReferenceGameObject _executionVfxPrefab;
        [TabGroup("Spell Ability")]
        [SerializeField]
        private AssetReferenceAudioClip _preparationSFX;
        [TabGroup("Spell Ability")]
        [SerializeField]
        private AssetReferenceAudioClip _executionSFX;
        [TabGroup("Spell Ability")]
        [SerializeField]
        private List<IAbilityEffect> _spellEffects = new();

        public AssetReferenceAudioClip PreparationSFX => _preparationSFX;
        public AssetReferenceAudioClip ExecutionSFX => _executionSFX;
        public AssetReferenceGameObject PreCastVfxPrefab => _preCastVfxPrefab;
        public AssetReferenceGameObject ExecutionVfxPrefab => _executionVfxPrefab;

        public WeaponAnimationType PreCastAnimationType => _animationType;
        public int CastAnimationHash => Animator.StringToHash(_castAnimationTrigger);

        #region IDisciplinePower
        public string ID => DatabaseID;
        public string ScriptName => ReadableID;
        public string Name => GetLocalizedName();
        public string Description => GetLocalizedDescription();
        public IDiscipline Discipline => _discipline;
        public int Level => _powerLevel;
        #endregion

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
        public override IAbilityTargetingStrategy GetTargetingStrategy()
        {
            return _targetingStrategy ??= CreateStrategy();
        }

        private IAbilityTargetingStrategy CreateStrategy()
        {
            return _abilityTargetingType switch
            {
                AbilityTargetingType.Friendly => new TargetCreatureStrategy(this),
                AbilityTargetingType.Hostile => new TargetCreatureStrategy(this),
                AbilityTargetingType.Self => new SelfTargetStrategy(this),
                _ => throw new NotImplementedException()
            };
        }

        protected override RangeData GetAbilityRangeData(IAbilityContext context)
        {
            return new()
            {
                ShortRange = 0,
                OptimalRange = _range,
                MaxRange = _range
            };
        }

        public IReadOnlyCollection<IAbilityEffect> GetEffects()
        {
            return _spellEffects;
        }

        public string GetProjectileLaunchEventArg()
        {
            return _projectileLaunchEvent;
        }
    }
}
