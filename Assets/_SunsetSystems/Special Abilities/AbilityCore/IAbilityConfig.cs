using System;
using SunsetSystems.Core.Database;

namespace SunsetSystems.Abilities
{
    public interface IAbilityConfig : IDatabaseEntry<IAbilityConfig>
    {
        Guid AbilityID { get; }
        IAbilityTargetingData GetTargetingData(IAbilityContext context);
        IAbilityCostData GetAbilityCosts(IAbilityContext context);
        IAbilityUIData GetAbilityUIData();
        AbilityCategory GetCategories();

        bool IsContextValidForExecution(IAbilityContext context);

        IAbilityExecutionStrategy GetExecutionStrategy();
        IAbilityTargetingStrategy GetTargetingStrategy();
    }
}
