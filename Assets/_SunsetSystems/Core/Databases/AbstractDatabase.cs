using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace SunsetSystems.Core.Database
{
    public abstract class AbstractDatabase<T> : SerializedScriptableObject, IDatabase<T> where T : IDatabaseEntry<T>
    {
        [SerializeField]
        protected Dictionary<string, T> _databaseRegistry = new();
        [SerializeField]
        protected Dictionary<string, string> _readableIDRegistry = new();

#if UNITY_EDITOR
        private void Awake()
        {

            if (GetEditorInstance() == null)
                SetEditorInstance(this);
        }

        protected abstract AbstractDatabase<T> GetEditorInstance();
        protected abstract void SetEditorInstance(AbstractDatabase<T> instance);

        protected virtual void OnValidate()
        {
            List<string> keysToDelete = new();
            foreach (string key in _databaseRegistry.Keys)
            {
                if (_databaseRegistry[key] == null)
                    keysToDelete.Add(key);
            }
            keysToDelete.ForEach(key => _databaseRegistry.Remove(key));
            _readableIDRegistry = new();
            _databaseRegistry.Values.ToList().ForEach(baseItem => _readableIDRegistry.TryAdd(baseItem.ReadableID, baseItem.DatabaseID));
        }

        [Button]
        public void FindAllItems()
        {
            UnityEditor.EditorUtility.SetDirty(this);
            _databaseRegistry = new();
            _readableIDRegistry = new();
            string[] assetPaths = UnityEditor.AssetDatabase.FindAssets("t:ScriptableObject");
            foreach (string guid in assetPaths)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var item = UnityEditor.AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                if (item is T typedItem)
                    Register(typedItem);
            }
        }
#endif

        public bool TryGetEntry(string itemID, out T entry)
        {
            entry = default;
            if (string.IsNullOrWhiteSpace(itemID))
            {
                Debug.LogError($"{this} >>> TryGetEntry recieved null or empty itemID!");
                return false;
            }

            return _databaseRegistry.TryGetValue(itemID, out entry);
        }

        public bool TryGetEntryByReadableID(string readableID, out T entry)
        {
            entry = default;
            if (string.IsNullOrWhiteSpace(readableID))
            {
                Debug.LogError($"Requested entry from a database, but ReadableID is null!");
                return false;
            }
            if (_readableIDRegistry.TryGetValue(readableID, out string key))
                return TryGetEntry(key, out entry);
            return false;
        }

        public bool Register(T entry)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            _databaseRegistry ??= new();
            _readableIDRegistry ??= new();
            bool result = false;
            result = entry != null && _databaseRegistry.TryAdd(entry.DatabaseID, entry);
            _readableIDRegistry = new();
            _databaseRegistry.Values.Where(item => item != null).ForEach(item => _readableIDRegistry.TryAdd(item.ReadableID, item.DatabaseID));
            return result;
        }

        public void Unregister(T entry)
        {
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
            if (_databaseRegistry.Remove(entry.DatabaseID))
            {
                _readableIDRegistry = new();
                _databaseRegistry.Values.ToList().ForEach(item => _readableIDRegistry.TryAdd(item.ReadableID, item.DatabaseID));
            }
        }
    }
}
