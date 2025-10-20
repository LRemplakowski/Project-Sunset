using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using SunsetSystems.Core.Database;
using SunsetSystems.DynamicLog;
using SunsetSystems.Entities.Characters;
using SunsetSystems.Persistence;
using UnityEngine;

namespace SunsetSystems
{
    public class RelationshipManager : SerializedMonoBehaviour, ISaveable
    {
        public static RelationshipManager Instance { get; private set; }
        public string DataKey => DataKeyConstants.RELATIONSHIP_MANAGER_DATA_KEY;

        [SerializeField, DictionaryDrawerSettings(IsReadOnly = true, KeyLabel = "Creature ID", ValueLabel = "Influence")]
        private Dictionary<string, int> _influenceData = new();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
            ISaveable.RegisterSaveable(this);
        }

        private void OnDestroy()
        {
            ISaveable.UnregisterSaveable(this);
        }

        public int GetInfluence(ICreature creature)
        {
            return GetInfluence(creature.References.CreatureData.ReadableID);
        }

        public int GetInfluence(string readableID)
        {
            if (_influenceData.TryGetValue(readableID, out int influence))
            {
                return influence;
            }
            else
            {
                _influenceData[readableID] = 0;
                return 0;
            }
        }

        public void ModifyInfluence(ICreature creature, int value)
        {
            ModifyInfluence(creature.References.CreatureData.ReadableID, value);
        }

        public void ModifyInfluence(string readableID, int value)
        {
            Debug.Log($"{nameof(RelationshipManager)} >>> Influence modified by {value}! Target: {readableID}");
            if (_influenceData.TryGetValue(readableID, out var stored))
                _influenceData[readableID] = stored + value;
            else
                _influenceData[readableID] = value;
            if (CreatureDatabase.Instance.TryGetEntryByReadableID(readableID, out CreatureConfig entry))
                LogInfluenceModification(entry.FullName, value);
        }

        private void LogInfluenceModification(string characterName, int influenceMod)
        {
            string message;
            if (influenceMod > 0)
                message = $"{characterName}: Gained {influenceMod} influence.";
            else
                message = $"{characterName}: Lost {Mathf.Abs(influenceMod)} influence.";
            DynamicLogManager.Instance.PostLogMessage(message);
        }

        public object GetSaveData()
        {
            return new RelationshipSaveData(this);
        }

        public bool InjectSaveData(object data)
        {
            if (data is not RelationshipSaveData saveData)
                return false;
            _influenceData = saveData.InfluenceData;
            return true;
        }

        [Serializable]
        public class RelationshipSaveData : SaveData
        {
            public Dictionary<string, int> InfluenceData = new();

            public RelationshipSaveData(RelationshipManager dataSource) : base()
            {
                InfluenceData = dataSource._influenceData;
            }

            public RelationshipSaveData() : base()
            {
                InfluenceData = new();
            }
        }
    }
}
