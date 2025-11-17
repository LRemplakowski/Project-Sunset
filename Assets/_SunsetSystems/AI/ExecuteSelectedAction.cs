using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Turn System")]
public class ExecuteSelectedAction : Action
{
    [SerializeField, SharedRequired]
    private SharedAIContext _aiContext;
	[SerializeField, SharedRequired]
	private SharedBool _hasActed;

	private bool _executionFailed = false;

    public override void OnStart()
	{
		var context = _aiContext.Value;
		context.GetAbilityUser().SetCurrentTargetObject(context.SelectedTarget);
        _executionFailed = !context.GetAbilityUser().ExecuteAbility(context.SelectedAbility, OnExecutionFinished);
	}

	public override TaskStatus OnUpdate()
	{
		if (_executionFailed)
		{
			return TaskStatus.Failure;
		}
		if (_hasActed.Value)
        {
            return TaskStatus.Success;
        }
		else
		{
			return TaskStatus.Running;
		}
    }

	private void OnExecutionFinished()
	{
		_hasActed.Value = true;
    }
}