using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using SunsetSystems.Combat;
using SunsetSystems.Core;
using SunsetSystems.Persistence;
using UnityEngine;

namespace SunsetSystems.ActorResources
{
    public class ActorResourcesUser : SerializedMonoBehaviour, IActionPointUser, IMovementPointUser, IBloodPointUser, IPersistentComponent
    {
        private const string COMPONENT_ID = "ACTOR_RESOURCE_MANAGER";

        [Title("References")]
        [SerializeField, Required]
        private ICombatant _combatBehaviour;

        [Title("Data")]
        [SerializeField, DictionaryDrawerSettings(IsReadOnly = true, KeyLabel = "Resource", ValueLabel = "Current Amount")]
        private Dictionary<ActorResource, int> _currentResources = new();

        [Title("Runtime")]
        [ShowInInspector, ReadOnly]
        private readonly HashSet<string> _movementBlockers = new();
        [ShowInInspector, ReadOnly]
        private readonly HashSet<string> _apBlockers = new();
        [ShowInInspector, ReadOnly]
        private readonly HashSet<string> _bloodBlockers = new();

        public string ComponentID => COMPONENT_ID;

        public event Action<int> OnActionPointUpdate;
        public event Action<int> OnBloodPointUpdate;

        private void OnValidate()
        {
            _currentResources ??= new();
            foreach (ActorResource resource in Enum.GetValues(typeof(ActorResource)))
            {
                if (_currentResources.ContainsKey(resource) is false)
                {
                    _currentResources[resource] = 0;
                }
            }
        }

        private void Awake()
        {
            CombatManager.OnCombatStart += OnCombatStart;
            CombatManager.OnCombatRoundEnd += OnCombatRoundEnd;
        }

        private void Start()
        {
            InitializeResources();
        }

        private void OnDestroy()
        {
            CombatManager.OnCombatStart -= OnCombatStart;
            CombatManager.OnCombatRoundEnd -= OnCombatRoundEnd;
        }

        private void ReplenishResources()
        {
            _currentResources[ActorResource.ActionPoints] = GetMaxActionPoints();
        }

        private void OnCombatStart(IEnumerable<ICombatant> _)
        {
            ReplenishResources();
        }

        private void OnCombatRoundEnd(ICombatant combatant)
        {
            if (combatant == _combatBehaviour)
            {
                ReplenishResources();
            }
        }

        private void InitializeResources()
        {
            var keys = _currentResources.Keys.ToList();
            foreach (var resourceType in keys)
            {
                _currentResources[resourceType] = GetMaxResource(resourceType);
            }
        }

        private int GetMaxResource(ActorResource resourceType)
        {
            return resourceType switch
            {
                ActorResource.ActionPoints => 2,
                ActorResource.BloodPoints => 5,
                _ => 0,
            };
        }

        private static bool HasAnyBlockers(ICollection<string> blockerCollection) => blockerCollection.Count > 0;

        private static bool HasResourceRemaining(Dictionary<ActorResource, int> resourceMap, ActorResource resource)
        {
            return resourceMap.TryGetValue(resource, out var remaining) && remaining > 0;
        }

        private static int ActionPointsToMovementPoints(int actionPoints) => actionPoints * GameConstants.MOVEMENT_PER_AP;

        private static int MovementPointsToActionPoints(int movementPoints)
        {
            int actionPoints = movementPoints / GameConstants.MOVEMENT_PER_AP;
            actionPoints += movementPoints % GameConstants.MOVEMENT_PER_AP > 0 ? 1 : 0;
            return actionPoints;
        }

        #region IActionPointUser
        public int GetCurrentActionPoints()
        {
            return _currentResources[ActorResource.ActionPoints];
        }

        public int GetMaxActionPoints() => GetMaxResource(ActorResource.ActionPoints);

        public bool CanUseActionPoints()
        {
            bool result = true;
            result &= !HasAnyBlockers(_apBlockers);
            result &= HasResourceRemaining(_currentResources, ActorResource.ActionPoints);
            return result;
        }

        public bool UseActionPoints(int amount)
        {
            if (_currentResources.TryGetValue(ActorResource.ActionPoints, out var currentAP) && currentAP >= amount)
            {
                currentAP -= amount;
                _currentResources[ActorResource.ActionPoints] = currentAP;
                OnActionPointUpdate?.Invoke(currentAP);
                return true;
            }
            return false;
        }

        public void AddActionPointUseBlocker(string sourceID)
        {
            _apBlockers.Add(sourceID);
        }

        public void RemoveActionPointUseBlocker(string sourceID)
        {
            _apBlockers.Remove(sourceID);
        }
        #endregion

        #region IMovementPointUser
        public int GetCurrentMovementPoints()
        {
            return ActionPointsToMovementPoints(_currentResources[ActorResource.ActionPoints]);
        }

        public int GetMaxMovementPoints() => ActionPointsToMovementPoints(GetMaxResource(ActorResource.ActionPoints));

        public bool GetCanMove()
        {
            bool result = true;
            result &= !HasAnyBlockers(_movementBlockers);
            result &= HasResourceRemaining(_currentResources, ActorResource.ActionPoints);
            return result;
        }

        public bool UseMovementPoints(int amount)
        {
            if (_currentResources.TryGetValue(ActorResource.ActionPoints, out var currentAP) && ActionPointsToMovementPoints(currentAP) >= amount)
            {
                _currentResources[ActorResource.ActionPoints] -= MovementPointsToActionPoints(amount);
                return true;
            }
            return false;
        }

        public void AddMovementPointUseBlocker(string sourceID)
        {
            _movementBlockers.Add(sourceID);
        }

        public void RemoveMovementPointUseBlocker(string sourceID)
        {
            _movementBlockers.Remove(sourceID);
        }
        #endregion

        #region IBloodPointUser
        public int GetCurrentBloodPoints()
        {
            return _currentResources[ActorResource.BloodPoints];
        }

        public int GetMaxBloodPoints() => GetMaxResource(ActorResource.BloodPoints);

        public bool GetCanUseBloodPoints()
        {
            bool result = true;
            result &= !HasAnyBlockers(_bloodBlockers);
            result &= HasResourceRemaining(_currentResources, ActorResource.BloodPoints);
            return result;
        }

        public bool UseBloodPoints(int amount)
        {
            if (_currentResources.TryGetValue(ActorResource.BloodPoints, out var currentBP) && currentBP >= amount)
            {
                currentBP -= amount;
                _currentResources[ActorResource.BloodPoints] = currentBP;
                OnBloodPointUpdate?.Invoke(currentBP);
                return true;
            }
            return false;
        }

        public void AddBloodPointUseBlocker(string sourceID)
        {
            _bloodBlockers.Add(sourceID);
        }

        public void RemoveBloodPointUseBlocker(string sourceID)
        {
            _bloodBlockers.Remove(sourceID);
        }
        #endregion

        #region IPersistentComponent
        public object GetComponentPersistenceData()
        {
            return new ResourcePersistentData(this);
        }

        public void InjectComponentPersistenceData(object data)
        {
            if (data is not ResourcePersistentData resourceData)
                return;
            _currentResources = new(resourceData.CurrentResources);
        }

        [Serializable]
        public class ResourcePersistentData
        {
            public Dictionary<ActorResource, int> CurrentResources;

            public ResourcePersistentData(ActorResourcesUser resourceManager)
            {
                CurrentResources = new(resourceManager._currentResources);
            }

            public ResourcePersistentData()
            {
                CurrentResources = new();
            }
        }
        #endregion
    }
}
