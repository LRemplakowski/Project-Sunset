using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using Sirenix.OdinInspector;
using SunsetSystems.ActionSystem;
using SunsetSystems.Combat;
using SunsetSystems.Combat.Grid;
using SunsetSystems.Persistence;
using TMPro;
using UnityEngine;

namespace SunsetSystems.Entities.Characters.Navigation
{
    public class NavigationManager : SerializedMonoBehaviour, INavigationManager, IPersistentComponent
    {
        private enum NavigationType
        {
            GridAI,
            RichAI
        }

        private const string COMPONENT_ID = "NAVIGATION_MANAGER";
        private const float MOVEMENT_THRESHOLD = 0.01f;

        [Title("Config")]
        [SerializeField]
        private float _faceTargetTime = 0.5f;

        [Title("References")]
        [SerializeField, Required]
        private FollowerEntity CurrentNavigationAI;
        [SerializeField]
        private SingleNodeBlocker _myBlocker;
        [SerializeField, Required]
        private IActionPerformer _actionPerformer;

        public Vector3 Position => CurrentNavigationAI.position;
        public bool FinishedCurrentPath => !CurrentNavigationAI.hasPath || CurrentNavigationAI.reachedCrowdedEndOfPath;
        public bool IsMoving => (CurrentNavigationAI.velocity.sqrMagnitude > MOVEMENT_THRESHOLD || !FinishedCurrentPath)
                                && CurrentNavigationAI.canMove
                                && !CurrentNavigationAI.isStopped;
        public float CurrentSpeed => CurrentNavigationAI.velocity.magnitude;
        public float MaxSpeed => CurrentNavigationAI.maxSpeed;
        public string ComponentID => COMPONENT_ID;

        private GraphMask _explorationMask;
        private GraphMask _combatMask;
        private GraphMask _currentGraphMask;
        private Coroutine _faceTargetCoroutine;
        private BlockManager.TraversalProvider _traversalProvider;

        private void Awake()
        {
            _currentGraphMask = _explorationMask;
            CurrentNavigationAI.pathfindingSettings.graphMask = _currentGraphMask;
        }

        private void Start()
        {
            CombatManager.OnCombatStart += OnCombatStart;
            CombatManager.OnCombatEnd += OnCombatEnd;

            BlockManager blockManager = FindAnyObjectByType<BlockManager>();
            _myBlocker.manager = blockManager;
            _traversalProvider = new BlockManager.TraversalProvider(blockManager, BlockManager.BlockMode.AllExceptSelector, new() { _myBlocker });
            foreach (var graph in AstarPath.active.graphs)
            {
                if (graph is GridGraph)
                {
                    _combatMask |= GraphMask.FromGraph(graph);
                }
                else
                {
                    _explorationMask |= GraphMask.FromGraph(graph);
                }
            }
            _currentGraphMask = _explorationMask;
            CurrentNavigationAI.pathfindingSettings.graphMask = _currentGraphMask;
        }

        private void OnCombatEnd(IEnumerable<ICombatant> _)
        {
            _currentGraphMask = _explorationMask;
            CurrentNavigationAI.pathfindingSettings.graphMask = _currentGraphMask;
        }

        private void OnCombatStart(IEnumerable<ICombatant> _)
        {
            _currentGraphMask = _combatMask;
            CurrentNavigationAI.pathfindingSettings.graphMask = _currentGraphMask;
        }

        private void OnDestroy()
        {
            CombatManager.OnCombatStart -= OnCombatStart;
            CombatManager.OnCombatEnd -= OnCombatEnd;
        }

        // Warp agent instantly to a position
        public bool Warp(Vector3 position)
        {
            // Set agent position directly
            CurrentNavigationAI.Teleport(position);
            return true;
        }

        // Calculate a path using A* ABPath
        public bool CalculatePath(Vector3 targetPosition, out ABPath path)
        {
            path = ABPath.Construct(Position, targetPosition, null);
            path.nnConstraint = new NNConstraint
            {
                graphMask = _currentGraphMask,
                constrainWalkability = true,
                walkable = true
            };
            AstarPath.StartPath(path);
            path.BlockUntilCalculated(); // synchronous calculation
            return path.CompleteState == PathCompleteState.Complete;
        }

        // Smooth rotation towards a point after movement
        public void FaceDirectionAfterMovementFinished(Vector3 point)
        {
            if (_faceTargetCoroutine != null)
                StopCoroutine(_faceTargetCoroutine);
            _faceTargetCoroutine = StartCoroutine(FaceTargetInTime(_faceTargetTime, point));
        }

        private IEnumerator FaceTargetInTime(float time, Vector3 targetPosition)
        {
            yield return new WaitUntil(() => IsMoving == false);

            Vector3 lookPosition = targetPosition - Position;
            lookPosition.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(lookPosition);
            Quaternion startRotation = CurrentNavigationAI.rotation;
            float slerp = 0f;

            while (slerp < time)
            {
                if (IsMoving)
                    yield break;
                slerp += Time.deltaTime;
                CurrentNavigationAI.rotation = Quaternion.Slerp(startRotation, targetRotation, slerp / time);
                yield return null;
            }

            CurrentNavigationAI.rotation = targetRotation;
        }

        // Set a navigation target for the agent
        public bool SetNavigationTarget(Vector3 target)
        {
            if (!CurrentNavigationAI.canMove)
                return false;
            CurrentNavigationAI.isStopped = false;
            CurrentNavigationAI.destination = target;
            CurrentNavigationAI.SearchPath();
            return true;
        }

        public bool SetGridTarget(IGridCell gridCell)
        {
            if (!CurrentNavigationAI.canMove)
                return false;
            CurrentNavigationAI.isStopped = false;
            ABPath path = ABPath.Construct(Position, gridCell.WorldPosition, null);
            path.nnConstraint = new NNConstraint
            {
                graphMask = _currentGraphMask,
                constrainWalkability = true,
                walkable = true,
            };
            path.traversalProvider = _traversalProvider;
            CurrentNavigationAI.SetPath(path);
            return true;
        }

        // Stop agent movement
        public void StopMovement(bool forceStopImmediate = false)
        {
            CurrentNavigationAI.isStopped = true;
        }

        public void SetNavigationEnabled(bool enabled)
        {
            CurrentNavigationAI.canMove = enabled;
        }

        public object GetComponentPersistenceData()
        {
            return new NavigatorPeristenceData(this);
        }

        public void InjectComponentPersistenceData(object data)
        {
            if (data is not NavigatorPeristenceData navData) return;
            CurrentNavigationAI.canMove = navData.NavigationEnabled;
        }

        [Serializable]
        public class NavigatorPeristenceData
        {
            public bool NavigationEnabled;

            public NavigatorPeristenceData(NavigationManager navigationManager)
            {
                NavigationEnabled = navigationManager.CurrentNavigationAI.canMove;
            }

            public NavigatorPeristenceData()
            {
                NavigationEnabled = true;
            }
        }
    }
}
