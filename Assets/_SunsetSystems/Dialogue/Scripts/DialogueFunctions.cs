using SunsetSystems.Dice;
using SunsetSystems.Inventory;
using SunsetSystems.Journal;
using SunsetSystems.Party;
using System;
using System.Collections.Generic;
using Yarn.Unity;

namespace SunsetSystems.Dialogue
{
    public static class DialogueFunctions
    {
        [YarnFunction("RollSingle")]
        public static int GetRollResultSingle(string statName)
        {
            int dice = 0;
            dice += GetStatValueFromString(statName);
            Outcome rollOutcome = Roll.d10(dice);
            return rollOutcome.successes;
        }

        [YarnFunction("Roll")]
        public static int GetRollResult(string attributeName, string skillName)
        {
            int dice = 0;
            dice += GetStatValueFromString(attributeName);
            dice += GetStatValueFromString(skillName);
            Outcome rollOutcome = Roll.d10(dice);
            return rollOutcome.successes;
        }

        [YarnFunction("GetAttributeSkillPoolSize")]
        public static int GetDicePoolSize(string attributeName, string skillName)
        {
            int size = 0;
            size += GetStatValueFromString(attributeName);
            size += GetStatValueFromString(skillName);
            return size;
        }

        [YarnFunction("UseDiscipline")]
        public static int GetUseDisciplineResult(string disciplineName)
        {
            int dice = 0;
            dice += GetStatValueFromString(disciplineName);
            Outcome rollOutcome = Roll.d10(dice);
            return rollOutcome.successes;
        }

        [YarnFunction("GetIsPartyMemberRecruited")]
        public static bool GetIsPartyMemberRecruited(string readableID)
        {
            return PartyManager.Instance.IsRecruitedMember(readableID);
        }

        [YarnFunction("CurrentMoney")]
        public static float GetCurrentMoney()
        {
            return InventoryManager.Instance.GetMoneyAmount();
        }

        [YarnFunction("GetIDFromName")]
        public static string GetIDFromName(string name)
        {
            if (DialogueHelper.VariableStorage.TryGetValue(name, out string id))
            {
                return id;
            }
            return "";
        }

        private static int GetStatValueFromString(string statName)
        {
            //AttributeType attributeType = GetAttributeTypeFromString(statName);
            //if (attributeType != AttributeType.Invalid)
            //{
            //    return PartyManager.MainCharacter.Data.Stats.Attributes.GetAttribute(attributeType).GetValue();
            //}
            //SkillType skillType = GetSkillTypeFromString(statName);
            //if (skillType != SkillType.Invalid)
            //{
            //    return PartyManager.MainCharacter.Data.Stats.Skills.GetSkill(skillType).GetValue();
            //}
            //return 1;
            throw new NotImplementedException();
        }

        private static AttributeType GetAttributeTypeFromString(string attributeTypeString)
        {
            if (Enum.TryParse(attributeTypeString, true, out AttributeType result))
            {
                return result;
            }
            return AttributeType.Invalid;
        }

        private static SkillType GetSkillTypeFromString(string skillTypeString)
        {
            if (Enum.TryParse(skillTypeString, true, out SkillType result))
            {
                return result;
            }
            return SkillType.Invalid;
        }

        [YarnFunction("neg")]
        public static int NegativeValue(int value)
        {
            return -value;
        }

        [YarnFunction("CurrentObjective")]
        public static bool GetIsObjectiveActive(string questID, string objectiveID)
        {
            return QuestJournal.Instance.TryGetTrackedObjectiveByReadableID(questID, objectiveID, out _);
        }

        [YarnFunction("GetCompanionInfluence")]
        public static int GetCompanionInfluence(string companionID)
        {
            throw new NotImplementedException();
        }

        [YarnFunction ("GetBloodPoints")]
        public static int GetBloodPoints(string characterID)
        {
            throw new NotImplementedException();
        }
    }
}
