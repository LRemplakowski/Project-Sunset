using Sirenix.OdinInspector;
using SunsetSystems.Abilities;
using SunsetSystems.Combat.Grid;
using SunsetSystems.Entities.Characters.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace SunsetSystems.ActionSystem
{
    public class MoveAbilityAction : EntityAction
    {
        [SerializeField]
        private INavigationManager navigationManager;
        [SerializeField, ReadOnly]
        private Vector3 destination;

        public MoveAbilityAction(MoveAbility ability, IAbilityContext context) : base(context.SourceActionPerformer)
        {
            var gridInstance = context.GridManager;
            var gridCell = context.TargetObject as IGridCell;
            var combatant = context.SourceCombatBehaviour;
            navigationManager = context.SourceCombatBehaviour.References.NavigationManager;
            NavMesh.SamplePosition(gridCell.WorldPosition, out var hit, 1f, NavMesh.AllAreas);
            destination = hit.position;
            conditions.Add(new Destination(navigationManager));
            this.destination = hit.position;
            if (gridInstance.TryGetCurrentGridCell(combatant, out IGridCell occupiedCell))
            {
                gridInstance.ClearOccupierFromCell(occupiedCell);
            }
            gridInstance.HandleCombatantMovedIntoGridCell(context.SourceCombatBehaviour, gridCell);
        }

        public override void Cleanup()
        {
            base.Cleanup();
            navigationManager.StopMovement(true);
        }

        public override void Begin()
        {
            navigationManager.SetNavigationTarget(destination);
        }
    }
}
