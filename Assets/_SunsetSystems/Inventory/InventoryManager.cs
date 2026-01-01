using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using SunsetSystems.Core.Database;
using SunsetSystems.Data;
using SunsetSystems.DynamicLog;
using SunsetSystems.Inventory.Data;
using SunsetSystems.Persistence;
using UnityEngine;

namespace SunsetSystems.Inventory
{
    [RequireComponent(typeof(ItemStorage))]
    public class InventoryManager : SerializedMonoBehaviour, ISaveable
    {
        public static InventoryManager Instance { get; private set; }

        [SerializeField]
        private ItemStorage _playerInventory;
        [SerializeField]
        private float _money;

        public string DataKey => DataKeyConstants.INVENTORY_MANAGER_DATA_KEY;

        public delegate void MoneyEvent(float currentMoney, float valueChange);
        public static event Action<IBaseItem> OnItemAcquired, OnItemLost;
        public static event MoneyEvent OnMoneyGained, OnMoneyLost;

        public void ResetOnGameStart()
        {
            _playerInventory.Contents.Clear();
            _money = 0;
        }

        protected void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
            if (!_playerInventory)
                _playerInventory = GetComponent<ItemStorage>();
            if (!_playerInventory)
                _playerInventory = gameObject.AddComponent<ItemStorage>();
            ISaveable.RegisterSaveable(this);
        }

        private void OnDestroy()
        {
            ISaveable.UnregisterSaveable(this);
        }

        public bool GiveItemToPlayer(IBaseItem item, int amount = 1, bool postLogMessage = true)
        {
            if (amount <= 0)
            {
                Debug.LogError("Cannot add less than 1 item to player inventory!");
                return false;
            }
            else if (_playerInventory.AddItem(new(item, amount)))
            {
                OnItemAcquired?.Invoke(item);
                if (postLogMessage)
                    DynamicLogManager.Instance.PostLogMessage(LogUtility.LogMessageFromItemPickup(item));
                return true;
            }
            return false;
        }

        public void GiveItemToPlayer(BaseItem item, int amount) => GiveItemToPlayer(item, amount, true);

        public bool TakeItemFromPlayer(IBaseItem item, int amount = 1, bool postLogMessage = true)
        {
            if (amount <= 0)
            {
                Debug.LogError("Cannot take less than 1 item from player inventory!");
                return false;
            }
            else if (_playerInventory.TryRemoveItem(new(item, amount)))
            {
                OnItemLost?.Invoke(item);
                if (postLogMessage)
                DynamicLogManager.Instance.PostLogMessage(LogUtility.LogMessageFromItemLoss(item));
                return true;
            }
            return false;
        }

        public bool GetInventoryContainsItemWithReadableID(string itemID, out int count)
        {
            count = 0;
            var items = _playerInventory.Contents.FindAll(entry => entry.ItemReference.ReadableID == itemID);
            if (items != null && items.Count() > 0)
            {
                count = items.Count();
                return true;
            }
            else
            {
                return false;
            }
        }

        public IEnumerable<InventoryEntry> GetPlayerInventoryContents() => _playerInventory.Contents;

        public void TransferItem(ItemStorage from, ItemStorage to, InventoryEntry item)
        {
            if (from.TryRemoveItem(item))
            {
                to.AddItem(item);
            }
        }

        public void AddMoney(float value)
        {
            _money += value;
            OnMoneyGained?.Invoke(_money, value);
        }

        public bool TryRemoveMoney(float value)
        {
            if (value > _money)
                return false;
            _money -= value;
            OnMoneyLost?.Invoke(_money, value);
            return true;
        }

        public float GetMoneyAmount()
        {
            return _money;
        }

        public void SetMoney(float value)
        {
            _money = value;
        }

        public object GetSaveData()
        {
            return new InventorySaveData(this);
        }

        public bool InjectSaveData(object data)
        {
            if (data is not InventorySaveData saveData)
                return false;
            if (saveData.InventoryContents != null)
            {
                foreach (var itemData in saveData.InventoryContents)
                {
                    if (string.IsNullOrWhiteSpace(itemData.ItemID))
                        continue;
                    if (ItemDatabase.Instance.TryGetEntry(itemData.ItemID, out var item))
                    {
                        _playerInventory.AddItem(new InventoryEntry(item, itemData.StackSize));
                    }
                }
            }
            SetMoney(saveData.Money);
            return true;
        }
    }

    [Serializable]
    public class InventorySaveData
    {
        public List<InventoryContentData> InventoryContents;
        public float Money;

        public InventorySaveData(InventoryManager manager) : this()
        {
            foreach (var itemEntry in manager.GetPlayerInventoryContents())
            {
                if (itemEntry.ItemReference == null)
                {
                    Debug.LogError("Player has a null item in their inventory!");
                    continue;
                }
                InventoryContents.Add(new() { ItemID = itemEntry.ItemReference.DatabaseID, StackSize = itemEntry.StackSize });
            }
            Money = manager.GetMoneyAmount();
        }

        public InventorySaveData()
        {
            InventoryContents = new();
        }

        [Serializable]
        public struct InventoryContentData
        {
            public string ItemID;
            public int StackSize;
        }
    }
}
