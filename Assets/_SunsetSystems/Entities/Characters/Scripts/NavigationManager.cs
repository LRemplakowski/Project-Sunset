using System;
using System.Collections;
using Sirenix.OdinInspector;
using SunsetSystems.ActionSystem;
using SunsetSystems.Persistence;
using UnityEngine;
using UnityEngine.AI;

namespace SunsetSystems.Entities.Characters.Navigation
{
    public class NavigationManager : SerializedMonoBehaviour, INavigationManager, IPersistentComponent
    {
        private const string COMPONENT_ID = "NAVIGATION_MANAGER";

        [Title("Config")]
        [SerializeField]
        private float _faceTargetTime = .5f;
        [Title("References")]
        [SerializeField, Required]
        private NavMeshAgent _navMeshAgent;
        [SerializeField, Required]
        private IActionPerformer _actionPerformer;

        private Coroutine _faceTargetCoroutine;

        public Vector3 Position => _navMeshAgent.transform.position;
        public bool FinishedCurrentPath
        {
            get
            {
                if (_navMeshAgent.pathPending)
                    return false;
                return _navMeshAgent.hasPath && GetAgentFinishedMovement();
            }
        }

        public bool IsMoving
        {
            get
            {
                if (_navMeshAgent.pathPending)
                    return true;
                return _navMeshAgent.hasPath && !GetAgentFinishedMovement() || _actionPerformer.PeekCurrentAction is Move or MoveAbilityAction;
            }
        }

        public float CurrentSpeed => _navMeshAgent.velocity.magnitude;
        public float MaxSpeed => _navMeshAgent.speed;

        public string ComponentID => COMPONENT_ID;

        public bool Warp(Vector3 position)
        {
            if (NavMesh.SamplePosition(position, out NavMeshHit hit, 1f, (int)NavMeshAreas.Walkable))
            {
                return _navMeshAgent.Warp(position);
            }
            else
            {
                Debug.LogError($"{nameof(NavigationManager)} >>> Failed to Warp Creature {GetComponentInParent<ICreatureReferences>().GameObject} to position {position}! Position is not valid!");
                return false;
            }
        }

        public bool CalculatePath(Vector3 targetPosition, NavMeshPath path) => _navMeshAgent.CalculatePath(targetPosition, path);

        private bool GetAgentFinishedMovement()
        {
            if (_navMeshAgent.isStopped)
                return true;
            return _navMeshAgent.pathStatus switch
            {
                NavMeshPathStatus.PathComplete => _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance,
                NavMeshPathStatus.PathPartial => _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance,
                NavMeshPathStatus.PathInvalid => true,
                _ => false,
            };
        }

        public void FaceDirectionAfterMovementFinished(Vector3 point)
        {
            if (_faceTargetCoroutine != null)
                StopCoroutine(_faceTargetCoroutine);
            StartCoroutine(FaceTargetInTime(_faceTargetTime, point));
        }

        private IEnumerator FaceTargetInTime(float time, Vector3 targetPosition)
        {
            yield return new WaitUntil(() => IsMoving is false);
            Vector3 lookPosition = targetPosition - _navMeshAgent.transform.position;
            lookPosition.y = 0;
            Quaternion targetRotation = Quaternion.LookRotation(lookPosition);
            Quaternion startRotation = _navMeshAgent.transform.rotation;
            float slerp = 0f;
            while (slerp < time)
            {
                if (IsMoving)
                    yield break;
                slerp += Time.deltaTime;
                _navMeshAgent.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, slerp / time);
                yield return null;
            }
            _navMeshAgent.transform.rotation = targetRotation;
        }

        public bool SetNavigationTarget(Vector3 target)
        {
            if (_navMeshAgent.isActiveAndEnabled is false)
                return false;
            _navMeshAgent.isStopped = false;
            return _navMeshAgent.SetDestination(target);
        }

        public void StopMovement(bool forceStopImmediate = false)
        {
            _navMeshAgent.ResetPath();
            if (forceStopImmediate)
                _navMeshAgent.isStopped = true;
        }

        public void SetNavigationEnabled(bool enabled)
        {
            _navMeshAgent.enabled = enabled;
        }

        public object GetComponentPersistenceData()
        {
            return new NavigatorPeristenceData(this);
        }

        public void InjectComponentPersistenceData(object data)
        {
            if (data is not NavigatorPeristenceData navData)
                return;
            _navMeshAgent.enabled = navData.NavigationEnabled;
        }

        [Serializable]
        public class NavigatorPeristenceData
        {
            public bool NavigationEnabled;

            public NavigatorPeristenceData(NavigationManager navigationManager)
            {
                NavigationEnabled = navigationManager._navMeshAgent.enabled;
            }

            public NavigatorPeristenceData()
            {
                NavigationEnabled = true;
            }
        }
    }
}
