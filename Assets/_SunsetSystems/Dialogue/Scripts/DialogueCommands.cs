using SunsetSystems.Audio;
using SunsetSystems.Core.Database;
using SunsetSystems.Entities.Characters;
using SunsetSystems.Inventory;
using SunsetSystems.Inventory.Data;
using SunsetSystems.Journal;
using SunsetSystems.Party;
using UnityEngine;
using Yarn.Unity;

namespace SunsetSystems.Dialogue
{
    public static class DialogueCommands
    {
        [YarnCommand("StartQuest")]
        public static void StartQuest(string readableID)
        {
            QuestJournal.Instance.BeginQuestByReadableID(readableID);
        }

        [YarnCommand("CompleteObjective")]
        public static void CompleteObjective(string readableQuestID, string objectiveID)
        {
            if (QuestJournal.Instance.TryGetTrackedObjectiveByReadableID(readableQuestID, objectiveID, out Objective objective))
            {
                objective.Complete();
            }
            else
            {
                Debug.LogError($"Dialogue tried to completed objective {objectiveID} in quest {readableQuestID} but that objective is not currently active!");
            }
        }

        [YarnCommand("FailObjective")]
        public static void FailObjective(string readableQuestID, string objectiveID)
        {
            if (QuestJournal.Instance.TryGetTrackedObjectiveByReadableID(readableQuestID, objectiveID, out Objective objective))
            {
                objective.Fail();
            }
            else
            {
                Debug.LogError($"Dialogue tried to fail objective {objectiveID} in quest {readableQuestID} but that objective is not currently active!");
            }
        }

        [YarnCommand("IncreaseHunger")]
        public static void IncreaseHunger(string characterID, int value)
        {
            ICreature character = PartyManager.Instance.GetPartyMemberByID(characterID);
            character.References.StatsManager.TryUseBlood(value);
        }

        [YarnCommand("DecreaseHunger")]
        public static void DecreaseHunger(string characterID, int value)
        {
            ICreature character = PartyManager.Instance.GetPartyMemberByID(characterID);
            character.References.StatsManager.RegainBlood(value);
        }

        [YarnCommand("AddMoney")]
        public static void AddMoney(float value)
        {
            InventoryManager.Instance.AddMoney(value);
        }

        [YarnCommand("ModifyInfluence")]
        public static void ModifyInfluence(int amount, string companionID)
        {
            RelationshipManager.Instance.ModifyInfluence(companionID, amount);
        }

        [YarnCommand("ModifyInfluenceNoLog")]
        public static void ModifyInfluenceNoLog(int amount, string companionID)
        {
            RelationshipManager.Instance.ModifyInfluence(companionID, amount, false);
        }

        [YarnCommand("RemoveMoney")]
        public static void RemoveMoney(float value)
        {
            if (InventoryManager.Instance.TryRemoveMoney(value) == false)
                Debug.LogError($"Money amount {value} is greater than current funds! Check money amount before removing money!");
        }

        [YarnCommand("PlaySFX")]
        public static void PlaySFX(string clipName)
        {
            AudioManager.Instance.PlaySFXOneShot(clipName);
        }

        [YarnCommand("GiveItem")]
        public static void GiveItem(string itemID)
        {
            if (ItemDatabase.Instance.TryGetEntryByReadableID(itemID, out IBaseItem item))
                InventoryManager.Instance.GiveItemToPlayer(item);
            else
                Debug.LogError($"Dialogue command GiveItem: Could not find item {itemID} in the ItemDatabase instance!");
        }

        [YarnCommand("GiveItemCount")]
        public static void GiveItemCount(string itemID, int count)
        {
            if (count <= 0)
            {
                Debug.LogError("Item count cannot be less than 1!");
                return;
            }
            if (ItemDatabase.Instance.TryGetEntryByReadableID(itemID, out IBaseItem item))
                InventoryManager.Instance.GiveItemToPlayer(item, count);
            else
                Debug.LogError($"Dialogue command GiveItemCount: Could not find item {itemID} in the ItemDatabase instance!");
        }

        [YarnCommand("RemoveItem")]
        public static void RemoveItem(string itemID)
        {

            if (ItemDatabase.Instance.TryGetEntryByReadableID(itemID, out var item))
            {
                InventoryManager.Instance.TakeItemFromPlayer(item, 1);
            }

        }

        [YarnCommand("DealDamage")]
        public static void DealDamage(string characterID, int damage)
        {
            if (CreatureDatabase.Instance.TryGetEntry(characterID, out var config))
            {
                var partyMember = PartyManager.Instance.GetPartyMemberByID(config.DatabaseID);
                if (partyMember != null)
                {
                    partyMember.References.StatsManager.TakeDamage(damage);
                }
            }
        }

        [YarnCommand("DecreaseWillpower")]
        public static void DecreaseWillpower(string characterID, int value)
        {
            if (CreatureDatabase.Instance.TryGetEntry(characterID, out var config))
            {
                var partyMember = PartyManager.Instance.GetPartyMemberByID(config.DatabaseID);
                if (partyMember != null)
                {
                    partyMember.References.StatsManager.Willpower.SuperficialDamage += value;
                }
            }
        }

        [YarnCommand("RemovePartyMember")]
        public static void RemovePartyMember(string characterID)
        {
            if (CreatureDatabase.Instance.TryGetEntry(characterID, out var config))
            {
                PartyManager.Instance.TryRemoveMemberFromActiveRoster(config);
            }
        }
    }
}
