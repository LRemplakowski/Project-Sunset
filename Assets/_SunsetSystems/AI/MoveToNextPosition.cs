using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using SunsetSystems.Abilities;
using UnityEngine;
using SunsetSystems.Combat;

[TaskCategory("Grid System")]
public class MoveToNextPosition : Action
{
	[SerializeField, SharedRequired]
	private SharedAIContext _aiContext;
	[SerializeField]
	private MoveAbility _moveAbility;
    [SerializeField, SharedRequired]
    private SharedBool _hasMoved;

    private bool _isMoving;

    public override void OnStart()
	{
        var abilityUser = _aiContext.Value.GetCombatant().References.AbilityUser;
        abilityUser.SetCurrentTargetObject(_aiContext.Value.SelectedPosition.Targetable);
        _isMoving = abilityUser.ExecuteAbility(_moveAbility, OnMovementFinished);
    }

    public override void OnEnd()
    {
        _isMoving = false;
    }

    public override void OnBehaviorRestart()
    {
        _hasMoved.Value = false;
    }

    public override TaskStatus OnUpdate()
	{
        if (_hasMoved.Value)
            return TaskStatus.Success;
        else if (_isMoving)
            return TaskStatus.Running;
        else
            return TaskStatus.Failure;
    }

    private void OnMovementFinished()
    {
        _hasMoved.Value = true;
    }
}