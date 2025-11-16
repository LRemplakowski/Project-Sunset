using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using SunsetSystems.Combat;
using SunsetSystems.Dice;
using SunsetSystems.Entities.Data;
using SunsetSystems.Party;
using UltEvents;
using UnityEngine;

namespace SunsetSystems.Entities.Characters
{
    public class StatsManager : SerializedMonoBehaviour
    {
        [SerializeField, HideInInspector]
        private Creature _owner;
        private Creature Owner
        {
            get
            {
                if (_owner == null)
                    _owner = GetComponentInParent<Creature>();
                return _owner;
            }
        }

        [Title("Events")]
        public UltEvent<ICreature> OnCreatureDied = new();
        public UltEvent<ICreature> OnCreatureRevived = new();
        [field: Title("Debug")]
        [field: SerializeField]
        public StatsData Stats { get; private set; }

        public Tracker Health => Stats.Trackers.GetTracker(TrackerType.Health);
        public Tracker Willpower => Stats.Trackers.GetTracker(TrackerType.Willpower);
        public Tracker Hunger => Stats.Trackers.GetTracker(TrackerType.Hunger);
        public Tracker Humanity => Stats.Trackers.GetTracker(TrackerType.Humanity);

        private void OnValidate()
        {
            if (_owner == null)
                _owner = GetComponentInParent<Creature>();
        }

        private void Start()
        {
            CombatManager.OnCombatEnd += OnCombatEnd;
        }

        private void OnDestroy()
        {
            CombatManager.OnCombatEnd -= OnCombatEnd;
        }

        private void OnCombatEnd(IEnumerable<ICombatant> combatants)
        {
            if (PartyManager.Instance.ActiveParty.Contains(Owner) && IsDead())
            {
                Heal(1);
            }
        }

        [Button]
        public void LoadFromConfig(StatsConfig config)
        {
            Stats = new(config);
        }

        [Button]
        public void TakeDamage(int damage)
        {
            Health.SuperficialDamage += damage;
            if (Health.GetValue() <= 0)
                Die();
        }

        [Button]
        public bool TryUseBlood(int amount)
        {
            if (amount > Hunger.GetValue())
                return false;
            Hunger.SuperficialDamage += amount;
            return true;
        }

        [Button]
        public void RegainBlood(int amount)
        {
            if (amount > Hunger.SuperficialDamage)
                Hunger.SuperficialDamage = 0;
            else
                Hunger.SuperficialDamage -= amount;
        }

        public virtual void Die()
        {
            Health.SuperficialDamage = Health.MaxValue;
            OnCreatureDied?.InvokeSafe(Owner);
        }

        public void Heal(int amount)
        {
            bool wasDead = IsDead();
            int currentDamage = Health.SuperficialDamage;
            currentDamage -= amount;
            currentDamage = currentDamage < 0 ? 0 : currentDamage;
            Health.SuperficialDamage = currentDamage;
            if (wasDead && IsAlive())
            {
                OnCreatureRevived?.InvokeSafe(Owner);
            }
        }

        public int GetCombatSpeed()
        {
            return Stats.Attributes.GetAttribute(AttributeType.Speed).GetValue();
            //return 5;
        }

        public int GetInitiative()
        {
            return Stats.Attributes.GetAttribute(AttributeType.Dexterity).GetValue();
        }

        public bool IsAlive()
        {
            return Health.GetValue() > 0;
        }

        public bool IsDead() => !IsAlive();

        public void CopyFromTemplate(ICreatureTemplate template)
        {
            Stats = new(template.StatsData);
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode is false)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        public Outcome GetSkillRoll(AttributeType attribute, SkillType skill, bool useHunger = false)
        {
            CreatureAttribute a = Stats.Attributes.GetAttribute(attribute);
            Skill s = Stats.Skills.GetSkill(skill);
            int normalDice = a.GetValue() + s.GetValue();
            int hungerDice = 0;
            if (useHunger)
            {
                hungerDice = Hunger.GetValue();
                normalDice = hungerDice <= normalDice ? normalDice - hungerDice : 0;
            }

            return Roll.d10(normalDice, hungerDice);
        }

        public Outcome GetSkillRoll(AttributeType attribute, SkillType skill, int dc, bool useHunger = false)
        {
            CreatureAttribute a = Stats.Attributes.GetAttribute(attribute);
            Skill s = Stats.Skills.GetSkill(skill);
            int normalDice = a.GetValue() + s.GetValue();
            int hungerDice = 0;
            if (useHunger)
            {
                hungerDice = Hunger.GetValue();
                normalDice = hungerDice <= normalDice ? normalDice - hungerDice : 0;
            }

            return Roll.d10(normalDice, hungerDice, dc);
        }

        public List<CreatureAttribute> GetAttributes()
        {
            return Stats.Attributes.GetAttributeList();
        }

        public CreatureAttribute GetAttribute(AttributeType attributeType)
        {
            foreach (CreatureAttribute attribute in GetAttributes())
            {
                if (attributeType == attribute.AttributeType)
                    return attribute;
            }
            return null;
        }

        public List<Skill> GetSkills()
        {
            return Stats.Skills.GetSkillList();
        }

        public Skill GetSkill(SkillType skillType)
        {
            foreach (Skill skill in GetSkills())
            {
                if (skillType == skill.SkillType)
                    return skill;
            }
            return null;
        }

        public HealthData GetHealthData()
        {
            return new HealthData(Health.MaxValue, Health.SuperficialDamage, Health.AggravatedDamage);
        }
    }

    public readonly struct HealthData
    {
        public readonly int maxHealth, superficialDamage, aggravatedDamage;

        public HealthData(int maxHealth, int superficialDamage, int aggravatedDamage)
        {
            this.maxHealth = maxHealth;
            this.superficialDamage = superficialDamage;
            this.aggravatedDamage = aggravatedDamage;
        }
    }
}
