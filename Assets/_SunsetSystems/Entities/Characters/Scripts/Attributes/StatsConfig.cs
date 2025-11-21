using System;
using System.Collections.Generic;
using UnityEngine;

namespace SunsetSystems.Entities.Data
{
    [CreateAssetMenu(fileName = "New Stats", menuName = "Character/Stats")]
    public class StatsConfig : ScriptableObject
    {
        public Trackers Trackers = Trackers.Initialize();
        public Clan Clan = Clan.Invalid;
        [Range(1, 16)]
        public int Generation = 12;
        public int BloodPotency = 1;
        public Attributes Attributes = Attributes.Initialize();
        public Skills Skills = Skills.Initialize();  
    }

    [Serializable]
    public class Trackers
    {
        [SerializeField]
        private Tracker health, willpower, hunger, humanity;

        public static Trackers Initialize()
        {
            Trackers result = new()
            {
                health = new(TrackerType.Health),
                willpower = new(TrackerType.Willpower),
                hunger = new(TrackerType.Hunger),
                humanity = new(TrackerType.Humanity)
            };
            result.hunger.SetValue(0);
            return result;
        }

        public static Trackers DeepCopy(Trackers existing)
        {
            if (existing == null)
                return Initialize();
            Trackers result = new()
            {
                health = new(existing.health),
                willpower = new(existing.willpower),
                hunger = new(existing.hunger),
                humanity = new(existing.humanity)
            };
            return result;
        }

        public Tracker GetTracker(TrackerType type)
        {
            return type switch
            {
                TrackerType.Health => health,
                TrackerType.Willpower => willpower,
                TrackerType.Humanity => humanity,
                TrackerType.Hunger => hunger,
                _ => new Tracker(TrackerType.Invalid),
            };
        }
    }

    [Serializable]
    public class Attributes
    {
        [SerializeField]
        private CreatureAttribute
            //PHYSICAL
            strength,
            dexterity,
            stamina,
            //SOCIAL
            charisma,
            manipulation,
            composure,
            //MENTAL
            intelligence,
            wits,
            resolve,
            speed;

        public static Attributes Initialize()
        {
            Attributes result = new()
            {
                //PHYSICAL
                strength = new CreatureAttribute(AttributeType.Strength),
                dexterity = new CreatureAttribute(AttributeType.Dexterity),
                stamina = new CreatureAttribute(AttributeType.Stamina),
                //SOCIAL
                charisma = new CreatureAttribute(AttributeType.Charisma),
                manipulation = new CreatureAttribute(AttributeType.Manipulation),
                composure = new CreatureAttribute(AttributeType.Composure),
                //MENTAL
                intelligence = new CreatureAttribute(AttributeType.Intelligence),
                wits = new CreatureAttribute(AttributeType.Wits),
                resolve = new CreatureAttribute(AttributeType.Resolve),
                speed = new CreatureAttribute(AttributeType.Speed)
            };
            result.speed.SetValue(5);
            return result;
        }

        public static Attributes DeepCopy(Attributes existing)
        {
            if (existing == null)
                return Initialize();
            Attributes result = new()
            {
                strength = new(existing.strength),
                dexterity = new(existing.dexterity),
                stamina = new(existing.stamina),
                charisma = new(existing.charisma),
                manipulation = new(existing.manipulation),
                composure = new(existing.composure),
                intelligence = new(existing.intelligence),
                wits = new(existing.wits),
                resolve = new(existing.resolve),
                speed = new(existing.speed)
            };
            return result;
        }

        public List<CreatureAttribute> GetAttributeList()
        {
            return new List<CreatureAttribute>()
            {
                strength, dexterity, stamina,
                charisma, manipulation, composure,
                intelligence, wits, resolve, speed
            };
        }

        public CreatureAttribute GetAttribute(AttributeType attributeType)
        {
            return attributeType switch
            {
                AttributeType.Strength => strength,
                AttributeType.Dexterity => dexterity,
                AttributeType.Stamina => stamina,
                AttributeType.Charisma => charisma,
                AttributeType.Manipulation => manipulation,
                AttributeType.Composure => composure,
                AttributeType.Intelligence => intelligence,
                AttributeType.Wits => wits,
                AttributeType.Resolve => resolve,
                AttributeType.Speed => speed,
                _ => new(AttributeType.Invalid),
            };
        }
    }

    [Serializable]
    public class Skills
    {
        [SerializeField]
        private Skill
            //PHYSICAL
            athletics,
            brawl,
            craft,
            drive,
            firearms,
            larceny,
            melee,
            stealth,
            survival,
            //SOCIAL
            animalKen,
            etiquette,
            insight,
            intimidation,
            leadership,
            performance,
            persuasion,
            streetwise,
            subterfuge,
            //MENTAL
            academics,
            awarness,
            finance,
            investigation,
            medicine,
            occult,
            politics,
            science,
            technology;

        public static Skills Initialize()
        {
            Skills result = new()
            {
                //PHYSICAL
                athletics = new Skill(SkillType.Athletics),
                brawl = new Skill(SkillType.Brawl),
                craft = new Skill(SkillType.Craft),
                drive = new Skill(SkillType.Drive),
                firearms = new Skill(SkillType.Firearms),
                larceny = new Skill(SkillType.Larceny),
                melee = new Skill(SkillType.Melee),
                stealth = new Skill(SkillType.Stealth),
                survival = new Skill(SkillType.Survival),
                //SOCIAL
                animalKen = new Skill(SkillType.AnimalKen),
                etiquette = new Skill(SkillType.Etiquette),
                insight = new Skill(SkillType.Insight),
                intimidation = new Skill(SkillType.Intimidation),
                leadership = new Skill(SkillType.Leadership),
                performance = new Skill(SkillType.Performance),
                persuasion = new Skill(SkillType.Persuasion),
                streetwise = new Skill(SkillType.Streetwise),
                subterfuge = new Skill(SkillType.Subterfuge),
                //MENTAL
                academics = new Skill(SkillType.Academics),
                awarness = new Skill(SkillType.Awareness),
                finance = new Skill(SkillType.Finance),
                investigation = new Skill(SkillType.Investigation),
                medicine = new Skill(SkillType.Medicine),
                occult = new Skill(SkillType.Occult),
                politics = new Skill(SkillType.Politics),
                science = new Skill(SkillType.Science),
                technology = new Skill(SkillType.Technology)
            };
            return result;
        }

        public static Skills DeepCopy(Skills existing)
        {
            if (existing == null)
                return Initialize();
            Skills result = new()
            {
                //PHYSICAL
                athletics = new Skill(existing.athletics),
                brawl = new Skill(existing.brawl),
                craft = new Skill(existing.craft),
                drive = new Skill(existing.drive),
                firearms = new Skill(existing.firearms),
                larceny = new Skill(existing.larceny),
                melee = new Skill(existing.melee),
                stealth = new Skill(existing.stealth),
                survival = new Skill(existing.survival),
                //SOCIAL
                animalKen = new Skill(existing.animalKen),
                etiquette = new Skill(existing.etiquette),
                insight = new Skill(existing.insight),
                intimidation = new Skill(existing.intimidation),
                leadership = new Skill(existing.leadership),
                performance = new Skill(existing.performance),
                persuasion = new Skill(existing.persuasion),
                streetwise = new Skill(existing.streetwise),
                subterfuge = new Skill(existing.subterfuge),
                //MENTAL
                academics = new Skill(existing.academics),
                awarness = new Skill(existing.awarness),
                finance = new Skill(existing.finance),
                investigation = new Skill(existing.investigation),
                medicine = new Skill(existing.medicine),
                occult = new Skill(existing.occult),
                politics = new Skill(existing.politics),
                science = new Skill(existing.science),
                technology = new Skill(existing.technology)
            };
            return result;
        }

        public Skill GetSkill(SkillType type)
        {
            return type switch
            {
                SkillType.Athletics => athletics,
                SkillType.Brawl => brawl,
                SkillType.Craft => craft,
                SkillType.Drive => drive,
                SkillType.Firearms => firearms,
                SkillType.Larceny => larceny,
                SkillType.Melee => melee,
                SkillType.Stealth => stealth,
                SkillType.Survival => survival,
                SkillType.AnimalKen => animalKen,
                SkillType.Etiquette => etiquette,
                SkillType.Insight => insight,
                SkillType.Intimidation => intimidation,
                SkillType.Leadership => leadership,
                SkillType.Performance => performance,
                SkillType.Persuasion => persuasion,
                SkillType.Streetwise => streetwise,
                SkillType.Subterfuge => subterfuge,
                SkillType.Academics => academics,
                SkillType.Awareness => awarness,
                SkillType.Finance => finance,
                SkillType.Investigation => investigation,
                SkillType.Medicine => medicine,
                SkillType.Occult => occult,
                SkillType.Politics => politics,
                SkillType.Science => science,
                SkillType.Technology => technology,
                _ => new Skill(SkillType.Invalid),
            };
        }

        public List<Skill> GetSkillList()
        {
            return new List<Skill>()
            {
                athletics, brawl, craft, drive, firearms, larceny, melee, stealth, survival,
                animalKen, etiquette, insight, intimidation, leadership, performance, persuasion, streetwise, subterfuge,
                academics, awarness, finance, investigation, medicine, occult, politics, science, technology
            };
        }
    }
}
