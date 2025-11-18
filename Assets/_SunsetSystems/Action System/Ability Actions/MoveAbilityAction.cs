using Sirenix.OdinInspector;
using SunsetSystems.Abilities;
using SunsetSystems.Combat;
using SunsetSystems.Combat.Grid;
using SunsetSystems.Entities.Characters.Navigation;
using UnityEngine;

namespace SunsetSystems.ActionSystem
{
    public class MoveAbilityAction : EntityAction
    {
        [SerializeField]
        private INavigationManager navigationManager;
        [SerializeField, ReadOnly]
        private IGridCell destination;
        [SerializeField]
        private GridManager gridInstance;
        [SerializeField]
        private ICombatant combatant;

        public MoveAbilityAction(MoveAbility ability, IAbilityContext context) : base(context.SourceActionPerformer)
        {
            gridInstance = context.GridManager;
            destination = context.TargetObject as IGridCell;
            combatant = context.SourceCombatBehaviour;
            navigationManager = context.SourceCombatBehaviour.References.NavigationManager;
        }

        public override void Cleanup()
        {
            base.Cleanup();
            navigationManager.StopMovement();
        }

        public override void Begin()
        {
            var destinationCondition = new Destination(navigationManager);
            var delayCondition = new Delay(.2f, destinationCondition);
            conditions.Add(destinationCondition);
            conditions.Add(delayCondition);
            if (gridInstance.TryGetCurrentGridCell(combatant, out IGridCell occupiedCell))
            {
                gridInstance.ClearOccupierFromCell(occupiedCell);
            }
            gridInstance.HandleCombatantMovedIntoGridCell(combatant, destination);
            bool destinationSet = navigationManager.SetGridTarget(destination);
            if (!destinationSet)
            {
                Debug.LogError("Failed to set navigation target for MoveAbilityAction, aborting action.");
                Abort();
            }
        }
    }
}
