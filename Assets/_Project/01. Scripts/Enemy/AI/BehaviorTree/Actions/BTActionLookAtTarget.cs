using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 제자리에서 타겟을 향해 부드럽게 회전하고, 조준 완료 및 공격 준비가 되면 다음 공격 노드로 진입시키는 액션 노드.
/// </summary>
public class BTActionLookAtTarget : BTActionNode
{
    [Tooltip("회전 속도")]
    [SerializeField] private float _rotationSpeed = 8.0f;

    [Tooltip("타겟 포착 각도")]
    [SerializeField] private float _facingAngleTolerance = 25.0f;

    [Tooltip("공격 조준 중 밀려났을 때 추적으로 전환할 거리")]
    [SerializeField] private float _breakDistanceMargin = 0.4f;

    private EnemyBlackboard _enemyBlackboard;
    private NavMeshAgent _navAgent;

    public override void Initialize(BTBlackboard blackboard)
    {
        base.Initialize(blackboard);
        _enemyBlackboard = blackboard as EnemyBlackboard;

        if (_enemyBlackboard?.Owner != null)
        {
            _navAgent = _enemyBlackboard.Owner.GetComponent<NavMeshAgent>();
        }
    }

    protected override BTNodeState ExecuteAction()
    {
        // 공격 진행 중이면 이미 조준 단계가 끝난 것으로 보고 Success 유지
        if (_enemyBlackboard != null && _enemyBlackboard.IsAttacking)
        {
            RestoreAgentRotation();
            return BTNodeState.Success;
        }

        // 타겟 유효성 검사
        if (_enemyBlackboard == null || !_enemyBlackboard.HasTarget)
        {
            RestoreAgentRotation();
            return BTNodeState.Failure;
        }

        Transform owner = _enemyBlackboard.Owner;
        Transform target = _enemyBlackboard.Target;

        if (owner == null || target == null)
        {
            RestoreAgentRotation();
            return BTNodeState.Failure;
        }

        // 내비메시 에이전트의 자동 회전 및 잔여 속도에 의한 간섭 방지
        if (_navAgent != null && _navAgent.isOnNavMesh)
        {
            _navAgent.velocity = Vector3.zero;
            _navAgent.isStopped = true;
            _navAgent.updateRotation = false;
        }

        // 거리 검사 (공격 사거리 + 마진 초과 시 추적으로 복귀)
        Vector3 directionToTarget = target.position - owner.position;
        directionToTarget.y = 0f;
        float currentDistance = directionToTarget.magnitude;

        float attackRange = _enemyBlackboard.EnemyData != null ? _enemyBlackboard.EnemyData.AttackRange : 2.0f;
        if (currentDistance > attackRange + _breakDistanceMargin)
        {
            RestoreAgentRotation();
            return BTNodeState.Failure;
        }

        // 타겟 방향으로 부드럽게 Slerp 회전
        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            owner.rotation = Quaternion.Slerp(owner.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
        }

        // 각도 검사 및 공격 준비 상태 확인
        float currentAngle = Vector3.Angle(owner.forward, directionToTarget.normalized);
        if (currentAngle <= _facingAngleTolerance)
        {
            // 각도가 맞아도 아직 쿨다운 중이라면 Attack 노드로 넘겨 시퀀스를 깨지 않고 조준 유지
            if (!_enemyBlackboard.IsAttackReady)
            {
                return BTNodeState.Running;
            }

            // 공격 준비까지 완료되었을 때만 에이전트 회전을 복원하고 공격 노드로 진입
            RestoreAgentRotation();
            return BTNodeState.Success;
        }

        return BTNodeState.Running;
    }

    private void RestoreAgentRotation()
    {
        if (_navAgent != null)
        {
            _navAgent.updateRotation = true;
        }
    }

    private void OnDisable()
    {
        RestoreAgentRotation();
    }
}