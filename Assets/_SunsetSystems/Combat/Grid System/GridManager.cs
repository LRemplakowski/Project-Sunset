using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using Sirenix.OdinInspector;
using SunsetSystems.Entities.Characters;
using SunsetSystems.Entities.Characters.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace SunsetSystems.Combat.Grid
{
    public class GridManager : SerializedMonoBehaviour
    {
        public GridUnit this[int x, int y, int z]
        {
            get => managedGrid[x, y, z];
        }

        public GridUnit this[Vector3Int position]
        {
            get => managedGrid[position];
        }

        [SerializeField]
        private CachedMultiLevelGrid managedGrid;

        private GridUnit currentlyHighlightedCell;
        private readonly List<GridUnit> currentlyHighlitedGridUnits = new();
        private readonly Dictionary<ICombatant, GridUnit> _occupiedGridCells = new();

        public float GetGridScale() => managedGrid.GridCellSize;

        public Vector3 GridPositionToWorldPosition(Vector3Int gridPosition) => managedGrid.GridPositionToWorldPosition(gridPosition);
        public Vector3Int WorldPositionToGridPosition(Vector3 worldPosition) => managedGrid.WorldPositionToGridPosition(worldPosition);

        public async void EnableGrid()
        {
            await managedGrid.EnableGrid();
        }

        public void DisableGrid()
        {
            managedGrid.DisableGrid();
        }

        public void HighlightCell(IGridCell gridCell)
        {
            ClearHighlightedCell();
            if (gridCell == null)
                return;
            GridUnit cellData = this[gridCell.GridPosition];
            cellData.Highlighted = true;
            currentlyHighlightedCell = cellData;
            managedGrid.MarkCellDirty(currentlyHighlightedCell);
        }

        public void HandleCombatantMovedIntoGridCell(ICombatant combatant, IGridCell cell)
        {
            GridUnit cellObject = this[cell.GridPosition];
            cellObject.Occupier = combatant;
            if (_occupiedGridCells.TryAdd(combatant, cellObject))
            {
                combatant.References.StatsManager.OnCreatureDied += OnOccupierDied;
            }
            else
            {
                Debug.LogError($"{nameof(GridManager)} >>> Combatant {combatant} moved into already occupied grid cell! This should not happen! Grid Cell: {cell.GridPosition}");
            }
            managedGrid.MarkCellDirty(cellObject);
        }

        private void OnOccupierDied(ICreature creature)
        {
            var combatant = creature.References.CombatBehaviour;
            if (_occupiedGridCells.TryGetValue(combatant, out var gridCell))
            {
                ClearOccupierFromCell(gridCell);
            }
            else
            {
                creature.References.StatsManager.OnCreatureDied -= OnOccupierDied;
                Debug.LogWarning($"Combatant {combatant} died, but it had no assigned grid cell! This might be a bug!");
            }
        }

        public bool TryGetCurrentGridCell(ICombatant occupier, out IGridCell cell)
        {
            cell = default;
            if (_occupiedGridCells.TryGetValue(occupier, out GridUnit gridUnit))
            {
                cell = gridUnit;
                return true;
            }
            return false;
        }

        public void ClearOccupierFromCell(IGridCell cell)
        {
            var gridUnit = this[cell.GridPosition];
            var occupier = gridUnit.Occupier;
            gridUnit.Occupier = null;
            if (occupier != null)
                occupier.References.StatsManager.OnCreatureDied -= OnOccupierDied;
            _occupiedGridCells.Remove(occupier);
            managedGrid.MarkCellDirty(cell);
        }

        public void ClearHighlightedCell()
        {
            if (currentlyHighlightedCell == null)
                return;
            currentlyHighlightedCell.Highlighted = false;
            managedGrid.MarkCellDirty(currentlyHighlightedCell);
            currentlyHighlightedCell = null;
        }

        public Dictionary<GridUnit, float> GetUnoccupiedCellsInRange(Vector3Int gridPosition, float range, INavigationManager agent)
        {
            // Calculate the maximum grid distance within _range
            float maxGridDistance = range * managedGrid.GridCellSize;
            var unitsInRange = managedGrid.GetAllWalkableGridUnits()
                .Where(unit => GetDistance(gridPosition, unit.GridPosition) <= maxGridDistance)
                .Where(unit => unit.IsFree || unit.Occupier.GetContext().MovementManager == agent)
                .ToArray();
            var pathDistances = agent.CalculateMultiplePaths(unitsInRange.Select(unit => unit.WorldPosition).ToArray());
            Dictionary<GridUnit, float> result = new();
            foreach (var unit in unitsInRange)
            {
                if (pathDistances.TryGetValue(unit.WorldPosition, out float distance))
                {
                    if (distance <= maxGridDistance)
                    {
                        result.Add(unit, distance);
                    }
                }
            }
            return result;
        }

        private float GetDistance(in Vector3Int from, in Vector3Int to)
        {
            return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.z - to.z);
        }

        public void ShowCellsInMovementRange(ICombatant combatant)
        {
            HideCellsInMovementRange();
            var mover = combatant.GetContext().MovementManager;
            var apManager = combatant.GetContext().ActionPointManager;
            if (mover.GetCanMove() == false)
                return;
            Vector3Int gridPosition = combatant.References.NavigationManager.GetGridPosition(this);
            var navigationManager = combatant.References.NavigationManager;
            currentlyHighlitedGridUnits.Clear();
            int currentMovementPoints = mover.GetCurrentMovementPoints();
            var gridDistancesMap = GetUnoccupiedCellsInRange(gridPosition, currentMovementPoints + (managedGrid.GridCellSize / 2), navigationManager);
            currentlyHighlitedGridUnits.AddRange(gridDistancesMap.Keys);
            foreach (GridUnit unit in currentlyHighlitedGridUnits)
            {
                unit.IsInMoveRange = true;
                managedGrid.MarkCellDirty(unit);
                float distance = gridDistancesMap[unit];
                if (distance >= currentMovementPoints / 2 + (managedGrid.GridCellSize / 2) || apManager.GetCurrentActionPoints() < apManager.GetMaxActionPoints())
                {
                    unit.IsInSprintRange = true;
                    managedGrid.MarkCellDirty(unit);
                }
            }
        }

        public void HideCellsInMovementRange()
        {
            foreach (GridUnit unit in currentlyHighlitedGridUnits)
            {
                unit.IsInSprintRange = false;
                unit.IsInMoveRange = false;
                managedGrid.MarkCellDirty(unit);
            }
        }

        public GridUnitObject GetNearestWalkableGridCell(Vector3 position)
        {
            Vector3Int gridPosition = GetNearestWalkableGridPosition(position);
            return managedGrid.GetCellGameObject(gridPosition);
        }

        public Vector3Int GetNearestWalkableGridPosition(Vector3 position, bool includeOccupied = true)
        {
            Vector3Int bestGridPos = WorldPositionToGridPosition(position);
            GridUnit unit = this[bestGridPos.x, bestGridPos.y, bestGridPos.z];
            if (unit.Walkable && (includeOccupied || !unit.IsOccupied))
            {
                return bestGridPos;
            }
            else
            {
                unit = CrawlForNearestWalkablePosition(unit, managedGrid, includeOccupied);
                return unit.GridPosition;
            }

            static GridUnit CrawlForNearestWalkablePosition(GridUnit relativeTo, CachedMultiLevelGrid managedGrid, bool includeOccupied)
            {
                IEnumerable<GridUnit> allWalkableUnits = managedGrid.GetAllWalkableGridUnits();
                GridUnit result = allWalkableUnits.FirstOrDefault();
                float gridDistance = float.MaxValue;
                foreach (GridUnit gridUnit in allWalkableUnits)
                {
                    if (!includeOccupied && gridUnit.IsOccupied)
                    {
                        continue;
                    }
                    float newDistance = Mathf.Abs((gridUnit.GridPosition - relativeTo.GridPosition).magnitude);
                    if (newDistance < gridDistance)
                    {
                        gridDistance = newDistance;
                        result = gridUnit;
                    }
                }
                return result;
            }
        }
    }
}
