using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using SunsetSystems.ActionSystem;
using SunsetSystems.Core;
using SunsetSystems.Data;
using SunsetSystems.Entities.Characters;
using SunsetSystems.Entities.Creatures;
using SunsetSystems.LevelUtility;
using SunsetSystems.Persistence;
using UltEvents;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SunsetSystems.Party
{
    public class PartyManager : SerializedMonoBehaviour, ISaveable, IResetable
    {
        public static PartyManager Instance { get; private set; }

        [ShowInInspector, ReadOnly, Title("Persistence")]
        public string DataKey => DataKeyConstants.PARTY_MANAGER_DATA_KEY;

        [Title("References")]
        [SerializeField]
        private Transform _creatureParent;

        [Title("Config")]
        [SerializeField, ValueDropdown("GetLayerNames")]
        private string _defaultPartyLayer;
        [SerializeField]
        private bool _forceSpawnPartyInStorage = false;

        [Title("Runtime")]
        [SerializeField]
        private string _mainCharacterKey;
        [SerializeField]
        private Dictionary<string, ICreature> _activeParty = new();
        public ICreature MainCharacter => _activeParty.TryGetValue(Instance._mainCharacterKey, out ICreature creature) ? creature : null;
        public string MainCharacterKey => _mainCharacterKey;
        public List<ICreature> ActiveParty => _activeParty.Values.ToList();
        public List<ICreature> Companions => _activeParty.Where(kv => kv.Key != Instance._mainCharacterKey).Select(kv => kv.Value).ToList();
        [SerializeField]
        private HashSet<string> _activeCoterieMemberKeys = new();
        [SerializeField]
        private HashSet<string> _coterieMemberKeysCache = new();
        [SerializeField]
        private Dictionary<string, ICreatureTemplate> _cachedPartyTemplates = new();

        private Dictionary<string, Vector3> _partyPositions = null;
        public List<string> AllCoterieMembers => _coterieMemberKeysCache.ToList();

        [Title("Events")]
        public UltEvent<IEnumerable<ICreature>> OnActivePartyInitialized = new();
        public UltEvent<string> OnPartyMemberRecruited = new();

        private bool _initializeAtSavedPositions = false;

        public bool IsInitialized { get; private set; } = false;

        private string[] GetLayerNames()
        {
            return Enumerable.Range(0, 31).Select(index => LayerMask.LayerToName(index)).Where(layerName => !string.IsNullOrEmpty(layerName)).ToArray();
        }

        public void ResetOnGameStart()
        {
            _activeParty = new();
            _coterieMemberKeysCache = new();
            _mainCharacterKey = "";
            _activeCoterieMemberKeys = new();
        }

        protected void Awake()
        {
            Instance = this;
            IsInitialized = false;
            ISaveable.RegisterSaveable(this);
            _activeCoterieMemberKeys.AddRange(_activeParty.Keys);
        }

        protected void OnDestroy()
        {
            ISaveable.UnregisterSaveable(this);
        }

        public void StopTheParty()
        {
            foreach (var member in ActiveParty)
            {
                member.PeekCurrentAction.Abort();
            }
        }

        public void OrderMoveMainCharacter(Transform destination)
        {
            MainCharacter.PerformAction(new Move(MainCharacter, destination.position));
        }

        public ICreature GetPartyMemberByID(string key)
        {
            return _activeParty[key];
        }

        public bool IsRecruitedMember(string key)
        {
            return _coterieMemberKeysCache.Contains(key);
        }

        public async void StartPlayerParty()
        {
            if (string.IsNullOrWhiteSpace(_mainCharacterKey))
                return;
            var waypoint = WaypointManager.Instance.GetSceneDefaultEntryWaypoint();
            if (waypoint != null && !_forceSpawnPartyInStorage)
            {
                await InitializePartyAtWaypoint(waypoint);
            }
            else
            {
                await InitializePartyInCreatureStorage();
            }
            IsInitialized = true;
        }

        private async Awaitable InitializePartyInCreatureStorage()
        {
            foreach (string key in Instance._activeCoterieMemberKeys)
            {
                if (_cachedPartyTemplates.TryGetValue(key, out ICreatureTemplate template))
                {
                    if (_activeParty.ContainsKey(key))
                    {
                        continue;
                    }
                    _activeParty.Add(key, await InitializePartyMemberInCreatureStorage(template));
                    if (_initializeAtSavedPositions && _partyPositions.TryGetValue(key, out Vector3 savedPosition) && _activeParty.TryGetValue(key, out ICreature creature))
                    {
                        creature.ForceToPosition(savedPosition, Quaternion.identity);
                    }
                }
            }
            OnActivePartyInitialized?.InvokeSafe(_activeParty.Values.ToList());
        }

        private async Awaitable InitializePartyAtWaypoint(IWaypoint waypoint)
        {
            var positions = waypoint.GetPositions(_activeCoterieMemberKeys.Count);
            int positionIndex = 0;
            ICreature spawnedPartyMember;
            Vector3 spawnPosition;
            foreach (string key in _activeCoterieMemberKeys)
            {
                if (_activeParty.ContainsKey(key))
                {
                    continue;
                }
                if (_cachedPartyTemplates.TryGetValue(key, out ICreatureTemplate template))
                {
                    if (_initializeAtSavedPositions && _partyPositions.TryGetValue(key, out Vector3 savedPosition))
                    {
                        spawnPosition = savedPosition;
                        spawnedPartyMember = await InitializePartyMemberAtPosition(template, spawnPosition);
                    }
                    else
                    {
                        spawnPosition = positions[positionIndex];
                        spawnedPartyMember = await InitializePartyMemberAtPosition(template, spawnPosition);
                        spawnedPartyMember.FacePointInSpace(waypoint.GetFacingDirectionFromPosition(in spawnPosition));
                    }
                    _activeParty.Add(key, spawnedPartyMember);
                    positionIndex++;
                    await Awaitable.NextFrameAsync();
                }
                else
                {
                    Debug.LogError($"{nameof(PartyManager)} >>> Party Member {key} is active, but has no template!");
                }
            }
            OnActivePartyInitialized?.InvokeSafe(_activeParty.Values.ToList());
        }

        private async Awaitable<ICreature> InitializePartyMemberAtPosition(ICreatureTemplate data, Vector3 position)
        {
            if (data == null)
            {
                throw new NullReferenceException($"{nameof(PartyManager)} >>> Party member initialization failed! Null CreatureData!");
            }
            else
            {
                ICreature memberInstance = await CreatureFactory.Instance.Create(data, position, Quaternion.identity, _creatureParent);
                memberInstance.References.GameObject.layer = LayerMask.NameToLayer(_defaultPartyLayer);
                return memberInstance;
            }
        }

        private async Awaitable<ICreature> InitializePartyMemberInCreatureStorage(ICreatureTemplate data)
        {
            if (data == null)
            {
                throw new NullReferenceException($"{nameof(PartyManager)} >>> Party member initialization failed! Null CreatureData!");
            }
            else
            {
                ICreature memberInstance = await CreatureFactory.Instance.Create(data, _creatureParent);
                memberInstance.References.GameObject.layer = LayerMask.NameToLayer(_defaultPartyLayer);
                return memberInstance;
            }
        }

        public void UpdatePartyMemberTemplateFromInstance(ICreature memberInstance)
        {
            _cachedPartyTemplates[memberInstance.ID] = memberInstance.CreatureTemplate;
        }

        [Title("Editor Utility")]
        [Button]
        public void RecruitCharacter(ICreatureTemplate memberTemplate)
        {
            Debug.Log($"{nameof(PartyManager)} >>> Recruited {memberTemplate.FullName} to party!");
            _coterieMemberKeysCache.Add(memberTemplate.DatabaseID);
            _cachedPartyTemplates.Add(memberTemplate.DatabaseID, memberTemplate);
            OnPartyMemberRecruited?.InvokeSafe(memberTemplate.DatabaseID);
        }

        [Button]
        public void RecruitCharacter(ICreature creature)
        {
            Debug.Log($"{nameof(PartyManager)} >>> Recruited {creature.References.CreatureData.FullName} to party!");
            var memberTemplate = creature.CreatureTemplate;
            _coterieMemberKeysCache.Add(creature.References.CreatureData.DatabaseID);
            _cachedPartyTemplates.Add(memberTemplate.DatabaseID, memberTemplate);
            TryAddMemberToActiveRoster(creature.References.CreatureData.DatabaseID, creature);
            OnPartyMemberRecruited?.InvokeSafe(creature.References.CreatureData.DatabaseID);
        }

        [Button]
        public void RecruitMainCharacter(ICreatureTemplateProvider mainCharTemplate)
        {
            var template = mainCharTemplate.CreatureTemplate;
            _mainCharacterKey = template.DatabaseID;
            RecruitCharacter(template);
            _activeCoterieMemberKeys.Add(_mainCharacterKey);
        }

        public bool TryAddMemberToActiveRoster(string memberID, ICreature creature)
        {
            _activeCoterieMemberKeys.Add(memberID);
            _activeParty.Add(memberID, creature);
            return true;
        }

        public bool TryRemoveMemberFromActiveRoster(string memberID)
        {
            if (memberID.Equals(_mainCharacterKey))
            {
                Debug.LogError($"{nameof(PartyManager)} >>> Cannot remove Main Character from active roster!");
                return false;
            }
            bool result = _activeCoterieMemberKeys.Remove(memberID);
            if (_activeParty.TryGetValue(memberID, out ICreature memberInstance))
            {
                UpdatePartyMemberTemplateFromInstance(memberInstance);
                CreatureFactory.Instance.DestroyCreature(memberInstance);
            }
            return result;
        }

        public bool TryRemoveMemberFromActiveRoster(ICreature partyMember)
        {
            return TryRemoveMemberFromActiveRoster(partyMember.References.CreatureData.DatabaseID);
        }

        public bool TryRemoveMemberFromActiveRoster(CreatureConfig memberTemplate)
        {
            return TryRemoveMemberFromActiveRoster(memberTemplate.DatabaseID);
        }

        public object GetSaveData()
        {
            _cachedPartyTemplates = UpdatePartyTemplates();
            PartySaveData saveData = new()
            {
                CachedPartyTemplates = new(_cachedPartyTemplates),
                ActiveMemberKeys = new(_activeCoterieMemberKeys),
                MainCharacterKey = _mainCharacterKey
            };
            Dictionary<string, Vector3> partyPositions = new();
            foreach (string key in _activeParty.Keys)
            {
                partyPositions.Add(key, _activeParty[key].References.Transform.position);
            }
            saveData.PartyPositions = partyPositions;
            saveData.PartyPositionsScene = SceneManager.GetActiveScene().name;
            return saveData;

            Dictionary<string, ICreatureTemplate> UpdatePartyTemplates()
            {
                Dictionary<string, ICreatureTemplate> templates = new();
                foreach (string key in _cachedPartyTemplates.Keys)
                {
                    if (_activeParty.TryGetValue(key, out ICreature creature))
                    {
                        templates[key] = creature.CreatureTemplate;
                    }
                    else
                    {
                        templates[key] = _cachedPartyTemplates[key];
                    }
                }
                return templates;
            }
        }

        public bool InjectSaveData(object data)
        {
            if (data is not PartySaveData saveData)
                return false;
            _coterieMemberKeysCache = new();
            _cachedPartyTemplates = new();
            _activeParty = new();
            _partyPositions = new();
            saveData.CachedPartyTemplates.Keys.ToList().ForEach(key => _cachedPartyTemplates.Add(key, saveData.CachedPartyTemplates[key]));
            _coterieMemberKeysCache.AddRange(_cachedPartyTemplates.Keys);
            _activeCoterieMemberKeys = new(saveData.ActiveMemberKeys);
            _mainCharacterKey = saveData.MainCharacterKey;

            foreach (string key in _activeCoterieMemberKeys)
            {
                if (saveData.PartyPositions.TryGetValue(key, out Vector3 position))
                {
                    _partyPositions.Add(key, position);
                }
            }
            _initializeAtSavedPositions = saveData.PartyPositionsScene == gameObject.scene.name;
            return true;
        }
    }

    [Serializable]
    public class PartySaveData
    {
        public Dictionary<string, Vector3> PartyPositions;
        public Dictionary<string, ICreatureTemplate> CachedPartyTemplates;
        public List<string> ActiveMemberKeys;
        public string MainCharacterKey;
        public string PartyPositionsScene;
    }
}
