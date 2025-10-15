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
            base.ExecuteTargetingEnd(context);
        }
    }
}
