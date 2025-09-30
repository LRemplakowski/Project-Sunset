using System;
using Sirenix.OdinInspector;
using SunsetSystems.Combat;
using SunsetSystems.Combat.Grid;
using SunsetSystems.Entities.Characters.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace SunsetSystems.ActionSystem
{
    [System.Serializable]
    public class Move : EntityAction, ICloneable
    {
        [SerializeField]
        private INavigationManager navigationManager;
        [SerializeField, ReadOnly]
        private Vector3 destination;
        public static event Action<IActionPerformer> OnMovementFinished;
        public static event Action<IActionPerformer> OnMovementStarted;
        //private Task rotationTask;

        public Move(IActionPerformer owner, Vector3 destination/*, float stoppingDistance = .1f*/) : base(owner, false)
        {
            this.navigationManager = owner.References.NavigationManager;
            NavMesh.SamplePosition(destination, out var hit, 1f, NavMesh.AllAreas);
            conditions.Add(new Destination(navigationManager));
            this.destination = hit.position;
        }

        public Move(ICombatant owner, IGridCell gridCell, GridManager gridInstance) : this(owner, gridInstance.GridPositionToWorldPosition(gridCell.GridPosition))
        {
            if (gridInstance.TryGetCurrentGridCell(owner, out IGridCell occupiedCell))
            {
                gridInstance.ClearOccupierFromCell(occupiedCell);
            }
            gridInstance.HandleCombatantMovedIntoGridCell(owner, gridCell);
        }

        private Move(Move from) : this(from.Owner, from.destination)
        {

        }

        public object Clone()
        {
            return new Move(this);
        }

        public override void Cleanup()
        {
            base.Cleanup();
            navigationManager.StopMovement();
            OnMovementFinished?.Invoke(this.Owner);
        }

        public override void Begin()
        {
            if (navigationManager.SetNavigationTarget(destination)) 
            {
                OnMovementStarted?.Invoke(this.Owner);
            }
            else
            {
                Cleanup();
            }
        }
    }
}
