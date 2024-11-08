using Sirenix.OdinInspector;
using SunsetSystems.Inventory;
using SunsetSystems.Inventory.Data;
using SunsetSystems.Party;
using SunsetSystems.UI;
using SunsetSystems.UI.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SunsetSystems.Equipment.UI
{
    public class EquipmentSlotDisplay : SerializedMonoBehaviour, IUserInterfaceView<IEquipmentSlot>
    {
        [SerializeField, ReadOnly]
        private IEquipmentSlot _cachedSlotData;
        [SerializeField]
        private Image _itemIcon;

        public void UpdateView(IUserInfertaceDataProvider<IEquipmentSlot> dataProvider)
        {
            _cachedSlotData = dataProvider.UIData;
            IEquipableItem itemInSlot = _cachedSlotData.GetEquippedItem();
            if (itemInSlot != null)
            {
                _itemIcon.sprite = itemInSlot.Icon;
                _itemIcon.gameObject.SetActive(true);
            }
            else
            {
                _itemIcon.gameObject.SetActive(false);
            }
        }

        public void UnequipItemFromSlot()
        {
            if (CanUnequipItem(_cachedSlotData))
            {
                var currentCharacter = CharacterSelector.SelectedCharacterKey;
                var eqManager = PartyManager.Instance.GetPartyMemberByID(currentCharacter).References.EquipmentManager;
                if (eqManager.UnequipItem(_cachedSlotData.ID, out var unequipped))
                    InventoryManager.Instance.GiveItemToPlayer(unequipped, postLogMessage: false);
            }
        }

        private static bool CanUnequipItem(IEquipmentSlot slot)
        {
            if (slot == null)
                return false;
            var item = slot.GetEquippedItem();
            return item != null && item.CanBeRemoved && !item.IsDefaultItem;
        }
    }
}
