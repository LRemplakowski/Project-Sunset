using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;


[TaskCategory("Turn System/")]
public class CanMove : Conditional
{
	[SerializeField, SharedRequired]
	private SharedAIContext _aiContext;
    [SerializeField, SharedRequired]
    private SharedBool _hasMoved;

	public override TaskStatus OnUpdate()
	{
        return HasEnoughMovementPoints() && !_hasMoved.Value ? TaskStatus.Success : TaskStatus.Failure;
    }

    private bool HasEnoughMovementPoints()
    {
        return _aiContext.Value.CanMove();
    }
}