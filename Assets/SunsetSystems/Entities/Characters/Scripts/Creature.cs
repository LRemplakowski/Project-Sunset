﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Apex.AI.Components;
using Entities.Characters.Data;
using SunsetSystems.Entities.Characters.Actions;
using System.Threading.Tasks;
using SunsetSystems.Utils.Threading;
using NaughtyAttributes;
using SunsetSystems.Loading;
using UMA.CharacterSystem;
using Redcode.Awaiting;

namespace SunsetSystems.Entities.Characters
{
    [RequireComponent(typeof(NavMeshAgent)),
    RequireComponent(typeof(NavMeshObstacle)),
    RequireComponent(typeof(StatsManager)),
    RequireComponent(typeof(CombatBehaviour)),
    RequireComponent(typeof(CreatureAnimator)),
    RequireComponent(typeof(Rigidbody)),
    RequireComponent(typeof(CapsuleCollider)),
    RequireComponent(typeof(Animator)),
    RequireComponent(typeof(DynamicCharacterAvatar)),
    RequireComponent(typeof(StatsManager)),
    RequireComponent(typeof(UtilityAIComponent))]
    public abstract class Creature : Entity, ISaveRuntimeData
    {
        private const float LOOK_TOWARDS_ROTATION_SPEED = 5.0f;

        public CreatureData Data { get; private set; } = new();

        [SerializeField]
        private CreatureConfig config;
        [field: SerializeField]
        public NavMeshAgent Agent { get; protected set; }

        [field: SerializeField]
        public NavMeshObstacle NavMeshObstacle { get; protected set; }

        [field: SerializeField]
        public StatsManager StatsManager { get; protected set; }

        [field: SerializeField]
        public CombatBehaviour CombatBehaviour { get; private set; }

        [SerializeField, ReadOnly]
        protected GridElement _currentGridPosition;
        public GridElement CurrentGridPosition
        {
            get => _currentGridPosition;
            set
            {
                if (_currentGridPosition)
                {
                    _currentGridPosition.Visited = GridElement.Status.NotVisited;
                }
                value.Visited = GridElement.Status.Occupied;
                _currentGridPosition = value;
            }
        }

        private Queue<EntityAction> _actionQueue;
        private Queue<EntityAction> ActionQueue
        {
            get
            {
                if (_actionQueue == null)
                {
                    _actionQueue = new Queue<EntityAction>();
                    AddActionToQueue(new Idle(this));
                }
                return _actionQueue;
            }
        }

        public bool IsAlive => StatsManager.IsAlive();
        public bool IsVampire => Data.creatureType.Equals(CreatureType.Vampire);

        #region Unity messages
        private void OnValidate()
        {
            Awake();
        }

        protected virtual void Awake()
        {
            if (!StatsManager)
                StatsManager = GetComponent<StatsManager>();
            if (!Agent)
                Agent = GetComponent<NavMeshAgent>();
            if (!CombatBehaviour)
                CombatBehaviour = GetComponent<CombatBehaviour>();
            if (!NavMeshObstacle)
                NavMeshObstacle = GetComponent<NavMeshObstacle>();
            Agent.enabled = false;
            NavMeshObstacle.enabled = true;
            if (config)
                Data = new(config);
            StatsManager.Initialize(Data.stats);
            LoadRuntimeData();
        }

        protected virtual void Start()
        {
            ActionQueue.Enqueue(new Idle(this));
        }

        public virtual void OnDestroy()
        {
            SaveRuntimeData();
        }

        public void Update()
        {
            if (ActionQueue.Peek().GetType() == typeof(Idle) && ActionQueue.Count > 1)
            {
                ActionQueue.Dequeue();
                ActionQueue.Peek().Begin();
            }
            if (ActionQueue.Peek().IsFinished())
            {
                ActionQueue.Dequeue();
                if (ActionQueue.Count == 0)
                    ActionQueue.Enqueue(new Idle(this));
                ActionQueue.Peek().Begin();
            }
        }
        #endregion

        #region Actions and control
        public void ForceCreatureToPosition(Vector3 position)
        {
            ClearAllActions();
            Debug.LogWarning("Forcing creature to position: " + position);
            Agent.Warp(position);
        }

        public void AddActionToQueue(EntityAction action)
        {
            ActionQueue.Enqueue(action);
        }

        public void ClearAllActions()
        {
            EntityAction currentAction = ActionQueue.Peek();
            if (currentAction != null)
                currentAction.Abort();
            ActionQueue.Clear();
            ActionQueue.Enqueue(new Idle(this));
        }

        public bool RotateTowardsTarget(Transform target)
        {
            if (target == null)
                return true;
            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * LOOK_TOWARDS_ROTATION_SPEED);
            float dot = Quaternion.Dot(transform.rotation, lookRotation);
            return dot >= 0.999f || dot <= -0.999f;
        }

        public async Task FaceTarget(Transform target)
        {
            while (!RotateTowardsTarget(target))
            {
                await new WaitForUpdate();
            }
        }

        public EntityAction PeekActionFromQueue()
        {
            return ActionQueue.Peek();
        }

        public bool HasActionsInQueue()
        {
            return !ActionQueue.Peek().GetType().IsAssignableFrom(typeof(Idle)) || ActionQueue.Count > 1;
        }

        public abstract void Move(Vector3 moveTarget, float stoppingDistance);
        public abstract void Move(Vector3 moveTarget);
        public abstract void Move(GridElement moveTarget);
        public abstract void Attack(Creature target);
        #endregion

        private void OnDrawGizmos()
        {
            float movementRange = GetComponent<StatsManager>().GetCombatSpeed();
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, movementRange);
        }

        public CreatureUIData GetCreatureUIData()
        {
            HealthData healthData = StatsManager.GetHealthData();
            CreatureUIData.CreatureDataBuilder builder = new(Data.FullName,
                Data.portrait,
                healthData,
                0);
            return builder.Create();
        }

        public void SaveRuntimeData()
        {
            //throw new System.NotImplementedException();
        }

        public void LoadRuntimeData()
        {
            //throw new System.NotImplementedException();
        }
    }
}
