﻿using SunsetSystems.Entities.Characters;
using UI.CharacterPortraits;
using UnityEngine;
using SunsetSystems.Utils;
using System.Collections.Generic;
using SunsetSystems.Loading;
using System;
using System.Linq;

namespace SunsetSystems.Party
{
    public class PartyManager : InitializedSingleton<PartyManager>, ISaveRuntimeData
    {
        [field: SerializeField]
        private SerializableStringCreatureInstanceDictionary _activeParty;
        public static Creature MainCharacter => Instance._activeParty[_mainCharacterKey];
        public static List<Creature> ActiveParty => Instance._activeParty.Values.ToList();
        public static List<Creature> Companions => Instance._activeParty.Where(kv => kv.Key != _mainCharacterKey).Select(kv => kv.Value) as List<Creature>;
        private static HashSet<string> _activeCoterieMemberKeys = new();
        [SerializeField]
        private SerializableStringCreatureDataDictionary _creatureDataCache;

        private static string _mainCharacterKey;

        private PartyPortraitsController _partyPortraits;
        private PartyPortraitsController PartyPortraits
        {
            get
            {
                if (!_partyPortraits)
                    _partyPortraits = this.FindFirstComponentWithTag<PartyPortraitsController>(TagConstants.PARTY_PORTRAITS_CONTROLLER);
                return _partyPortraits;
            }
        }

        public override void Initialize()
        {
            PartyPortraits.Clear();
            Debug.Log("Party members count: " + _activeParty.Count);
            foreach (string key in _activeCoterieMemberKeys)
            {
                PartyPortraits.AddPortrait(_activeParty[key].GetCreatureUIData());
            }
        }

        public static void InitializePartyAtPosition(Vector3 position)
        {
            foreach (string key in _activeCoterieMemberKeys)
            {
                CreatureData data = Instance._creatureDataCache[key];
                Instance._activeParty.Add(key, InitializePartyMember(data, position));
            }
        }

        public static void InitializePartyAtPositions(List<Vector3> positions)
        {
            int index = 0;
            foreach (string key in _activeCoterieMemberKeys)
            {
                CreatureData data = Instance._creatureDataCache[key];
                Vector3 position = positions[index];
                Instance._activeParty.Add(key, InitializePartyMember(data, position));
                index++;
            }
        }

        protected static Creature InitializePartyMember(CreatureData data, Vector3 position)
        {
            return CreatureInitializer.InitializeCreature(data, position);
        }

        public static void RecruitCharacter(CreatureData creatureData)
        {
            Instance._creatureDataCache.Add(creatureData.FullName, creatureData);
        }

        public static void RecruitMainCharacter(CreatureData mainCharacterData)
        {
            RecruitCharacter(mainCharacterData);
            _mainCharacterKey = mainCharacterData.FullName;
            if (TryAddMemberToActiveRoster(_mainCharacterKey) == false)
                Debug.LogError("Trying to recruit Main Character but Main Character already exists!");
        }

        public static bool TryAddMemberToActiveRoster(string memberName)
        {
            if (Instance._creatureDataCache.ContainsKey(memberName) == false)
                Debug.LogError("Trying to add character to roster but character " + memberName + " is not yet recruited!");
            return _activeCoterieMemberKeys.Add(memberName);
        }

        public static bool TryRemoveMemberFromActiveRoster(string memberName)
        {
            if (memberName.Equals(_mainCharacterKey))
            {
                Debug.LogError("Cannot remove Main Character from active roster!");
                return false;
            }
            return _activeCoterieMemberKeys.Remove(memberName);
        }

        public static void UpdateActivePartyData()
        {
            foreach (string key in _activeCoterieMemberKeys)
            {
                Instance._activeParty[key].Data = Instance._creatureDataCache[key];
            }
        }

        public static bool UpdateCreatureData(CreatureData data)
        {
            if (_activeCoterieMemberKeys.Contains(data.FullName))
            {
                Instance._creatureDataCache[data.FullName] = data;
                return true;
            }
            else
            {
                Debug.LogWarning("No cached party member with name " + data.FullName + " found!");
                return false;
            }
        }

        public void SaveRuntimeData()
        {
            throw new System.NotImplementedException();
        }

        public void LoadRuntimeData()
        {
            throw new System.NotImplementedException();
        }
    }

    [Serializable]
    public class SerializableStringCreatureDataDictionary : SerializableDictionary<string, CreatureData>
    {

    }

    [Serializable]
    public class SerializableStringCreatureInstanceDictionary : SerializableDictionary<string, Creature>
    {

    }
}
