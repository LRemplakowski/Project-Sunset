using System;
using SunsetSystems.Abilities.Execution;
using SunsetSystems.Abilities.Targeting;
using SunsetSystems.ActionSystem;
using SunsetSystems.Combat.Grid;
using SunsetSystems.Inventory;
using UnityEngine;

namespace SunsetSystems.Abilities
{
    [CreateAssetMenu(fileName = "New Move Ability", menuName = "Sunset Abilities/Move")]
    public class MoveAbility : AbstractAbilityConfig
    {
        private IAbilityExecutionStrategy _executionStrategy;
        private IAbilityTargetingStrategy _targetingStrategy;

        protected override bool ValidateAbilityTarget(IAbilityContext context)
        {
            return GetTargetAsGridCell(context, out var gridCell)
                   && IsPositionFree(gridCell);

            static bool GetTargetAsGridCell(IAbilityContext context, out IGridCell cell)
            {
                cell = default;
                if (context.TargetObject is IGridCell gridCell)
                {
                    cell = gridCell;
                    return true;
                }
                return false;
            }

            static bool IsPositionFree(IGridCell cell) => cell.IsFree;
        }

        protected override RangeData GetAbilityRangeData(IAbilityContext context)
        {
            var mover = context.SourceCombatBehaviour.GetContext().MovementManager;
            return new()
            {
                OptimalRange = mover.GetCurrentMovementPoints() / 2,
                MaxRange = mover.GetCurrentMovementPoints()
            };
        }

        protected override int GetMovementPointCost(IAbilityContext context)
        {
            if (context.TargetObject is not IGridCell position)
            {
                return int.MaxValue;
            }
            var combatBehaviour = context.SourceCombatBehaviour;
            var navManager = combatBehaviour.References.NavigationManager;
            var gridManager = context.GridManager;
            navManager.CalculatePath(position.WorldPosition, out var path);
            var movementCost = Mathf.RoundToInt(path.GetTotalLength() / gridManager.GetGridScale()) * _baseMovementCost;
            return movementCost;
        }

        public override IAbilityExecutionStrategy GetExecutionStrategy() => _executionStrategy ??= new MoveStrategy(this);
        public override IAbilityTargetingStrategy GetTargetingStrategy() => _targetingStrategy ??= new DoubleClickGridStrategy(this);

        protected override string GetAPCostText(IAbilityContext context)
        {
            if (_baseMovementCost <= 0)
                return string.Empty;
            return $"AP: {_baseMovementCost} per 5 Tiles";
        }
    }
}
