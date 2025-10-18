using System;
using System.Linq;
using SunsetSystems.Core.Database;
using SunsetSystems.Dice;
using SunsetSystems.Entities.Characters;
using SunsetSystems.Inventory;
using SunsetSystems.Journal;
using SunsetSystems.Party;
using UnityEngine;
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
            if (CreatureDatabase.Instance.TryGetEntryByReadableID(readableID, out var config))
                return PartyManager.Instance.IsRecruitedMember(config.DatabaseID);
            else
                return false;
        }

        [YarnFunction("GetCurrentMoney")]
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
            AttributeType attributeType = GetAttributeTypeFromString(statName);
            if (attributeType != AttributeType.Invalid)
            {
                return PartyManager.Instance.MainCharacter.References.StatsManager.GetAttribute(attributeType)?.GetValue() ?? 0;
            }
            SkillType skillType = GetSkillTypeFromString(statName);
            if (skillType != SkillType.Invalid)
            {
                return PartyManager.Instance.MainCharacter.References.StatsManager.GetSkill(skillType)?.GetValue() ?? 0;
            }
            return 1;
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

        [YarnFunction("IsObjectiveActive")]
        public static bool GetIsObjectiveActive(string questID, string objectiveID)
        {
            return QuestJournal.Instance.TryGetTrackedObjectiveByReadableID(questID, objectiveID, out _);
        }

        [YarnFunction("GetCompanionInfluence")]
        public static int GetCompanionInfluence(string companionID)
        {
            return RelationshipManager.Instance.GetInfluence(companionID);
        }

        [YarnFunction ("GetBloodPoints")]
        public static int GetBloodPoints(string characterID)
        {
            if (CreatureDatabase.Instance.TryGetEntryByReadableID(characterID, out var config))
            {
                var partyMember = PartyManager.Instance.GetPartyMemberByID(config.DatabaseID);
                if (partyMember != null)
                {
                    return partyMember.References.StatsManager.Hunger.GetValue();
                }
            }
            return 0;
        }

        [YarnFunction ("GetHasItem")]
        public static bool GetHasItem(string itemID)
        {
            IsItemEquipped(itemID);
            return IsItemInInventory(itemID) || IsItemEquipped(itemID);

            static bool IsItemEquipped(string itemID)
            {
                return PartyManager.Instance.ActiveParty.Select(partyMember => partyMember.References.EquipmentManager).Any(eq => eq.IsItemEquipped(itemID));
            }

            static bool IsItemInInventory(string itemID)
            {
                return InventoryManager.Instance.GetInventoryContainsItemWithReadableID(itemID, out _);
            }
        }

        [YarnFunction("GetIsCompanionInParty")]
        public static bool GetIsCompanionInParty(string characterID)
        {
            foreach (var partyMember in PartyManager.Instance.ActiveParty)
            {
                Debug.Log($"Comparing party member IDs! {partyMember.References.CreatureData.ReadableID} == {characterID} ? {partyMember.References.CreatureData.ReadableID == characterID}");
                if (partyMember.References.CreatureData.ReadableID == characterID)
                    return true;
            }
            return false;
        }

        [YarnFunction("GetFirstName")]
        public static string GetFirstName(string characterID)
        {
            if (CreatureDatabase.Instance.TryGetEntryByReadableID(characterID, out CreatureConfig creatureAsset))
                return creatureAsset.FirstName;
            return "";
        }

        [YarnFunction("GetLastName")]
        public static string GetLastName(string characterID)
        {
            if (CreatureDatabase.Instance.TryGetEntryByReadableID(characterID, out CreatureConfig creatureAsset))
                return creatureAsset.LastName;
            return "";
        }

        [YarnFunction("GetCharacterDisciplineRank")]
        public static int GetCharacterDisciplineRank(string characterID, string disciplineID)
        {
            Debug.LogError("DialogueFunctions >>> GetCharacterDisciplineRank is not implemented!");
            return 0;
        }

        [YarnFunction("GetPartyHasDiscipline")]
        public static bool GetParyHasDiscipline(string disciplineID)
        {
            Debug.LogError("DialogueFunctions >>> GetPartyHasDiscipline is not implemented!");
            return true;
        }

        [YarnFunction("CustomVisited")]
        public static bool CustomVisited(string nodeID)
        {
            var variableStorage = DialogueHelper.VariableStorage;
            if (variableStorage != null && variableStorage.TryGetValue($"visited:{nodeID}", out bool visited))
                return visited;
            return false;
        }
    }
}
