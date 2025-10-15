using System;
using SunsetSystems.Combat;
using SunsetSystems.Combat.Grid;
using UnityEngine;

namespace SunsetSystems.Abilities.Targeting
{
    public class DoubleClickGridStrategy : IAbilityTargetingStrategy
    {
        private readonly IAbilityConfig _ability;
        private Collider _cachedLastHit;

        public event Action OnExecutionTriggered;

        public DoubleClickGridStrategy(IAbilityConfig ability)
        {
            _ability = ability;
        }

        public void ExecuteSetTargetLock(ITargetingContext context)
        {
            Collider hitCollider = context.GetLastRaycastCollider();
            context.GetCurrentGrid().ClearHighlightedCell();
            if (context.IsPointerOverUI() || IsColliderNull(hitCollider) || !hitCollider.TryGetComponent(out GridUnitObject gridCell))
                return;
            if (IsCurrentTarget(hitCollider) && context.IsTargetLocked())
            {
                OnExecutionTriggered?.Invoke();
            }
            else
            {
                _cachedLastHit = hitCollider;
                context.GetCurrentGrid().HighlightCell(gridCell);
                context.TargetUpdateDelegate().Invoke(gridCell);
                context.TargetLockSetDelegate().Invoke(true);
            }
        }

        public void ExecuteClearTargetLock(ITargetingContext context)
        {
            context.TargetLockSetDelegate().Invoke(false);
        }

        public void ExecutePointerPosition(ITargetingContext context)
        {
            Collider hitCollider = context.GetLastRaycastCollider();
            context.GetCurrentGrid().ClearHighlightedCell();
            if (context.IsPointerOverUI() || IsColliderNull(hitCollider) || !hitCollider.TryGetComponent(out GridUnitObject gridCell))
            {
                context.TargetUpdateDelegate().Invoke(null);
                context.TargetingLineUpdateDelegate().Invoke(false);
            }
            else
            {
                context.GetCurrentGrid().HighlightCell(gridCell);
                context.TargetUpdateDelegate().Invoke(gridCell);
                context.TargetingLineUpdateDelegate().Invoke(false);
            }
        }

        public void ExecuteTargetingBegin(ITargetingContext context)
        {
            context.GetCurrentGrid().ShowCellsInMovementRange(context.GetSelf());
            context.TargetUpdateDelegate().Invoke(null);
            context.TargetingLineUpdateDelegate().Invoke(false);
            context.TargetLockSetDelegate().Invoke(false);
            context.GetExecutionUI().UpdateShowInterface(false, () => false);
        }

        public void ExecuteTargetingEnd(ITargetingContext context)
        {
            context.GetCurrentGrid().HideCellsInMovementRange();
            context.TargetUpdateDelegate().Invoke(null);
            context.TargetingLineUpdateDelegate().Invoke(false);
            context.TargetLockSetDelegate().Invoke(false);
            context.GetExecutionUI().UpdateShowInterface(false, () => false);
        }


        private bool IsCurrentTarget(Collider target) => _cachedLastHit == target;
        private static bool IsColliderNull(Collider collider) => collider == null;
    }
}