using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Turn System")]
public class IsCurrentTargetInRange : Conditional
{
    [SerializeField, SharedRequired]
    private SharedAIContext _aiContext;
    [SerializeField, SharedRequired]
    private SharedBool _hasActed;

    public override TaskStatus OnUpdate()
	{
        return IsTargetInRange() && !_hasActed.Value ? TaskStatus.Success : TaskStatus.Failure;
	}

    private bool IsTargetInRange()
    {
        return _aiContext.Value.IsCurrentTargetInAbilityRange();
    }
}