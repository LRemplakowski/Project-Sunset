using SunsetSystems.Combat;

namespace SunsetSystems.Abilities.Targeting
{

    public class SelfTargetStrategy : AbstractTargetingStrategy
    {
        public SelfTargetStrategy(IAbilityConfig abilityConfig) : base(abilityConfig)
        {

        }

        public override void ExecuteSetTargetLock(ITargetingContext context)
        {

        }

        public override void ExecuteClearTargetLock(ITargetingContext context)
        {
            
        }

        public override void ExecutePointerPosition(ITargetingContext context)
        {

        }

        public override void ExecuteTargetingBegin(ITargetingContext context)
        {
            context.TargetUpdateDelegate().Invoke(context.GetSelfTarget());
            context.TargetingLineUpdateDelegate().Invoke(false);
            context.TargetLockSetDelegate().Invoke(true);
            var executionUI = context.GetExecutionUI();
            executionUI.RegisterConfirmationCallback(TriggerExecution);
            executionUI.UpdateShowInterface(true, () => context.CanExecuteAbility(_ability));
            if (IsDangerous(_ability, context.GetAbilityContext()))
            {
                if (_ability is IAOEAbility aoe)
                {
                    HighlightDanger(context, context.GetSelfTarget(), aoe.GetAOERadius(context.GetAbilityContext()));
                }
                else
                {
                    HighlightDanger(context, context.GetSelfTarget(), 0);
                }
            }
            base.ExecuteTargetingBegin(context);
        }

        public override void ExecuteTargetingEnd(ITargetingContext context)
        {
            context.TargetUpdateDelegate().Invoke(null);
            context.TargetingLineUpdateDelegate().Invoke(false);
            context.TargetLockSetDelegate().Invoke(false);
            var executionUI = context.GetExecutionUI();
            executionUI.UnregisterConfirmationCallback(TriggerExecution);
            executionUI.UpdateShowInterface(false, () => false);
            ClearDangerHighlight(context);
            base.ExecuteTargetingEnd(context);
        }

        private static bool IsDangerous(IAbilityConfig abilityConfig, IAbilityContext context)
        {
            var abilityCategory = abilityConfig.GetCategories();
            return (abilityCategory & AbilityCategory.Dangerous) != 0;
        }

        private static void HighlightDanger(ITargetingContext context, ITargetable target, int radius)
        {
            var grid = context.GetCurrentGrid();
            var gridPosition = target.GetContext().GridPosition;
            grid.MarkCellsDangerous(true, gridPosition, radius);
        }

        private static void ClearDangerHighlight(ITargetingContext context)
        {
            context.GetCurrentGrid().ClearDangerousCells();
        }
    }
}
