using UnityEngine;

/// <summary>
/// 제자리에서 타깃을 향해 몸을 회전시키고, 조준이 완료되면 공격 노드로 진입시키는 액션 노드.
/// </summary>
public class BTActionLookAtTarget : BTActionNode
{
    [Tooltip("타겟을 향한 회전 속도")]
    [SerializeField] private float _rotationSpeed = 8.0f;

    [Tooltip("타겟을 정면으로 포착했다고 판단하는 허용 각도")]
    [SerializeField] private float _facingAngleTolerance = 25.0f;

    [Tooltip("공격 조준 중 다른 적에게 밀려났을 때 추적으로 전환할 거리")]
    [SerializeField] private float _breakDistanceMargin = 0.4f;

    private EnemyBlackboard _enemyBlackboard;
    private BTActionAttack _attackAction;

    public override void Initialize(BTBlackboard blackboard)
    {
        base.Initialize(blackboard);
        _enemyBlackboard = blackboard as EnemyBlackboard;

        if (_enemyBlackboard?.Owner != null)
        {
            _attackAction = _enemyBlackboard.Owner.GetComponentInChildren<BTActionAttack>();
        }
    }

    protected override BTNodeState ExecuteAction()
    {
        // 이미 공격 모션이 진행 중인 경우 끝날 때까지 유지
        if (_attackAction != null && _attackAction.IsAttacking)
        {
            return BTNodeState.Success;
        }

        // 타깃 유효성 검사
        if (_enemyBlackboard == null || !_enemyBlackboard.HasTarget)
            return BTNodeState.Failure;

        Transform owner = _enemyBlackboard.Owner;
        Transform target = _enemyBlackboard.Target;

        if (owner == null || target == null)
            return BTNodeState.Failure;

        // 거리 검사
        Vector3 directionToTarget = target.position - owner.position;
        directionToTarget.y = 0f;
        float currentDistance = directionToTarget.magnitude;

        float attackRange = _enemyBlackboard.EnemyData != null ? _enemyBlackboard.EnemyData.AttackRange : 2.0f;
        if (currentDistance > attackRange + _breakDistanceMargin)
        {
            return BTNodeState.Failure;
        }

        // 타깃을 향한 회전 계산
        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            owner.rotation = Quaternion.Slerp(owner.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
        }

        // 허용 각도 내에 들어오면 즉시 Success 반환하여 다음 공격 액션 실행
        float currentAngle = Vector3.Angle(owner.forward, directionToTarget.normalized);
        if (currentAngle <= _facingAngleTolerance)
        {
            return BTNodeState.Success;
        }

        return BTNodeState.Running;
    }
}