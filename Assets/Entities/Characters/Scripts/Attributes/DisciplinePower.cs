﻿using UnityEngine;

[System.Serializable, CreateAssetMenu(fileName = "New Power", menuName = "Character/Power")]
public class DisciplinePower : ScriptableObject
{
    private const string typeTooltip = "Drzewo dyscyplin do którego należy dyscyplina.";
    private const string levelTooltip = "Minimalny poziom potrzebny do wykupienia dyscypliny. Ma znaczenie tylko dla rozwoju postaci.";
    private const string secondaryTypeTooltip = "Wymagany przy amalgamatach.";
    private const string secondaryLevelTooltip = "Wymagany przy amalgamatach. Używany tylko jeśli Secondary Type nie jest Invalid. Ma znaczenie tylko przy rozwoju postaci.";
    private const string _targetTooltip = "Cel na jaki wpływa dyscyplina.";
    private const string rangeTooltip = "Zasięg w jakim może być użyta dyscyplina.";
    private const string hasDisciplinePoolString = "Czy moc posiada dice pool niezbędny do skutecznej aktywacji.";
    private const string disciplineRollDifficultyTooltip = "ST jakie musi przerzucić użytkownik by skutecznie aktywować dyscyplinę.";
    private const string hasAttackPoolTooltip = "Czy moc posiada odrębną pule kości dla ataku. Ma znaczenie tylko dla mocy o celu innym niż Self.";
    private const string defensePoolTooltip = "Czy moc posiada pulę kości dla broniącego się celu. " 
        + "Ma znaczenie tylko dla mocy o celu innym niż Self. " 
        + "Ta pula kości będzie sporna z Attack Pool lub, jeśli moc go nie posiada, Discipline Pool. " 
        + "Właściwość zostanie zignorowana jeśli dyscyplina nie ma żadnej z tych puli kości.";
    private const string targetableCreatureTypeTooltip = "Rodzaj istot na jakie może wpływać moc. Ma znaczenie tylko dla dyscyplin o celu innym niż Self.";

    [SerializeField]
    private string _scriptName;
    public string ScriptName { get; }
    [SerializeField, Tooltip(typeTooltip)]
    private DisciplineType type = DisciplineType.Invalid;
    public DisciplineType Type { get => type; }
    [SerializeField, Range(1, 5), Tooltip(levelTooltip)]
    private int level = 1;
    public int Level { get => level; }
    [SerializeField, Tooltip(secondaryTypeTooltip)]
    private DisciplineType secondaryType = DisciplineType.Invalid;
    public DisciplineType SecondaryType { get => secondaryType; }
    [SerializeField, Range(0, 5), Tooltip(secondaryLevelTooltip)]
    private int secondaryLevel;
    public int SecondaryLevel { get => secondaryLevel; }

    [SerializeField, Tooltip(_targetTooltip)]
    private Target _target = Target.Self;
    public Target Target { get => _target; }
    [SerializeField, Tooltip(rangeTooltip)]
    private Range range = Range.Ranged;
    public Range Range { get => range; }
    [SerializeField, Tooltip(hasDisciplinePoolString)]
    private bool hasDiciplinePool = false;
    public bool HasDisciplinePool { get => hasDiciplinePool; }
    [SerializeField]
    private DicePool disciplinePool = new DicePool();
    public DicePool DisciplinePool { get => disciplinePool; }
    [SerializeField, Tooltip(disciplineRollDifficultyTooltip)]
    private int disciplineRollDifficulty = 0;
    public int DisciplineRollDifficulty { get => disciplineRollDifficulty; }
    [SerializeField, Tooltip(hasAttackPoolTooltip)]
    private bool hasAttackPool = false;
    public bool HasAttackPool { get => hasAttackPool; }
    [SerializeField]
    private DicePool attackPool = new DicePool();
    public DicePool AttackPool { get => attackPool; }
    [SerializeField, Tooltip(defensePoolTooltip)]
    private bool hasDefensePool = false;
    public bool HasDefensePool { get => hasDefensePool; }
    [SerializeField]
    private DicePool defensePool = new DicePool();
    public DicePool DefensePool { get => defensePool; }
    [SerializeField, Tooltip(targetableCreatureTypeTooltip)]
    private TargetableCreatureType targetableCreatureType = TargetableCreatureType.Any;
    public TargetableCreatureType TargetableCreatureType { get => targetableCreatureType; }

    public EffectWrapper[] GetEffects()
    {
        return effects;
    }

    [SerializeField]
    private EffectWrapper[] effects = new EffectWrapper[1];
    [System.Serializable]
    public class EffectWrapper
    {
        //Editor variables
        [SerializeField]
        private bool isExpanded = false;

        private const string effectTypeTooltip = "Typ efektu. Na jaką właściwość ma wpływać lub czy ma być obsługiwany osobnym skryptem.";
        private const string affectedCreatureTooltip = "Na kogo ma wpłynąć efekt. Nie ma znaczenia jeśli cel mocy jest Self.";

        //Discipline variables
        [SerializeField, Tooltip(effectTypeTooltip)]
        private EffectType effectType;
        public EffectType EffectType { get => effectType; }
        [SerializeField, Tooltip(affectedCreatureTooltip)]
        private AffectedCreature _affectedCreature = AffectedCreature.Self;
        public AffectedCreature AffectedCreature { get => _affectedCreature; }
        [SerializeField]
        private Duration duration = Duration.Immediate;
        public Duration Duration { get => duration; }
        [SerializeField]
        private int rounds = 0;
        public int Rounds { get => rounds; }

        public abstract class Effect { }

        public Effect GetEffect()
        {
            return effectType switch
            {
                EffectType.Attribute => attributeEffect,
                EffectType.Skill => skillEffect,
                EffectType.Discipline => disciplineEffect,
                EffectType.Tracker => trackerEffect,
                EffectType.ScriptDriven => scriptEffect,
                _ => scriptEffect,
            };
        }

        public AttributeEffect attributeEffect = new AttributeEffect();
        [System.Serializable]
        public class AttributeEffect : Effect
        {
            [SerializeField]
            private EffectModifier propertyModifier;
            public EffectModifier PropertyModifier { get => propertyModifier; }
            [SerializeField]
            private AttributeType affectedProperty;
            public AttributeType AffectedProperty { get => affectedProperty; }
            [SerializeField]
            private ModifierType modifierType;
            public ModifierType ModifierType { get => modifierType; }
            [SerializeField]
            private int modifierValue;
            public int ModifierValue { get => modifierValue; }
        }

        public SkillEffect skillEffect = new SkillEffect();
        [System.Serializable]
        public class SkillEffect : Effect
        {
            [SerializeField]
            private EffectModifier propertyModifier;
            public EffectModifier PropertyModifier { get => propertyModifier; }
            [SerializeField]
            private SkillType affectedProperty;
            public SkillType AffectedProperty { get => affectedProperty; }
            [SerializeField]
            private ModifierType modifierType;
            public ModifierType ModifierType { get => modifierType; }
            [SerializeField]
            private int modifierValue;
            public int ModifierValue { get => modifierValue; }
        }

        public DisciplineEffect disciplineEffect = new DisciplineEffect();
        [System.Serializable]
        public class DisciplineEffect : Effect
        {
            [SerializeField]
            private EffectModifier propertyModifier;
            public EffectModifier PropertyModifier { get => propertyModifier; }
            [SerializeField]
            private DisciplineType affectedProperty;
            public DisciplineType AffectedProperty { get => affectedProperty; }
            [SerializeField]
            private ModifierType modifierType;
            public ModifierType ModifierType { get => modifierType; }
            [SerializeField]
            private int modifierValue;
            public int ModifierValue { get => modifierValue; }
        }

        public TrackerEffect trackerEffect = new TrackerEffect();
        [System.Serializable]
        public class TrackerEffect : Effect
        {
            [SerializeField]
            private EffectModifier propertyModifier;
            public EffectModifier PropertyModifier { get => propertyModifier; }
            [SerializeField]
            private TrackerType affectedProperty;
            public TrackerType AffectedProperty { get => affectedProperty; }
            [SerializeField]
            private AffectedValue affectedValue;
            public AffectedValue AffectedValue { get => affectedValue; }
            [SerializeField]
            private ModifierType modifierType;
            public ModifierType ModifierType { get => modifierType; }
            [SerializeField]
            private int modifierValue;
            public int ModifierValue { get => modifierValue; }

        }

        public ScriptEffect scriptEffect = new ScriptEffect();
        [System.Serializable]
        public class ScriptEffect : Effect
        {
            [SerializeField]
            private DisciplineScript script;
            public DisciplineScript Script { get => script; }
        }
    }

    public enum AffectedValue
    {
        MaxValue,
        CurrentValue
    }

    public enum EffectModifier
    {
        StaticValue,
        LevelBased,
        RollBased
    }

    public enum FieldType
    {
        Attribute,
        Skill,
        Discipline
    }

    public enum AffectedCreature
    {
        Self,
        Target
    }

    [System.Serializable]
    public class DicePool
    {
        [SerializeField]
        private FieldType firstPool, secondPool;
        public FieldType FirstPool { get => firstPool; }
        public FieldType SecondPool { get => secondPool; }
        [SerializeField]
        private AttributeType attribute, secondAttribute;
        public AttributeType Attribute { get => attribute; }
        public AttributeType SecondAttribute { get => secondAttribute; }
        [SerializeField]
        private SkillType skill, secondSkill;
        public SkillType Skill { get => skill; }
        public SkillType SecondSkill { get => secondSkill; }
        [SerializeField]
        private DisciplineType discipline, secondDiscipline;
        public DisciplineType Discipline { get => discipline; }
        public DisciplineType SecondDiscipline { get => secondDiscipline; }
    }
}