using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using SunsetSystems.Core.Database;
using SunsetSystems.Equipment;
using SunsetSystems.Inventory.Data;
using UltEvents;
using UnityEngine;

namespace SunsetSystems.Entities.Characters
{
    public class EquipmentManager : SerializedMonoBehaviour, IEquipmentManager
    {
        public event Action<IEquipableItem> OnItemEquipped
        {
            add => ItemEquipped.DynamicCalls += value;
            remove => ItemEquipped.DynamicCalls -= value;
        }
        public event Action<IEquipableItem> OnItemUnequipped
        {
            add => ItemUnequipped.DynamicCalls += value;
            remove => ItemUnequipped.DynamicCalls -= value;
        }

        [field: Title("Data")]
        [field: OdinSerialize, DictionaryDrawerSettings(IsReadOnly = true)]
        public Dictionary<EquipmentSlotID, IEquipmentSlot> EquipmentSlots { get; private set; }
        public IEnumerable<IBaseItem> EquippedItems => EquipmentSlots.Values.Select(slot => slot.GetEquippedItem()).ToList();

        [Title("Events")]
        public UltEvent<IEquipableItem> ItemEquipped;
        public UltEvent<IEquipableItem> ItemUnequipped;

        private void Start()
        {
            foreach (var item in EquippedItems)
            {
                if (item == null || item is not IEquipableItem equipable)
                    continue;
                ItemEquipped?.InvokeSafe(equipable);
            }
        }

        private void OnValidate()
        {
            if (EquipmentSlots == null || EquipmentSlots.Count < Enum.GetValues(typeof(EquipmentSlotID)).Length-1)
            {
                EquipmentSlots = new()
                {
                    { EquipmentSlotID.PrimaryWeapon, new EquipmentSlot(EquipmentSlotID.PrimaryWeapon) },
                    { EquipmentSlotID.SecondaryWeapon, new EquipmentSlot(EquipmentSlotID.SecondaryWeapon) },
                    { EquipmentSlotID.Chest, new EquipmentSlot(EquipmentSlotID.Chest) },
                    { EquipmentSlotID.Boots, new EquipmentSlot(EquipmentSlotID.Boots) },
                    { EquipmentSlotID.Hands, new EquipmentSlot(EquipmentSlotID.Hands) },
                    { EquipmentSlotID.Trinket, new EquipmentSlot(EquipmentSlotID.Trinket) }
                };
            }
        }

        public EquipmentSlotID GetSlotForItem(IEquipableItem item)
        {
            if (item.ItemCategory == ItemCategory.WEAPON)
            {
                if (EquipmentSlots[EquipmentSlotID.PrimaryWeapon].GetEquippedItem().IsDefaultItem)
                    return EquipmentSlotID.PrimaryWeapon;
                if (EquipmentSlots[EquipmentSlotID.SecondaryWeapon].GetEquippedItem().IsDefaultItem)
                    return EquipmentSlotID.SecondaryWeapon;
            }
            foreach (var slotID in EquipmentSlots.Keys)
            {
                if (ValidateItem(slotID, item) && EquipmentSlots[slotID].GetEquippedItem().CanBeRemoved)
                    return slotID;
            }
            return EquipmentSlotID.Invalid;
        }

        public bool EquipItem(EquipmentSlotID slotID, IEquipableItem item, out IEquipableItem previouslyEquipped)
        {
            previouslyEquipped = default;
            if (ValidateItem(slotID, item))
            {
                if (EquipmentSlots.TryGetValue(slotID, out IEquipmentSlot slot) && slot.GetEquippedItem().CanBeRemoved)
                {
                    if (slot.TryEquipItem(item, out previouslyEquipped))
                    {
                        if (previouslyEquipped != null && previouslyEquipped.ReadableID != slot.DefaultItemID)
                            ItemUnequipped?.InvokeSafe(previouslyEquipped);
                        ItemEquipped?.InvokeSafe(item);
                        return true;
                    }
                }
            }
            return false;
        }

        public bool UnequipItem(EquipmentSlotID slotID, out IEquipableItem unequipped)
        {
            unequipped = default;
            if (EquipmentSlots.TryGetValue(slotID, out IEquipmentSlot slot) && slot.TryUnequipItem(out unequipped))
            {
                ItemUnequipped?.InvokeSafe(unequipped);
                ItemEquipped?.InvokeSafe(slot.GetEquippedItem());
                return true;
            }
            return false;
        }

        public bool IsItemEquipped(IEquipableItem item)
        {
            return EquipmentSlots.Values.Any(slot => slot.GetEquippedItem().DatabaseID == item.DatabaseID);
        }

        public bool IsItemEquipped(string readableID)
        {
            return EquipmentSlots.Values.Any(slot => slot.GetEquippedItem().ReadableID == readableID);
        }

        private bool ValidateItem(EquipmentSlotID slotID, IEquipableItem item)
        {
            if (EquipmentSlots.TryGetValue(slotID, out IEquipmentSlot slot))
            {
                return slot.AcceptedCategory == item.ItemCategory;
            }
            return false;
        }

        public void CopyFromTemplate(ICreatureTemplate template)
        {
            if (template == null || template.EquipmentSlotsData == null)
            {
                EquipmentSlots = InitializeEquipmentSlots();
                return;
            }
            EquipmentSlots ??= InitializeEquipmentSlots();
            var itemDB = ItemDatabase.Instance;
            foreach (EquipmentSlotID key in template.EquipmentSlotsData.Keys)
            {
                if (template.EquipmentSlotsData.TryGetValue(key, out var templateSlot) && EquipmentSlots.TryGetValue(key, out var mySlot))
                {
                    if (itemDB.TryGetEntryByReadableID(templateSlot, out var item) && item is IEquipableItem equipable && mySlot.TryEquipItem(equipable, out var _))
                    {
                        Debug.Log($"Injected item {mySlot.GetEquippedItem()} into slot {mySlot.ID}! Creature: {transform.parent.parent.name}");
                        ItemEquipped?.InvokeSafe(EquipmentSlots[key].GetEquippedItem());
                    }
                    else
                    {
                        Debug.LogError($"Could not inject item {item} into slot {key} of creature {transform.parent.parent.name}!");
                    }
                }
            }
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode is false)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }

        private Dictionary<EquipmentSlotID, IEquipmentSlot> InitializeEquipmentSlots()
        {
            return new()
                {
                    { EquipmentSlotID.PrimaryWeapon, new EquipmentSlot(EquipmentSlotID.PrimaryWeapon) },
                    { EquipmentSlotID.SecondaryWeapon, new EquipmentSlot(EquipmentSlotID.SecondaryWeapon) },
                    { EquipmentSlotID.Chest, new EquipmentSlot(EquipmentSlotID.Chest) },
                    { EquipmentSlotID.Boots, new EquipmentSlot(EquipmentSlotID.Boots) },
                    { EquipmentSlotID.Hands, new EquipmentSlot(EquipmentSlotID.Hands) },
                    { EquipmentSlotID.Trinket, new EquipmentSlot(EquipmentSlotID.Trinket) }
                };
        }
    }
}
