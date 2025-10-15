using System;
using SunsetSystems.Combat;

namespace SunsetSystems.Abilities.Targeting
{
    public class SelfTargetStrategy : IAbilityTargetingStrategy
    {
        private IAbilityConfig _ability;

        public event Action OnExecutionTriggered;

        public SelfTargetStrategy(IAbilityConfig abilityConfig)
        {
            _ability = abilityConfig;
        }

        public void ExecuteSetTargetLock(ITargetingContext context)
        {

        }

        public void ExecuteClearTargetLock(ITargetingContext context)
        {
            
        }

        public void ExecutePointerPosition(ITargetingContext context)
        {

        }

        public void ExecuteTargetingBegin(ITargetingContext context)
        {
            context.TargetUpdateDelegate().Invoke(context.GetSelfTarget());
            context.TargetingLineUpdateDelegate().Invoke(false);
            context.TargetLockSetDelegate().Invoke(true);
            var executionUI = context.GetExecutionUI();
            executionUI.RegisterConfirmationCallback(TriggerExecution);
            executionUI.UpdateShowInterface(true, () => context.CanExecuteAbility(_ability));
        }

        public void ExecuteTargetingEnd(ITargetingContext context)
        {
            context.TargetUpdateDelegate().Invoke(null);
            context.TargetingLineUpdateDelegate().Invoke(false);
            context.TargetLockSetDelegate().Invoke(false);
            var executionUI = context.GetExecutionUI();
            executionUI.UnregisterConfirmationCallback(TriggerExecution);
            executionUI.UpdateShowInterface(false, () => false);
        }

        private void TriggerExecution()
        {
            OnExecutionTriggered?.Invoke();
        }
    }
}
