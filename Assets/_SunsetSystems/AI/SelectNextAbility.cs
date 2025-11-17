using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using SunsetSystems.Utils.Extensions;
using System.Linq;
using SunsetSystems.Abilities;

[TaskCategory("Turn System")]
public class SelectNextAbility : Action
{
    [SerializeField, SharedRequired]
    private SharedAIContext _aiContext;

    public override void OnStart()
	{
        _aiContext.Value.ReloadAmmo();
        var randomAbility = _aiContext.Value.GetAbilityUser()
                                            .GetAllAbilities()
                                            .Select(abilityRuntime => abilityRuntime.AbilityConfig)
                                            .Where(ability => ability.GetCategories().HasFlag(AbilityCategory.Movement) is false)
                                            .Where(ability => ability.GetCategories().HasFlag(AbilityCategory.Support) is false)
                                            .GetRandom();
		_aiContext.Value.SelectedAbility = randomAbility;
	}

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}