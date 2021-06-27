﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(Inventory)), RequireComponent(typeof(StatsManager)), RequireComponent(typeof(CombatBehaviour))]
public abstract class Creature : Entity
{
    public enum Sex
    {
        M, 
        F
    }

    public Sex GetSex()
    {
        return sex;
    }

    private const float lookTowardsRotationSpeed = 5.0f;
    [SerializeField]
    private Sex sex = Sex.F;
    public NavMeshAgent agent;
    public Inventory inventory;
    [SerializeField, ReadOnly]
    private GridElement _currentGridPosition;
    [ExposeProperty]
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

    [HideInInspector]
    public Queue<EntityAction> ActionQueue { get; private set; }

    public abstract void Move(Vector3 moveTarget);
    public abstract void Move(GridElement moveTarget);
    public abstract void Attack(Creature target);

    private void Start()
    {
        inventory = GetComponent<Inventory>();
        agent = GetComponent<NavMeshAgent>();
        ActionQueue = new Queue<EntityAction>();
        ActionQueue.Enqueue(new Idle(this));
    }

    public void AddActionToQueue(EntityAction action)
    {
        ActionQueue.Enqueue(action);
    }

    public void ClearAllActions()
    {
        ActionQueue.Peek().Abort();
        ActionQueue.Clear();
        ActionQueue.Enqueue(new Idle(this));
    }

    public bool RotateTowardsTarget(Transform target)
    {
        if (target == null)
            return true;
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * lookTowardsRotationSpeed);
        float dot = Quaternion.Dot(transform.rotation, lookRotation);
        return dot >= 0.999f || dot <= -0.999f;
    }

    public IEnumerator FaceTarget(Transform target)
    {
        yield return new WaitUntil(() => RotateTowardsTarget(target));
        StopCoroutine(FaceTarget(target));
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
            //Debug.Log("Action finished!\n" + ActionQueue.Peek());
            ActionQueue.Dequeue();
            if (ActionQueue.Count == 0) 
                ActionQueue.Enqueue(new Idle(this));
            ActionQueue.Peek().Begin();
        }
    }

    private void OnDrawGizmos()
    {   
        float movementRange = GetComponent<StatsManager>().GetCombatSpeed();
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, movementRange);
    }
}
