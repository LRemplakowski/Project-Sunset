using System;
using System.Collections.Generic;
using SunsetSystems.Entities.Characters;
using SunsetSystems.Inventory.Data;

namespace SunsetSystems.Equipment
{
    public interface IEquipmentManager
    {
        event Action<IEquipableItem> OnItemEquipped;
        event Action<IEquipableItem> OnItemUnequipped;

        Dictionary<EquipmentSlotID, IEquipmentSlot> EquipmentSlots { get; }
        IEnumerable<IBaseItem> EquippedItems { get; }
        EquipmentSlotID GetSlotForItem(IEquipableItem item);
        bool EquipItem(EquipmentSlotID slotID, IEquipableItem item, out IEquipableItem previouslyEquipped);
        bool UnequipItem(EquipmentSlotID slotID, out IEquipableItem unequipped);
        bool IsItemEquipped(IEquipableItem item);
        bool IsItemEquipped(string readableID);
        void CopyFromTemplate(ICreatureTemplate template);
    }
}
