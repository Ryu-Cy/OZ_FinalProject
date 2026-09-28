using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// CombatManager로부터 할당받은 전술 좌표(우회 지점)로 기동하는 액션 노드.
/// </summary>
public class BTActionFlank : BTActionNode
{
    [Header("Movement Settings")]
    [Tooltip("기존 목표 좌표와 새 좌표 간의 최소 갱신 차이")]
    [SerializeField] private float _repathThreshold = 0.25f;

    [Tooltip("공격 사거리 대비 확실한 진입")]
    [SerializeField] private float _strikeRangeOffset = 0.15f;

    private EnemyBlackboard _enemyBlackboard;
    private NavMeshAgent _navAgent;
    private Vector3 _currentDestination;
    private bool _hasSetDestination = false;

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
        if (_enemyBlackboard == null)
            return BTNodeState.Failure;

        if (_navAgent == null)
        {
            _navAgent = GetComponentInParent<NavMeshAgent>();
            if (_navAgent == null)
                return BTNodeState.Failure;
        }

        // 우회 역할이 아니거나 할당 좌표가 없다면 실패
        if (!_enemyBlackboard.HasAssignedPosition || _enemyBlackboard.Role != CombatRole.SideAttacker)
        {
            ResetMovement();
            return BTNodeState.Failure;
        }

        Transform owner = _enemyBlackboard.Owner;
        Transform target = _enemyBlackboard.Target;

        if (owner == null || target == null)
        {
            ResetMovement();
            return BTNodeState.Failure;
        }

        float attackRange = _enemyBlackboard.EnemyData != null ? _enemyBlackboard.EnemyData.AttackRange : 2.0f;

        // 플레이어와의 실시간 거리 계산
        Vector3 diff = target.position - owner.position;
        diff.y = 0f;
        float distToTarget = diff.magnitude;

        if (distToTarget <= (attackRange - _strikeRangeOffset))
        {
            // 타깃 정면 조준
            if (diff.sqrMagnitude > 0.001f)
            {
                owner.rotation = Quaternion.LookRotation(diff);
            }

            StopMovement();
            _enemyBlackboard.ClearAssignedTactics();
            // 역할 덮어쓰기 없이 즉시 Success 반환하여 다음 공격 액션 실행 유도
            return BTNodeState.Success;
        }

        // 이동 지속 처리
        Vector3 targetPos = _enemyBlackboard.AssignedPosition;

        if (_navAgent.isOnNavMesh)
        {
            _navAgent.updatePosition = true;
            if (_navAgent.isStopped) _navAgent.isStopped = false;
        }

        if (_enemyBlackboard.EnemyData != null)
        {
            _navAgent.speed = _enemyBlackboard.EnemyData.ChaseSpeed;
        }

        // 목적지 갱신
        if (!_hasSetDestination || Vector3.Distance(_currentDestination, targetPos) > _repathThreshold)
        {
            _currentDestination = targetPos;
            _enemyBlackboard.Destination = targetPos;
            if (_navAgent.isOnNavMesh)
            {
                _navAgent.SetDestination(targetPos);
            }
            _hasSetDestination = true;
        }

        return BTNodeState.Running;
    }

    private void StopMovement()
    {
        if (_navAgent != null && _navAgent.isOnNavMesh)
        {
            _navAgent.velocity = Vector3.zero;
            _navAgent.isStopped = true;
            _navAgent.ResetPath();
            _navAgent.updatePosition = true;
        }
    }

    private void ResetMovement()
    {
        _hasSetDestination = false;
        StopMovement();
    }

    private void OnDisable()
    {
        ResetMovement();
    }
}