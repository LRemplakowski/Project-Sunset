using SunsetSystems.Combat;
using SunsetSystems.Entities.Characters;
using SunsetSystems.Inventory;
using UnityEngine;

namespace SunsetSystems.Abilities.Targeting
{
    public class TargetCreatureStrategy : AbstractTargetingStrategy
    {
        private const float TARGETING_RANGE_MARGIN = .5f;
        private static readonly Color TargetInRangeColor = Color.red;
        private static readonly Color TargetOutOfRangeColor = Color.gray;

        public TargetCreatureStrategy(IAbilityConfig ability) : base(ability)
        {

        }

        public override void ExecuteSetTargetLock(ITargetingContext context)
        {
            if (ValidateTarget(_ability, context, out ITargetable target) is false)
            {
                ClearTargetingDelegates(context);
                DisableExecutionUI(context);
                return;
            }
            ICombatant current = context.GetSelf();
            context.TargetUpdateDelegate().Invoke(target);
            var abilityRange = _ability.GetTargetingData(context.GetAbilityContext()).GetRangeData();
            bool isTargetInRange = IsTargetInRange(current, target, in abilityRange);
            current.References.NavigationManager.FaceDirectionAfterMovementFinished(target.GetContext().Transform.position);
            context.TargetLockSetDelegate().Invoke(true);
            var executionUI = context.GetExecutionUI();
            if (isTargetInRange)
            {
                if (CanShowTargetingLine(in abilityRange))
                    ShowTargetingLine(context, target.ProjectileTarget.position, TargetInRangeColor);
                executionUI.RegisterConfirmationCallback(TriggerExecution);
                executionUI.UpdateShowInterface(true, () => context.CanExecuteAbility(_ability));
            }
            else
            {
                if (CanShowTargetingLine(in abilityRange))
                    ShowTargetingLine(context, target.ProjectileTarget.position, TargetOutOfRangeColor);
                executionUI.UnregisterConfirmationCallback(TriggerExecution);
                executionUI.UpdateShowInterface(true, () => false);
            }
        }

        public override void ExecuteClearTargetLock(ITargetingContext context)
        {
            ClearTargetingDelegates(context);
            DisableExecutionUI(context);
        }

        public override void ExecutePointerPosition(ITargetingContext context)
        {
            if (ValidateTarget(_ability, context, out ITargetable target) is false)
            {
                ClearTargetingDelegates(context);
                return;
            }
            context.TargetUpdateDelegate().Invoke(target);
            var abilityRange = _ability.GetTargetingData(context.GetAbilityContext()).GetRangeData();
            ICombatant current = context.GetSelf();
            current.References.NavigationManager.FaceDirectionAfterMovementFinished(target.GetContext().Transform.position);
            if (CanShowTargetingLine(in abilityRange))
            {
                var targetingLineColor = IsTargetInRange(current, target, in abilityRange) ? TargetInRangeColor : TargetOutOfRangeColor;
                ShowTargetingLine(context, target.ProjectileTarget.position, in targetingLineColor);
            }
        }

        public override void ExecuteTargetingBegin(ITargetingContext context)
        {
            ClearTargetingDelegates(context);
            DisableExecutionUI(context);
            context.GetTargetingLineRenderer().SetPosition(0, context.GetSelf().AimingOrigin);
            base.ExecuteTargetingBegin(context);

        }

        public override void ExecuteTargetingEnd(ITargetingContext context)
        {
            ClearTargetingDelegates(context);
            DisableExecutionUI(context);
            base.ExecuteTargetingEnd(context);
        }

        private void ShowTargetingLine(ITargetingContext context, in Vector3 target, in Color lineColor)
        {
            var targetingLineRenderer = context.GetTargetingLineRenderer();
            targetingLineRenderer.SetPosition(1, target);
            targetingLineRenderer.startColor = lineColor;
            targetingLineRenderer.endColor = lineColor;
            context.TargetingLineUpdateDelegate().Invoke(true);
        }

        private void ClearTargetingDelegates(ITargetingContext context)
        {
            context.TargetUpdateDelegate().Invoke(null);
            context.TargetingLineUpdateDelegate().Invoke(false);
            context.TargetLockSetDelegate().Invoke(false);
        }

        private void DisableExecutionUI(ITargetingContext context)
        {
            var executionUI = context.GetExecutionUI();
            executionUI.UnregisterConfirmationCallback(TriggerExecution);
            executionUI.UpdateShowInterface(false, () => false);
        }

        private static bool ValidateTarget(IAbilityConfig ability, ITargetingContext context, out ITargetable target)
        {
            target = default;
            var collider = context.GetLastRaycastCollider();
            if (collider == null)
                return false;
            if (collider.TryGetComponent(out ICreature targetCreature) is false)
                return false;
            target = targetCreature.References.Targetable;
            return target.IsValidTarget(ability.GetTargetingData(context.GetAbilityContext()).GetValidEntityTypesFlag());
        }

        private static bool IsTargetInRange(ICombatant attacker, ITargetable target, in RangeData abilityRange)
        {
            Vector3Int attackerPosition = attacker.GetContext().GridPosition;
            Vector3Int targetPosition = target.GetContext().GridPosition;
            var gridDistance = Vector3Int.Distance(attackerPosition, targetPosition);
            bool result = gridDistance <= abilityRange.MaxRange + TARGETING_RANGE_MARGIN;
            return result;
        }

        private static bool CanShowTargetingLine(in RangeData abilityRange) => abilityRange.MaxRange > 1;
    }
}
