using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SunsetSystems.Combat;
using SunsetSystems.UI;
using SunsetSystems.UI.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using SunsetSystems.Inventory;

namespace SunsetSystems.Abilities
{

    [CreateAssetMenu(fileName = "New Power", menuName = "Character/Power")]
    public class DisciplinePower : SerializedScriptableObject, IUserInfertaceDataProvider<DisciplinePower>
    {
        [field: SerializeField]
        public string PowerName { get; private set; }
        [field: SerializeField]
        public AssetReferenceSprite PowerIcon { get; private set; }
        [field: SerializeField]
        public AssetReference PowerGUIButtonAsset { get; private set; }
        [field: SerializeField, MultiLineProperty]
        public string PowerDescription { get; private set; }
        [field: SerializeField]
        public int BloodCost { get; private set; }
        [field: SerializeField]
        public int Cooldown { get; private set; }
        [SerializeField, ReadOnly]
        private string _id = Guid.NewGuid().ToString();
        public string ID => _id;
        [SerializeField]
        private DisciplineType type;
        public DisciplineType Type { get => type; }
        [SerializeField, Range(1, 5)]
        private int level = 1;
        public int Level { get => level; }
        [SerializeField]
        private DisciplineType secondaryType;
        public DisciplineType SecondaryType { get => secondaryType; }
        [SerializeField, Range(0, 5)]
        private int secondaryLevel;
        public int SecondaryLevel { get => secondaryLevel; }

        [SerializeField]
        private AbilityTargetingType _target = AbilityTargetingType.Self;
        public AbilityTargetingType Target { get => _target; }
        [SerializeField]
        private Duration _duration = Duration.Immediate;
        public Duration Duration => _duration;
        [SerializeField]
        private AbilityRange _range = AbilityRange.Ranged;
        public AbilityRange Range { get => _range; }
        [SerializeField]
        private TargetableEntityType _targetableCreatureType;
        public TargetableEntityType TargetableCreatureType { get => _targetableCreatureType; }

        private TooltipContent _powerTooltip;
        public TooltipContent Tooltip => _powerTooltip;
        public DisciplinePower UIData => this;

        [SerializeField]
        private List<IEffectOLD> effects = new();

        private void OnValidate()
        {
            _powerTooltip._title = this.PowerName;
            _powerTooltip._description = this.PowerDescription;
            if (string.IsNullOrWhiteSpace(_id))
                _id = Guid.NewGuid().ToString();
        }

        public IEnumerable<IEffectOLD> GetEffects()
        {
            return effects;
        }


        public bool IsValidTarget(ITargetable target, ISpellbookManager caster)
        {
            return _target switch
            {
                //AbilityTargetingType.Self => target.IsMe(caster.References.CombatBehaviour),
                //AbilityTargetingType.Friendly => target.IsFriendlyTowards(caster.References.CombatBehaviour),
                //AbilityTargetingType.Hostile => target.IsHostileTowards(caster.References.CombatBehaviour),
                //AbilityTargetingType.AOE_Friendly => throw new NotImplementedException(),
                //AbilityTargetingType.AOE_Hostile => throw new NotImplementedException(),
                //AbilityTargetingType.Invalid => false,
                _ => false,
            };
        }
    }
}