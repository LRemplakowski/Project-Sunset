using System;
using System.Collections;
using Sirenix.OdinInspector;
using SunsetSystems.ActionSystem;
using SunsetSystems.Persistence;
using UnityEngine;
using Pathfinding;
using SunsetSystems.Combat;
using System.Collections.Generic;

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
        private GraphMask _explorationMask;
        [SerializeField]
        private GraphMask _combatMask;
        [SerializeField, Required]
        private IActionPerformer _actionPerformer;

        private Coroutine _faceTargetCoroutine;

        public Vector3 Position => CurrentNavigationAI.position;
        private GraphMask CurrentGraphMask { get; set; }

        public bool FinishedCurrentPath => !CurrentNavigationAI.pathPending && CurrentNavigationAI.reachedEndOfPath;

        public bool IsMoving =>
            CurrentNavigationAI.velocity.sqrMagnitude > MOVEMENT_THRESHOLD ||
            _actionPerformer.PeekCurrentAction is Move or MoveAbilityAction;

        public float CurrentSpeed => CurrentNavigationAI.velocity.magnitude;
        public float MaxSpeed => CurrentNavigationAI.maxSpeed;

        public string ComponentID => COMPONENT_ID;

        private void Awake()
        {
            CurrentGraphMask = _explorationMask;
            CurrentNavigationAI.pathfindingSettings.graphMask = CurrentGraphMask;
        }

        private void Start()
        {
            CombatManager.OnCombatStart += OnCombatStart;
            CombatManager.OnCombatEnd += OnCombatEnd;
        }

        private void OnCombatEnd(IEnumerable<ICombatant> _)
        {
            CurrentGraphMask = _explorationMask;
            CurrentNavigationAI.pathfindingSettings.graphMask = CurrentGraphMask;
        }

        private void OnCombatStart(IEnumerable<ICombatant> _)
        {
            CurrentGraphMask = _combatMask;
            CurrentNavigationAI.pathfindingSettings.graphMask = CurrentGraphMask;
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
                graphMask = CurrentGraphMask,
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
