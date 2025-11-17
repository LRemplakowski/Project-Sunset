using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Pathfinding;
using Sirenix.OdinInspector;
using SunsetSystems.ActionSystem;
using SunsetSystems.Combat;
using SunsetSystems.Combat.Grid;
using SunsetSystems.Persistence;
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
                                && CurrentNavigationAI.simulateMovement
                                && !CurrentNavigationAI.isStopped;
        public float CurrentSpeed => CurrentNavigationAI.velocity.magnitude;
        public float MaxSpeed => CurrentNavigationAI.maxSpeed;
        public string ComponentID => COMPONENT_ID;

        private GraphMask _explorationMask;
        private GraphMask _combatMask;
        private GraphMask _currentGraphMask;
        private Coroutine _faceTargetCoroutine;
        private BlockManager.TraversalProvider _traversalProvider;
        private IGridCell _currentGridCell = null;

        private void Awake()
        {
            _currentGraphMask = _explorationMask;
            CurrentNavigationAI.pathfindingSettings.graphMask = _currentGraphMask;
        }

        private void Start()
        {
            CombatManager.OnCombatStart += OnCombatStart;
            CombatManager.OnCombatEnd += OnCombatEnd;
            CombatManager.OnCombatRoundBegin += OnCombatRoundStart;

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
            _currentGridCell = null;
            _currentGraphMask = _explorationMask;
            CurrentNavigationAI.pathfindingSettings.graphMask = _currentGraphMask;
            _myBlocker.Unblock();
        }

        private void OnCombatStart(IEnumerable<ICombatant> _)
        {
            _currentGridCell = null;
            _currentGraphMask = _combatMask;
            CurrentNavigationAI.pathfindingSettings.graphMask = _currentGraphMask;
        }

        private void OnCombatRoundStart(ICombatant combatant)
        {
            _myBlocker.BlockAtCurrentPosition();
        }

        private void OnDestroy()
        {
            CombatManager.OnCombatStart -= OnCombatStart;
            CombatManager.OnCombatEnd -= OnCombatEnd;
            CombatManager.OnCombatRoundBegin -= OnCombatRoundStart;
        }

        // Warp agent instantly to a position
        public bool Warp(Vector3 position, Quaternion rotation)
        {
            // Set agent position directly
            CurrentNavigationAI.Teleport(position, true);
            CurrentNavigationAI.rotation = rotation;
            CurrentNavigationAI.destination = position;
            StopMovement();
            return true;
        }

        // Calculate a path using A* ABPath
        [Button]
        public bool CalculatePath(Vector3 targetPosition, out ABPath path)
        {
            path = StartSinglePath(targetPosition);
            path.BlockUntilCalculated(); // synchronous calculation
            return path.CompleteState == PathCompleteState.Complete;
        }

        public async UniTask<ABPath> CalculatePathAsync(Vector3 targetPosition)
        {
            ABPath path = StartSinglePath(targetPosition);
            await path.WaitForPath();
            return path;
        }

        private ABPath StartSinglePath(Vector3 targetPosition)
        {
            ABPath path = ABPath.Construct(Position, targetPosition, null);
            var traversalConstraint = TraversalConstraint.None;
            traversalConstraint.graphMask = _currentGraphMask;
            traversalConstraint.traversalProvider = _traversalProvider;
            path.traversalConstraint = traversalConstraint;
            AstarPath.StartPath(path);
            return path;
        }

        [Button]
        public Dictionary<Vector3, float> CalculateMultiplePaths(Vector3[] targetPositions)
        {
            Dictionary<Vector3, float> results = new();
            OnPathDelegate[] pathDelegates = new OnPathDelegate[targetPositions.Length];
            for (int i = 0; i < pathDelegates.Length; i++)
            {
                Vector3 targetPosition = targetPositions[i];
                pathDelegates[i] = (path) => OnPath(targetPosition, path);
            }
            MultiTargetPath path = StartMultiPath(targetPositions, pathDelegates);
            path.BlockUntilCalculated(); // synchronous calculation
            return results;

            void OnPath(in Vector3 targetPosition, Path p)
            {
                if (p.CompleteState != PathCompleteState.Complete)
                    return;
                results[targetPosition] = p.GetTotalLength();
            }
        }

        public async UniTask<Dictionary<Vector3, float>> CalculateMultiplePathsAsync(Vector3[] targetPositions)
        {
            Dictionary<Vector3, float> results = new();
            OnPathDelegate[] pathDelegates = new OnPathDelegate[targetPositions.Length];
            for (int i = 0; i < pathDelegates.Length; i++)
            {
                Vector3 targetPosition = targetPositions[i];
                pathDelegates[i] = (path) => OnPath(targetPosition, path);
            }
            MultiTargetPath path = StartMultiPath(targetPositions, pathDelegates);
            await path.WaitForPath();
            return results;

            void OnPath(in Vector3 targetPosition, Path p)
            {
                if (p.CompleteState != PathCompleteState.Complete)
                    return;
                results[targetPosition] = p.GetTotalLength();
            }

        }

        private MultiTargetPath StartMultiPath(Vector3[] targetPositions, OnPathDelegate[] pathDelegates)
        {
            NearestNodeConstraint nodeConstraint = NearestNodeConstraint.Walkable;
            nodeConstraint.graphMask = _currentGraphMask;
            nodeConstraint.traversalProvider = _traversalProvider;
            MultiTargetPath path;
            if (_currentGridCell != null)
            {
                path = MultiTargetPath.Construct(_currentGridCell.WorldPosition, targetPositions, pathDelegates);
            }
            else
            {
                var nearestNode = AstarPath.active.GetNearest(Position, nodeConstraint);
                path = MultiTargetPath.Construct(nearestNode.position, targetPositions, pathDelegates);
            }
            path.nearestNodeDistanceMetric = DistanceMetric.ClosestAsSeenFromAbove(Vector3.up);
            var traversalConstraint = TraversalConstraint.None;
            traversalConstraint.graphMask = _currentGraphMask;
            traversalConstraint.traversalProvider = _traversalProvider;
            path.traversalConstraint = traversalConstraint;
            path.pathsForAll = true;
            AstarPath.StartPath(path);
            return path;
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
            if (!CurrentNavigationAI.simulateMovement)
                return false;
            CurrentNavigationAI.isStopped = false;
            CurrentNavigationAI.pathfindingSettings.traversalProvider = null;
            CurrentNavigationAI.destination = target;
            CurrentNavigationAI.SearchPath();
            return true;
        }

        [Button]
        public bool SetGridTarget(IGridCell gridCell)
        {
            if (!CurrentNavigationAI.simulateMovement)
                return false;
            CurrentNavigationAI.isStopped = false;
            CurrentNavigationAI.pathfindingSettings.traversalProvider = _traversalProvider;
            CurrentNavigationAI.destination = gridCell.WorldPosition;
            CurrentNavigationAI.SearchPath();
            _currentGridCell = gridCell;
            return true;
        }

        // Stop agent movement
        public void StopMovement(bool forceStopImmediate = false)
        {
            CurrentNavigationAI.isStopped = true;
        }

        public void SetNavigationEnabled(bool enabled)
        {
            CurrentNavigationAI.simulateMovement = enabled;
        }

        public Vector3Int GetGridPosition(GridManager grid)
        {
            if (_currentGridCell != null)
                return _currentGridCell.GridPosition;
            return grid.WorldPositionToGridPosition(Position);
        }

        public object GetComponentPersistenceData()
        {
            return new NavigatorPeristenceData(this);
        }

        public void InjectComponentPersistenceData(object data)
        {
            if (data is not NavigatorPeristenceData navData) return;
            CurrentNavigationAI.simulateMovement = navData.NavigationEnabled;
        }

        [Serializable]
        public class NavigatorPeristenceData
        {
            public bool NavigationEnabled;

            public NavigatorPeristenceData(NavigationManager navigationManager)
            {
                NavigationEnabled = navigationManager.CurrentNavigationAI.simulateMovement;
            }

            public NavigatorPeristenceData()
            {
                NavigationEnabled = true;
            }
        }
    }
}
