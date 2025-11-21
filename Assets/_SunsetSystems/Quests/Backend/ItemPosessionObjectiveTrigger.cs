using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using SunsetSystems.Inventory;
using SunsetSystems.Inventory.Data;
using SunsetSystems.Party;
using SunsetSystems.Persistence;
using UnityEngine;

namespace SunsetSystems.Journal
{
    [RequireComponent(typeof(PersistentSceneObject))]
    public class ItemPosessionObjectiveTrigger : SerializedMonoBehaviour, IPersistentComponent
    {
        [SerializeField, Required]
        private Objective objectiveToTrigger;
        [OdinSerialize, Required, HideReferenceObjectPicker]
        private List<ItemObjectiveData> requiredItems = new();

        public string ComponentID => "ITEM_POSESSION_TRIGGER";

        private bool _triggered = false;

        private void Start()
        {
            if (!_triggered)
            {
                Objective.OnObjectiveActive += StartHandlingObjective;
                Objective.OnObjectiveFailed += StopHandlingObjective;
                Objective.OnObjectiveCompleted += StopHandlingObjective;
                QuestJournal.OnObjectiveDataInjected += HandleObjectiveDataInjected;
            }
        }

        private void HandleObjectiveDataInjected(HashSet<Objective> objectives)
        {
            if (objectives.Contains(objectiveToTrigger))
            {
                StartHandlingObjective(objectiveToTrigger);
            }
        }

        private void StartHandlingObjective(Objective obj)
        {
            if (obj.DatabaseID != objectiveToTrigger.DatabaseID)
                return;
            bool hasRequiredItems = false;
            if (requiredItems.Count > 0)
                hasRequiredItems = true;
            foreach (ItemObjectiveData itemData in requiredItems)
            {
                hasRequiredItems &= itemData.HasRequiredItems();
            }
            if (hasRequiredItems)
            {
                objectiveToTrigger.Complete();
                _triggered = true;
            }
            else
            {
                InventoryManager.OnItemAcquired += OnItemAddedToPlayerInventory;
            }
        }

        private void OnItemAddedToPlayerInventory(IBaseItem item)
        {
            if (requiredItems.Any(requiredItemEntry => requiredItemEntry.RequiresItem(item)) is false)
                return;
            bool hasRequiredItems = false;
            if (requiredItems.Count > 0)
                hasRequiredItems = true;
            foreach (ItemObjectiveData itemData in requiredItems)
            {
                hasRequiredItems &= itemData.HasRequiredItems();
                if (hasRequiredItems is false)
                    break;
            }
            if (hasRequiredItems)
            {
                objectiveToTrigger.Complete();
                _triggered = true;
            }
        }

        private void StopHandlingObjective(Objective obj)
        {
            if (obj.DatabaseID != objectiveToTrigger.DatabaseID)
                return;
            Objective.OnObjectiveActive -= StartHandlingObjective;
            Objective.OnObjectiveFailed -= StopHandlingObjective;
            Objective.OnObjectiveCompleted -= StopHandlingObjective;
            InventoryManager.OnItemAcquired -= OnItemAddedToPlayerInventory;
        }

        public object GetComponentPersistenceData()
        {
            return _triggered;
        }

        public void InjectComponentPersistenceData(object data)
        {
            if (data is bool boolData)
            {
                _triggered = boolData;
                if (_triggered)
                {
                    Objective.OnObjectiveActive -= StartHandlingObjective;
                    Objective.OnObjectiveFailed -= StopHandlingObjective;
                    Objective.OnObjectiveCompleted -= StopHandlingObjective;
                    InventoryManager.OnItemAcquired -= OnItemAddedToPlayerInventory;
                }
            }
        }

        [Serializable]
        private class ItemObjectiveData
        {
            [OdinSerialize, Required]
            private List<IBaseItem> requiredItems = new();
            [SerializeField, Min(1)]
            private int requiredCount = 1;
            [SerializeField]
            private ItemCountLogic countingLogic;

            public IEnumerable<IBaseItem> GetRequiredItems()
            {
                return requiredItems.AsEnumerable();
            }

            public bool RequiresItem(IBaseItem item)
            {
                return GetRequiredItems().Any(required => required.DatabaseID == item.DatabaseID);
            }

            public bool HasRequiredItems()
            {
                int itemCount = 0;
                foreach (IBaseItem item in requiredItems)
                {
                    bool hasRequiredItems;
                    if (InventoryManager.Instance.GetInventoryContainsItemWithReadableID(item.ReadableID, out int oneTypeCount))
                    {
                        switch (countingLogic)
                        {
                            case ItemCountLogic.CountOneType:
                                hasRequiredItems = oneTypeCount >= requiredCount;
                                if (hasRequiredItems)
                                    return true;
                                break;
                            case ItemCountLogic.CountAllTypes:
                                itemCount += oneTypeCount;
                                hasRequiredItems = itemCount >= requiredCount;
                                if (hasRequiredItems)
                                    return true;
                                break;
                        }
                    }
                    else if (item is IEquipableItem equipable)
                    {
                        var equipments = PartyManager.Instance.ActiveParty.Select(member => member.References.EquipmentManager);
                        oneTypeCount = 0;
                        foreach (var eq in equipments)
                        {
                            if (eq.IsItemEquipped(equipable))
                            {
                                switch (countingLogic)
                                {
                                    case ItemCountLogic.CountOneType:
                                        oneTypeCount += 1;
                                        hasRequiredItems = oneTypeCount >= requiredCount;
                                        if (hasRequiredItems)
                                            return true;
                                        break;
                                    case ItemCountLogic.CountAllTypes:
                                        itemCount += 1;
                                        hasRequiredItems = itemCount >= requiredCount;
                                        if (hasRequiredItems)
                                            return true;
                                        break;
                                }
                            }
                        }
                    }
                }
                return false;
            }

            private enum ItemCountLogic
            {
                CountOneType, CountAllTypes
            }
        }
    }
}
