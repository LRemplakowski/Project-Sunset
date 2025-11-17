using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Turn System")]
public class CanAct : Conditional
{
    [SerializeField, SharedRequired]
    private SharedAIContext _aiContext;
	[SerializeField, SharedRequired]
	private SharedBool _hasActed;

    public override TaskStatus OnUpdate()
	{
        return HasEnoughActionPoints() && !_hasActed.Value ? TaskStatus.Success : TaskStatus.Failure;
    }

	private bool HasEnoughActionPoints()
	{
		return _aiContext.Value.GetHasEnoughActionPoints(_aiContext.Value.SelectedAbility);
	}
}