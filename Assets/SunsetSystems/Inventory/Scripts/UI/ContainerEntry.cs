using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SunsetSystems.Inventory.UI
{
    [RequireComponent(typeof(Button))]
    internal class ContainerEntry : MonoBehaviour
    {
        [SerializeField]
        private Image _icon;
        [SerializeField]
        private TextMeshProUGUI _text;
        [SerializeField]
        private InventoryManager _inventoryManager;
        private InventoryEntry _content;
        private ItemStorage _storage;

        private void Start()
        {
            if (this.TryFindFirstWithTag(TagConstants.INVENTORY_MANAGER, out GameObject inventoryManagerGO))
            {
                inventoryManagerGO.TryGetComponent(out _inventoryManager);
            }
        }

        public void SetEntryContent(InventoryEntry content, ItemStorage storage)
        {
            _content = content;
            _storage = storage;
            _text.text = content._item.ItemName;
            _icon.sprite = content._item.Icon;
        }

        public void OnClick()
        {
            _inventoryManager.TransferItem(_storage, _inventoryManager.PlayerInventory, _content);
            Destroy(gameObject);
        }
    }
}
