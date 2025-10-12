using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SunsetSystems.Combat;
using SunsetSystems.Combat.Grid;
using UnityEngine;

namespace SunsetSystems.AI
{
    public static class AIHelpers
    {
        public static IEnumerable<IGridCell> GetPositionsInRange(ICombatant gridActor, int range, GridManager gridManager)
        {
            var currentGridPosition = gridManager.WorldPositionToGridPosition(gridActor.Transform.position);
            var cellsInRange = gridManager.GetUnoccupiedCellsInRange(currentGridPosition, range, gridActor.References.NavigationManager);
            return cellsInRange.Keys.Where(cell => cell.IsFree);
        }
    }
}
