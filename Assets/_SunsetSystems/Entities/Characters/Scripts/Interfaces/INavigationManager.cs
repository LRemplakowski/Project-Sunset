using Pathfinding;
using SunsetSystems.Combat.Grid;
using UnityEngine;
using UnityEngine.AI;

namespace SunsetSystems.Entities.Characters.Navigation
{
    public interface INavigationManager
    {
        bool FinishedCurrentPath { get; }
        bool IsMoving { get; }
        Vector3 Position { get; }
        float CurrentSpeed { get; }
        float MaxSpeed { get; }

        bool Warp(Vector3 position);
        bool CalculatePath(Vector3 targetPosition, out ABPath path);
        void FaceDirectionAfterMovementFinished(Vector3 point);
        bool SetNavigationTarget(Vector3 target);
        bool SetGridTarget(IGridCell gridCell);
        void StopMovement(bool forceStopImmediate = false);
        void SetNavigationEnabled(bool enabled);
    }
}
