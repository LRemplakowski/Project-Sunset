using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Pathfinding;
using SunsetSystems.Combat.Grid;
using UnityEngine;

namespace SunsetSystems.Entities.Characters.Navigation
{
    public interface INavigationManager
    {
        public const float DESTINATION_REACHED_THRESHOLD = 0.25f;
        bool FinishedCurrentPath { get; }
        bool IsMoving { get; }
        Vector3 Position { get; }
        float CurrentSpeed { get; }
        float MaxSpeed { get; }
        float RemainingDistance { get; }

        bool Warp(Vector3 position, Quaternion rotation);
        UniTask<ABPath> CalculatePathAsync(Vector3 targetPosition);
        bool CalculatePath(Vector3 targetPosition, out ABPath path);
        UniTask<Dictionary<Vector3, float>> CalculateMultiplePathsAsync(Vector3[] targetPosition);
        Dictionary<Vector3, float> CalculateMultiplePaths(Vector3[] targetPositions);
        void FaceDirectionAfterMovementFinished(Vector3 point);
        bool SetNavigationTarget(Vector3 target);
        bool SetGridTarget(IGridCell gridCell);
        void StopMovement(bool forceStopImmediate = false);
        void SetNavigationEnabled(bool enabled);
        Vector3Int GetGridPosition(GridManager grid);
    }
}
