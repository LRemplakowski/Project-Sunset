using Sirenix.OdinInspector;
using SunsetSystems.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SunsetSystems.Combat.Grid
{
    public class GridUnitObject : SerializedMonoBehaviour
    {
        [SerializeField]
        private BoxCollider cellCollider;
        [SerializeField]
        private MeshRenderer cellRenderer;
        [SerializeField]
        private Dictionary<GridCellStateData, IMaterialConfig> gridCellMaterialConfigs = new();
        [ShowInInspector, ReadOnly]
        private GridUnit unitData = null;

        private GridCellStateData defaultState = new();
        private GridCellBaseState previousState = GridCellBaseState.Default;
        private GridCellBaseState currentState = GridCellBaseState.Default;
        public GridCellBaseState CurrentCellState => currentState;

        public Vector3 WorldPosition => transform.position + new Vector3(0, unitData.SurfaceY - transform.position.y, 0);

        public bool InjectUnitData(GridUnit unitData)
        {
            if (this.unitData == null)
            {
                this.unitData = unitData;
                cellCollider.size = new Vector3(unitData.CellSize, 0.1f, unitData.CellSize);
                Vector3 worldPosition = transform.TransformPoint(unitData.X, unitData.Y, unitData.Z);
                worldPosition.y = unitData.SurfaceY;
                transform.position = worldPosition;
                defaultState = new()
                {
                    BaseState = GridCellBaseState.Default,
                    SubState = GridCellSubState.Default,
                };
                SetGridCellState(defaultState, false);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetGridCellState(GridCellStateData stateData, bool cachePrevious = false)
        {
            SetGridCellState(stateData.BaseState, stateData.SubState, cachePrevious);
        }

        public void SetGridCellState(GridCellBaseState state, GridCellSubState subState = GridCellSubState.Default, bool cachePrevious = false)
        {
            if (cachePrevious)
                previousState = currentState;
            currentState = state;
            if (gridCellMaterialConfigs.TryGetValue(new(state, subState), out IMaterialConfig value))
                SetCellMaterialParams(value.PropertyOverrides);
        }

        private void SetCellMaterialParams(IEnumerable<MaterialPropertyData> propertyData, bool useSharedMaterial = false)
        {
            Material mat;
            if (useSharedMaterial)
                mat = cellRenderer.sharedMaterial;
            else
                mat = cellRenderer.material;
            foreach (MaterialPropertyData data in propertyData)
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

        public void RestoreCachedPreviousVisualState()
        {
            SetGridCellState(previousState);
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
    }
}
