using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 블랙보드에 등록된 타겟을 향해 NavMeshAgent로 이동하는 액션 노드
/// </summary>
public class BTActionChaseTarget : BTActionNode
{
    [Header("Chase Settings")]
    [Tooltip("타겟 도달 판단 거리")]
    [SerializeField] private float stoppingDistance = 1.8f;

    [Tooltip("타겟 위치 갱신 최소 거리")]
    [SerializeField] private float repathTolerance = 0.2f;

    private EnemyBlackboard enemyBlackboard;
    private NavMeshAgent navAgent;
    private Vector3 lastDestination;

    #region Properties
    public float StoppingDistance
    {
        get => stoppingDistance;
        set => stoppingDistance = value;
    }
    #endregion

    public override void Initialize(BTBlackboard blackboard)
    {
        base.Initialize(blackboard);

        enemyBlackboard = blackboard as EnemyBlackboard;

        if (enemyBlackboard?.Owner != null)
        {
            navAgent = enemyBlackboard.Owner.GetComponent<NavMeshAgent>();
        }
    }

    protected override BTNodeState ExecuteAction()
    {
        if (enemyBlackboard == null || navAgent == null)
            return BTNodeState.Failure;

        // 타겟 유효성 검사
        if (!enemyBlackboard.HasTarget)
        {
            StopMovement();
            return BTNodeState.Failure;
        }

        Transform target = enemyBlackboard.Target;
        Vector3 ownerPosition = enemyBlackboard.Owner.position;
        Vector3 targetPosition = target.position;

        float distanceToTarget = Vector3.Distance(ownerPosition, targetPosition);

        // 정지 거리 진입 확인
        if (distanceToTarget <= stoppingDistance)
        {
            StopMovement();
            return BTNodeState.Success;
        }

        // 타겟 이동에 따른 NavMesh 경로 갱신
        if (Vector3.Distance(lastDestination, targetPosition) > repathTolerance)
        {
            navAgent.isStopped = false;
            navAgent.SetDestination(targetPosition);
            lastDestination = targetPosition;
            enemyBlackboard.Destination = targetPosition;
        }

        // 경로 계산 대기 중 체크
        if (navAgent.pathPending)
            return BTNodeState.Running;

        return BTNodeState.Running;
    }

    /// <summary>
    /// 이동 정지 및 경로 초기화
    /// </summary>
    private void StopMovement()
    {
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
            navAgent.ResetPath();
        }
    }
}