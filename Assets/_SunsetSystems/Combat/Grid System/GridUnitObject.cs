using Pathfinding;
using Sirenix.OdinInspector;
using SunsetSystems.Abilities;
using SunsetSystems.Core;
using SunsetSystems.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SunsetSystems.Combat.Grid
{
    public class GridUnitObject : SerializedMonoBehaviour, IGridCell, ITargetable
    {
        [SerializeField]
        private BoxCollider cellCollider;
        [SerializeField]
        private MeshRenderer cellRenderer;
        [SerializeField]
        private Dictionary<GridCellStateData, IMaterialConfig> gridCellMaterialConfigs = new();
        [ShowInInspector, ReadOnly]
        private GridUnit unitData = null;
        [ShowInInspector, ReadOnly]
        private GridCellStateData currentCellState;

        private IMaterialConfig defaultCellStateConfig;
        private static MaterialPropertyNameAndTypeComparer propertyNameAndTypeComparer = new();

        public GraphNode GraphNode => unitData.GraphNode;
        public Vector3Int GridPosition => unitData.GridPosition;
        public Vector3 WorldPosition => transform.position;
        public Transform ProjectileTarget => transform;

        public bool IsOccupied => unitData.IsOccupied;
        public bool IsFree => unitData.IsFree;
        public float CellSize => unitData.CellSize;
        public bool Highlighted => unitData.Highlighted;

        private ITargetableContext _context;
        public ITargetableContext GetContext() => _context;

        private void Awake()
        {
            _context = new GridUnitContext(this, gameObject);
        }

        private void Start()
        {
            defaultCellStateConfig = gridCellMaterialConfigs[new() { BaseState = GridCellBaseState.Default, SubState = GridCellSubState.Default }];
            propertyNameAndTypeComparer ??= new();
        }

        public bool InjectUnitData(GridUnit unitData)
        {
            if (this.unitData == null)
            {
                this.unitData = unitData;
                cellCollider.size = new Vector3(unitData.CellSize, cellCollider.size.y, unitData.CellSize);
                Vector3 cellPosition = unitData.WorldPosition;
                transform.localPosition = transform.InverseTransformPoint(cellPosition);
                cellRenderer.transform.localScale = Vector3.one * unitData.CellSize;
                UpdateCellState();
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UpdateCellState()
        {
            currentCellState = CellStateEvaluator.EvaluateState(unitData);
            if (gridCellMaterialConfigs.TryGetValue(currentCellState, out IMaterialConfig value))
            {
                SetCellMaterialParams(value.PropertyOverrides);
            }
        }

        private void SetCellMaterialParams(IEnumerable<MaterialPropertyData> propertyData, bool useSharedMaterial = false)
        {
            Material mat;
            if (useSharedMaterial)
                mat = cellRenderer.sharedMaterial;
            else
                mat = cellRenderer.material;
            foreach (MaterialPropertyData data in propertyData.Union(defaultCellStateConfig.PropertyOverrides, propertyNameAndTypeComparer))
            {
                switch (data.PropertyType)
                {
                    case MaterialPropertyType.Float:
                        mat.SetFloat(data.PropertyName, data.GetValue<float>());
                        break;
                    case MaterialPropertyType.Int:
                        mat.SetInteger(data.PropertyName, data.GetValue<int>());
                        break;
                    case MaterialPropertyType.Vector:
                        mat.SetVector(data.PropertyName, data.GetValue<Vector4>());
                        break;
                    case MaterialPropertyType.Matrix:
                        mat.SetMatrix(data.PropertyName, data.GetValue<Matrix4x4>());
                        break;
                    case MaterialPropertyType.Texture:
                        mat.SetTexture(data.PropertyName, data.GetValue<Texture>());
                        break;
                    default:
                        Debug.LogError($"Invalid MaterialPropretyType {Enum.GetName(typeof(MaterialPropertyType), data.PropertyType)}!");
                        break;
                }
            }
        }

        [Button]
        public void ForceInjectMaterialDataFromConfig(IMaterialConfig config)
        {
            SetCellMaterialParams(config.PropertyOverrides, true);
        }

        [Button]
        public void FillUnitStateDictionary()
        {
            foreach (GridCellBaseState baseState in Enum.GetValues(typeof(GridCellBaseState)))
            {
                foreach (GridCellSubState subState in Enum.GetValues(typeof(GridCellSubState)))
                {
                    GridCellStateData data = new()
                    {
                        BaseState = baseState,
                        SubState = subState,
                    };
                    gridCellMaterialConfigs.TryAdd(data, null);
                }
            }
        }

        public bool IsValidTarget(ICombatant caster, TargetableEntityType validTargetsFlag)
        {
            return validTargetsFlag.HasFlag(TargetableEntityType.Position);
        }

        public struct GridCellStateData
        {
            public GridCellBaseState BaseState;
            public GridCellSubState SubState;

            public GridCellStateData(GridCellBaseState BaseState, GridCellSubState SubState)
            {
                this.BaseState = BaseState;
                this.SubState = SubState;
            }
        }

        public enum GridCellBaseState
        {
            Default, Highlight, Walkable, Sprintable
        }

        public enum GridCellSubState
        {
            Default, Hostile, Friendly, HalfCover, FullCover
        }

        private static class CellStateEvaluator
        {
            public static GridCellStateData EvaluateState(GridUnit gridUnit)
            {
                GridCellStateData result = new();
                EvaluateBaseState(gridUnit, ref result);
                EvaluateSubState(gridUnit, ref result);
                return result;

                static void EvaluateBaseState(GridUnit gridUnit, ref GridCellStateData result)
                {
                    result.BaseState = GridCellBaseState.Default;
                    if (gridUnit.Walkable)
                    {
                        if (gridUnit.IsInSprintRange)
                            result.BaseState = GridCellBaseState.Sprintable;
                        if (gridUnit.IsInMoveRange)
                            result.BaseState = GridCellBaseState.Walkable;
                    }
                    if (gridUnit.Highlighted)
                        result.BaseState = GridCellBaseState.Highlight;
                }

                static void EvaluateSubState(GridUnit gridUnit, ref GridCellStateData result)
                {
                    result.SubState = GridCellSubState.Default;
                    if (gridUnit.AdjacentToCover)
                    {
                        switch (gridUnit.CoverQuality)
                        {
                            case CoverQuality.Half:
                                result.SubState = GridCellSubState.HalfCover;
                                break;
                            case CoverQuality.Full:
                                result.SubState = GridCellSubState.FullCover;
                                break;
                        }
                    }
                    if (gridUnit.IsOccupied)
                    {
                        if (gridUnit.Occupier is IFactionMember factionMember && factionMember.GetFaction() is Faction.Hostile)
                            result.SubState = GridCellSubState.Hostile;
                        else
                            result.SubState = GridCellSubState.Friendly;
                    }
                }
            }
        }

        private class MaterialPropertyNameAndTypeComparer : EqualityComparer<MaterialPropertyData>
        {
            public override bool Equals(MaterialPropertyData first, MaterialPropertyData second)
            {
                return first.PropertyName == first.PropertyName && second.PropertyType == second.PropertyType;
            }

            public override int GetHashCode(MaterialPropertyData obj)
            {
                return (obj.PropertyName, obj.PropertyType).GetHashCode();
            }
        }

        private class GridUnitContext : ITargetableContext
        {
            private readonly GameObject _go;
            private readonly IGridCell _gridCell;
            public GameObject GameObject => _go;
            public Transform Transform => _go.transform;
            public Vector3Int GridPosition => _gridCell.GridPosition;

            public GridUnitContext(IGridCell gridCell, GameObject go)
            {
                _go = go;
                _gridCell = gridCell;
            }
        }
    }
}
