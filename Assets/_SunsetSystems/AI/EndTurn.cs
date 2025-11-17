using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

[TaskCategory("Turn System")]
public class EndTurn : Action
{
    [SerializeField, SharedRequired]
    private SharedAIContext _aiContext;
	[SerializeField, SharedRequired]
	private SharedBool _hasMoved;
    [SerializeField, SharedRequired]
    private SharedBool _hasActed;

    public override void OnStart()
	{
		_aiContext.Value.GetCombatant().SignalEndTurn();
	}

    public override void OnBehaviorRestart()
    {
        _hasActed.Value = false;
        _hasMoved.Value = false;
    }

    public override void OnEnd()
    {
        _hasMoved.Value = false;
        _hasActed.Value = false;
    }

	public override TaskStatus OnUpdate()
	{
		return TaskStatus.Success;
	}
}