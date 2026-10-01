using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// CombatManager로부터 지정받은 좌표로 기동하여 길을 가로막는 액션 노드.
/// </summary>
public class BTActionBodyBlock : BTActionNode
{
    [Header("Movement Settings")]
    [Tooltip("좌표 갱신 거리")]
    [SerializeField] private float _repathThreshold = 0.3f;

    [Tooltip("차단 위치 도착 허용 반경")]
    [SerializeField] private float _arrivalDistance = 0.2f;

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

        // 바디블로커 역할이 아니거나 할당 좌표가 없다면 실패
        if (!_enemyBlackboard.HasAssignedPosition || _enemyBlackboard.Role != CombatRole.BodyBlocker)
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

        // 플레이어가 공격 사거리까지 접근해오면 탈출
        if (distToTarget <= attackRange)
        {
            if (diff.sqrMagnitude > 0.001f)
            {
                owner.rotation = Quaternion.LookRotation(diff);
            }

            StopMovement();
            return BTNodeState.Success;
        }

        // 좌표를 향해 이동
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
            if (_navAgent.isOnNavMesh)
            {
                _navAgent.SetDestination(targetPos);
            }
            _hasSetDestination = true;
        }

        // 좌표에 이미 도달한 경우 제자리에서 플레이어를 정면 응시
        if (_navAgent.isOnNavMesh && !_navAgent.pathPending && _navAgent.remainingDistance <= _arrivalDistance)
        {
            if (diff.sqrMagnitude > 0.001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(diff);
                owner.rotation = Quaternion.Slerp(owner.rotation, lookRot, Time.deltaTime * 6.0f);
            }
        }

        return BTNodeState.Running;
    }

    /// <summary>
    /// NavMeshAgent 정지 및 경로 초기화
    /// </summary>
    private void StopMovement()
    {
        if (_navAgent != null && _navAgent.isOnNavMesh)
        {
            _navAgent.velocity = Vector3.zero;
            _navAgent.isStopped = true;
            _navAgent.ResetPath();
            _navAgent.updatePosition = false;
        }
    }

    /// <summary>
    /// NavMeshAgent 이동 상태 초기화
    /// </summary>
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