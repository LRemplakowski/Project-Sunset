using NaughtyAttributes;
using SunsetSystems.Journal;
using SunsetSystems.Resources;
using SunsetSystems.UI.Utils;
using UnityEditor;
using UnityEngine;

namespace SunsetSystems.Inventory.Data
{
    public abstract class BaseItem : ScriptableObject, IRewardable, IGameDataProvider<BaseItem>
    {
        [field: SerializeField]
        public string ReadableID { get; protected set; }
        [field: SerializeField, ReadOnly]
        public string DatabaseID { get; private set; }
        [field: SerializeField, ReadOnly]
        public ItemCategory ItemCategory { get; protected set; }
        [field: SerializeField, TextArea]
        public string ItemDescription { get; protected set; }
        [field: SerializeField]
        public GameObject Prefab { get; protected set; }
        [field: SerializeField]
        public Sprite Icon { get; protected set; }
        [field: SerializeField]
        public bool Stackable { get; protected set; }

        public BaseItem Data => this;

        private void OnEnable()
        {
#if UNITY_EDITOR
            if (string.IsNullOrWhiteSpace(ReadableID))
            {
                ReadableID = name;
                EditorUtility.SetDirty(this);
            }
            if (Icon == null)
            {
                Icon = ResourceLoader.GetFallbackIcon();
                EditorUtility.SetDirty(this);
            }
            if (string.IsNullOrWhiteSpace(DatabaseID))
                AssignNewID();
            ItemDatabase.Instance?.Register(this);
#endif
        }

        private void Reset()
        {
            ReadableID = name;
            Icon = ResourceLoader.GetFallbackIcon();
            AssignNewID();
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR
            ItemDatabase.Instance?.Unregister(this);
#endif
        }

        private void AssignNewID()
        {
            DatabaseID = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public void ApplyReward(int amount)
        {
            InventoryManager.PlayerInventory.AddItem(new InventoryEntry(this, amount));
        }
    }
}
