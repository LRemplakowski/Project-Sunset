using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SunsetSystems.Inventory;
using UnityEngine;

namespace SunsetSystems.Abilities
{
    public abstract class AbstractAbilityConfig : SerializedScriptableObject, IAbilityConfig
    {
        [SerializeField]
        private Guid _abilityID = Guid.NewGuid();
        public Guid AbilityID => _abilityID;
        [TabGroup("UI Data")]
        [SerializeField]
        private string _fallbackName;
        [TabGroup("UI Data")]
        [SerializeField, MultiLineProperty]
        private string _fallbackDescription;
        [TabGroup("UI Data")]
        [SerializeField, DictionaryDrawerSettings(IsReadOnly = true)]
        private Dictionary<IAbilityUIData.IconState, Sprite> _icons = new();
        [SerializeField, TabGroup("Ability Core")]
        private AbilityCategory _categoryMask;
        [SerializeField, TabGroup("Ability Core")]
        protected TargetableEntityType _validTargetsMask;
        [SerializeField, TabGroup("Ability Core")]
        protected AbilityRange _targetingDistanceType;
        [SerializeField, TabGroup("Ability Core")]
        protected AbilityTargetingType _abilityTargetingType;
        [SerializeField, TabGroup("Ability Cost")]
        protected int _baseMovementCost = 0, _baseAPCost = 0, _baseBloodCost = 0;

        private void OnValidate()
        {
            _icons ??= new();
            foreach (IAbilityUIData.IconState iconState in Enum.GetValues(typeof(IAbilityUIData.IconState)))
            {
                if (_icons.ContainsKey(iconState) is false)
                {
                    _icons[iconState] = null;
                }
            }
        }

        public AbilityCategory GetCategories()
        {
            return _categoryMask;
        }

        public IAbilityUIData GetAbilityUIData()
        {
            return new AbilityUIData(this);
        }

        public IAbilityCostData GetAbilityCosts(IAbilityContext context)
        {
            return new AbilityCost(context, this);
        }

        public IAbilityTargetingData GetTargetingData(IAbilityContext context)
        {
            return new AbilityTargetingData(context, this);
        }

        protected string GetLocalizedName()
        {
            return _fallbackName;
        }

        protected string GetLocalizedDescription()
        {
            return _fallbackDescription;
        }

        public bool IsContextValidForExecution(IAbilityContext context)
        {
            return IsContextNotNull(context) 
                   && IsTargetableNotNull(context)
                   && GetTargetHasValidFlag(context, GetValidTargetsMask(context))
                   && ValidateAbilityTarget(context);

            static bool IsTargetableNotNull(IAbilityContext context) => context.TargetObject != null;
            static bool GetTargetHasValidFlag(IAbilityContext context, TargetableEntityType flag)
            {
                if (flag.HasFlag(TargetableEntityType.Self) && context.SourceCombatBehaviour == context.TargetObject)
                    return true;
                return context.TargetObject.IsValidTarget(context.SourceCombatBehaviour, flag);
            }
        }

        protected bool IsContextNotNull(IAbilityContext context) => context != null;

        protected virtual AbilityRange GetAbilityRangeType(IAbilityContext context)
        {
            return _targetingDistanceType;
        }

        protected virtual AbilityTargetingType GetAbilityTargetingType(IAbilityContext context)
        {
            return _abilityTargetingType;
        }

        protected virtual int GetActionPointCost(IAbilityContext context)
        {
            return _baseAPCost;
        }

        protected virtual int GetBloodPointCost(IAbilityContext context)
        {
            return _baseBloodCost;
        }

        protected virtual int GetMovementPointCost(IAbilityContext context)
        {
            return _baseMovementCost;
        }

        protected virtual TargetableEntityType GetValidTargetsMask(IAbilityContext context)
        {
            return _validTargetsMask;
        }

        protected abstract bool ValidateAbilityTarget(IAbilityContext context);
        public abstract IAbilityExecutionStrategy GetExecutionStrategy();
        public abstract IAbilityTargetingStrategy GetTargetingStrategy();
        protected abstract RangeData GetAbilityRangeData(IAbilityContext context);

        protected virtual string GetAPCostText(IAbilityContext context)
        {
            if (GetActionPointCost(context) <= 0)
                return string.Empty;
            return $"AP: {GetActionPointCost(context)}";
        }
        protected virtual string GetBloodCostText(IAbilityContext context)
        {
            if (GetBloodPointCost(context) <= 0)
                return string.Empty;
            return $"Blood: {GetBloodPointCost(context)}";
        }

        private readonly struct AbilityCost : IAbilityCostData
        {
            public int MovementCost { get; }
            public int ActionPointCost { get; }
            public int BloodCost { get; }

            public AbilityCost(IAbilityContext context, AbstractAbilityConfig ability)
            {
                MovementCost = ability.GetMovementPointCost(context);
                ActionPointCost = ability.GetActionPointCost(context);
                BloodCost = ability.GetBloodPointCost(context);
            }
        }

        private readonly struct AbilityTargetingData : IAbilityTargetingData
        {
            private readonly AbilityTargetingType _targetingType;
            private readonly RangeData _rangeData;
            private readonly AbilityRange _rangeType;
            private readonly TargetableEntityType _validTargetsFlag;

            public AbilityTargetingData(IAbilityContext context, AbstractAbilityConfig ability)
            {
                _targetingType = ability.GetAbilityTargetingType(context);
                _rangeData = ability.GetAbilityRangeData(context);
                _rangeType = ability.GetAbilityRangeType(context);
                _validTargetsFlag = ability.GetValidTargetsMask(context);
            }

            public AbilityTargetingType GetAbilityTargetingType()
            {
                return _targetingType;
            }

            public RangeData GetRangeData()
            {
                return _rangeData;
            }

            public AbilityRange GetRangeType()
            {
                return _rangeType;
            }

            public TargetableEntityType GetValidEntityTypesFlag()
            {
                return _validTargetsFlag;
            }
        }

        private readonly struct AbilityUIData : IAbilityUIData
        {
            private readonly AbstractAbilityConfig _ability;
            public readonly Dictionary<IAbilityUIData.IconState, Sprite> _icons;
            public readonly Func<string> _localizedName;
            public readonly Func<string> _localizedDescription;

            public AbilityUIData(AbstractAbilityConfig ability)
            {
                _ability = ability;
                _icons = new(ability._icons);
                _localizedName = ability.GetLocalizedName;
                _localizedDescription = ability.GetLocalizedDescription;
            }

            public Sprite GetAbilityIcon(IAbilityUIData.IconState iconState)
            {
                return _icons.TryGetValue(iconState, out var icon) ? icon : null;
            }

            public string GetLocalizedName()
            {
                return _localizedName.Invoke();
            }

            public string GetLocalizedDescription()
            {
                return _localizedDescription.Invoke();
            }

            public string GetAPCostText(IAbilityContext context)
            {
                return _ability.GetAPCostText(context);
            }

            public string GetBloodCostText(IAbilityContext context)
            {
                return _ability.GetBloodCostText(context);
            }
        }
    }
}
