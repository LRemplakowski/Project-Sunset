using SunsetSystems.Inventory.Data;
using SunsetSystems.Management;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SunsetSystems.Inventory.UI
{
    public class InventoryGUI : MonoBehaviour
    {
        [SerializeField]
        private InventoryItemDisplay _displayPrefab;
        [SerializeField]
        private GameObject _itemListContentParent;


        public void AddItems(List<BaseItem> items)
        {
            items.ForEach(item => AddItem(item));
        }

        public void AddItem(BaseItem item)
        {
            InventoryItemDisplay itemDisplay = Instantiate(_displayPrefab, _itemListContentParent.transform);
            itemDisplay.item = item;
        }

        public void ClearItemList()
        {
            _itemListContentParent.transform.DestroyChildren();
        }

        private void OnEnable()
        {
            AddItems(References.Get<InventoryManager>().PlayerInventory.Contents as List<BaseItem>);
        }

        private void OnDisable()
        {
            ClearItemList();
        }
    }
}
