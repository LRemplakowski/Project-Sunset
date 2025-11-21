using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using SunsetSystems.Entities.Characters;
using SunsetSystems.Entities.Interactable;
using SunsetSystems.Inventory.Data;
using SunsetSystems.Party;
using UltEvents;
using UnityEngine;

namespace SunsetSystems.Journal
{
    public class ItemEquippedObjectiveTrigger : SerializedMonoBehaviour, IObjectiveTrigger
    {
        [SerializeField]
        private CreatureConfig _creatureConfig;
        [SerializeField]
        private Objective _objective;
        [SerializeField]
        private IEquipableItem[] _requiredItems = new IEquipableItem[0];

        private PartyManager _partyManager;
        private ICreature _trackedCreature;
        private bool _allItemsEquipped = false;
        private bool _isActive = false;

        private void Start()
        {
            _partyManager = PartyManager.Instance;
            _partyManager.OnActivePartyInitialized += HandleActivePartyInitialized;
            _partyManager.OnPartyMemberRecruited += HandlePartyMemberRecruited;
        }

        private void Update()
        {
            if (_isActive && CheckCompletion(_objective))
            {
                _isActive = false;
                _objective.Complete();
                gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (_partyManager != null)
            {
                _partyManager.OnActivePartyInitialized += HandleActivePartyInitialized;
                _partyManager.OnPartyMemberRecruited += HandlePartyMemberRecruited;
            }
            if (_trackedCreature != null)
            {
                _trackedCreature.References.EquipmentManager.OnItemEquipped += OnItemEquipped;
            }
            Objective.OnObjectiveActive += HandleObjectiveActive;
            QuestJournal.OnObjectiveDataInjected += HandleObjectiveDataInjected;
        }

        private void OnDisable()
        {
            if (_partyManager != null)
            {
                _partyManager.OnActivePartyInitialized -= HandleActivePartyInitialized;
                _partyManager.OnPartyMemberRecruited -= HandlePartyMemberRecruited;
            }
            if (_trackedCreature != null)
            {
                _trackedCreature.References.EquipmentManager.OnItemEquipped -= OnItemEquipped;
            }
            Objective.OnObjectiveActive -= HandleObjectiveActive;
            QuestJournal.OnObjectiveDataInjected -= HandleObjectiveDataInjected;
        }

        private void OnDestroy()
        {
            if (_partyManager != null)
            {
                _partyManager.OnActivePartyInitialized -= HandleActivePartyInitialized;
                _partyManager.OnPartyMemberRecruited -= HandlePartyMemberRecruited;
            }
            if (_trackedCreature != null)
            {
                _trackedCreature.References.EquipmentManager.OnItemEquipped -= OnItemEquipped;
            }
            _partyManager = null;
            _trackedCreature = null;
        }

        private void HandleObjectiveDataInjected(HashSet<Objective> objectives)
        {
            if (objectives.Contains(_objective))
            {
                HandleObjectiveActive(_objective);
            }
        }

        private void HandleObjectiveActive(Objective objective)
        {
            if (objective.DatabaseID == _objective.DatabaseID)
            {
                _isActive = true;
            }
        }

        private void HandleActivePartyInitialized(IEnumerable<ICreature> activeParty)
        {
            _trackedCreature = activeParty.FirstOrDefault(c => c.References.CreatureData.DatabaseID == _creatureConfig.DatabaseID);
            if (_trackedCreature != null)
            {
                _trackedCreature.References.EquipmentManager.OnItemEquipped += OnItemEquipped;
            }
        }

        private void HandlePartyMemberRecruited(string databaseID)
        {
            if (_trackedCreature != null) return;

            if (databaseID == _creatureConfig.DatabaseID)
            {
                _partyManager.GetPartyMemberByID(databaseID);
                _trackedCreature = _partyManager.GetPartyMemberByID(databaseID); ;
                if (_trackedCreature != null)
                {
                    _trackedCreature.References.EquipmentManager.OnItemEquipped += OnItemEquipped;
                }
            }
        }

        private void OnItemEquipped(IEquipableItem item)
        {
            if (_trackedCreature == null) return;

            var equippedItems = _trackedCreature.References.EquipmentManager.EquippedItems;
            var matchingCount = equippedItems.Count(item => _requiredItems.Any(required => required.DatabaseID == item.DatabaseID));
            _allItemsEquipped = matchingCount >= _requiredItems.Length;
        }

        public bool CheckCompletion(Objective objective)
        {
            return _allItemsEquipped;
        }
    }
}
