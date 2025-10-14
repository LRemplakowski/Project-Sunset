using Sirenix.OdinInspector;
using SunsetSystems.Abilities.Execution;
using SunsetSystems.Abilities.Targeting;
using SunsetSystems.Animation;
using SunsetSystems.Combat;
using SunsetSystems.Inventory;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SunsetSystems.Abilities
{
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
        private int _range;
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
        [SerializeField]
        private AssetReferenceGameObject _projectileVfxPrefab;
        [TabGroup("Spell Ability")]
        [SerializeField]
        private AssetReferenceAudioClip _preparationSFX;
        [TabGroup("Spell Ability")]
        [SerializeField]
        private AssetReferenceAudioClip _executionSFX;
        [TabGroup("Spell Ability")]
        [SerializeField]
        private AttributeType _damageAttribute;
        [TabGroup("Spell Ability")]
        [SerializeField]
        private SkillType _damageSkill;
        [TabGroup("Spell Ability")]
        [SerializeField]
        private int _baseDamage;

        public AssetReferenceAudioClip PreparationSFX => _preparationSFX;
        public AssetReferenceAudioClip ExecutionSFX => _executionSFX;
        public AssetReferenceGameObject PreCastVfxPrefab => _preCastVfxPrefab;
        public AssetReferenceGameObject ProjectileVfxPrefab => _projectileVfxPrefab;

        public WeaponAnimationType PreCastAnimationType => _animationType;
        public int CastAnimationHash => Animator.StringToHash(_castAnimationTrigger);

        #region IDisciplinePower
        public string ID => AbilityID.ToString();
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
        public override IAbilityTargetingStrategy GetTargetingStrategy() => _targetingStrategy ??= new TargetCreatureStrategy(this);

        protected override RangeData GetAbilityRangeData(IAbilityContext context)
        {
            return new RangeData(0, _range, _range);
        }

        public int GetDamage(IAbilityContext context)
        {
            int attributeBonus = context.SourceCombatBehaviour.References.StatsManager.GetAttribute(_damageAttribute).Value;
            int skillBonus = context.SourceCombatBehaviour.References.StatsManager.GetSkill(_damageSkill).Value;
            return _baseDamage * (attributeBonus + skillBonus);
        }

        public string GetProjectileLaunchEventArg()
        {
            return _projectileLaunchEvent;
        }
    }
}
