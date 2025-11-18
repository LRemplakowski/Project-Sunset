using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskCategory("Grid Combat")]
public class ShouldKeepCurrentPosition : Conditional
{
    [SerializeField]
    private float _keepPositionThreshold = 100f;
    [SerializeField, Range(1, 100)]
    private float _inCoverScore = 10, _currentPositionScore = 10, _hasTargetsInRange = 10f;
    [SerializeField]
    private bool _invertEvaluation = false;

    [SerializeField, SharedRequired]
    private SharedAIContext _aiContext;
    [SerializeField, SharedRequired]
    private SharedBool _hasMoved;

    private bool _shouldKeepPosition;

    public override void OnStart()
    {
        float targetsScore = _aiContext.Value.GetTargetsInWeaponRange() * _hasTargetsInRange;
        float coverScore = _aiContext.Value.IsInCover() ? _inCoverScore : 0;
        float totalScore = _currentPositionScore + coverScore + targetsScore;
        _shouldKeepPosition = EvaluateScore(totalScore, _keepPositionThreshold);
        if (_invertEvaluation)
            _shouldKeepPosition = !_shouldKeepPosition;
    }

    public override TaskStatus OnUpdate()
	{
        if (_shouldKeepPosition)
        {
            _hasMoved.Value = true;
            return TaskStatus.Success;
        }
        else
        {
            return TaskStatus.Failure;
        }
	}

    private bool EvaluateScore(float score, float threshold)
    {
        return score >= threshold;
    }
}