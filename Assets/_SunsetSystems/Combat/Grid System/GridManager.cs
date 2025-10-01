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

        public List<GridUnit> GetCellsInRange(Vector3Int gridPosition, float range, INavigationManager agent, out Dictionary<GridUnit, float> distanceToUnitDictionary)
        {
            distanceToUnitDictionary = new();
            List<GridUnit> unitsInRange = new();
            // Calculate the maximum grid distance within _range
            float maxGridDistance = range * managedGrid.GridCellSize;

            foreach (GridUnit unit in managedGrid.GetAllWalkableGridUnits())
            {
                // Calculate the grid distance between gridPosition and unit's position
                float gridDistance = (new Vector2(gridPosition.x, gridPosition.z) - new Vector2(unit.GridPosition.x, unit.GridPosition.z)).magnitude;

                if (gridDistance <= maxGridDistance)
                {
                    Vector3 unitWorldPosition = GridPositionToWorldPosition(unit.GridPosition);
                    if (agent.CalculatePath(unitWorldPosition, out ABPath path))
                    {
                        // Calculate path length
                        float pathLength = path.GetTotalLength();
                        if (pathLength <= maxGridDistance)
                        {
                            unitsInRange.Add(unit);
                            distanceToUnitDictionary[unit] = pathLength;
                        }
                    }
                }
            }
            return unitsInRange;
        }

        public void ShowCellsInMovementRange(ICombatant combatant)
        {
            HideCellsInMovementRange();
            var mover = combatant.GetContext().MovementManager;
            Vector3Int gridPosition = WorldPositionToGridPosition(combatant.References.Transform.position);
            var navigationManager = combatant.References.NavigationManager;
            currentlyHighlitedGridUnits.Clear();
            currentlyHighlitedGridUnits.AddRange(GetCellsInRange(gridPosition, mover.GetCurrentMovementPoints() + (managedGrid.GridCellSize / 2), navigationManager, out Dictionary<GridUnit, float> distanceToUnitDictionary));
            foreach (GridUnit unit in currentlyHighlitedGridUnits)
            {
                float distanceToUnit = distanceToUnitDictionary[unit];
                if (distanceToUnit <= mover.GetCurrentMovementPoints() + (managedGrid.GridCellSize / 2) && mover.GetCanMove())
                {
                    unit.IsInMoveRange = true;
                    managedGrid.MarkCellDirty(unit);
                }
                //if (distanceToUnit <= combatant.SprintRange + (managedGrid.GridCellSize / 2) && !combatant.HasMoved && !combatant.HasActed)
                //{
                //    unit.IsInSprintRange = true;
                //    managedGrid.MarkCellDirty(unit);
                //}
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
