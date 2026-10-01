using UnityEngine;
using UnityEngine.AI;

public class BTActionKeepDistance : BTActionNode
{
    [Header("Distance Settings")]
    [Tooltip("후퇴를 시작할 거리 비율")]
    [Range(0.3f, 0.9f)]
    [SerializeField] private float triggerRetreatRatio = 0.75f;

    [Tooltip("후퇴를 멈출 거리 비율")]
    [Range(0.5f, 1.0f)]
    [SerializeField] private float stopRetreatRatio = 0.95f;

    [Header("Movement")]
    [Tooltip("후퇴 속도 배율")]
    [SerializeField] private float retreatSpeedMultiplier = 1.0f;

    [Tooltip("1회 후퇴 시 뒤로 빠질 목표 거리")]
    [SerializeField] private float retreatStepDistance = 4.0f;

    private EnemyBlackboard enemyBlackboard;
    private NavMeshAgent navAgent;
    private bool isRetreating = false;

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
        if (enemyBlackboard == null || enemyBlackboard.EnemyData == null || !enemyBlackboard.HasTarget)
        {
            StopRetreat();
            return BTNodeState.Failure;
        }

        // 공격 모션 중에는 이동하지 않음
        if (enemyBlackboard.IsAttacking)
        {
            StopRetreat();
            return BTNodeState.Failure;
        }

        float currentDist = Vector3.Distance(enemyBlackboard.Owner.position, enemyBlackboard.Target.position);
        float attackRange = enemyBlackboard.EnemyData.AttackRange;

        float triggerDist = attackRange * triggerRetreatRatio;
        float stopDist = attackRange * stopRetreatRatio;

        // 아직 후퇴 중이 아닐 때
        if (!isRetreating)
        {
            // 공격 범위 내에 들어왔을 때 후퇴 시작
            if (currentDist <= triggerDist)
            {
                StartRetreat();
                return BTNodeState.Running;
            }
            return BTNodeState.Failure;
        }

        // 안전 거리 확보 또는 목적지 도달 시 정지
        if (currentDist >= stopDist || (navAgent != null && !navAgent.pathPending && navAgent.remainingDistance <= 0.4f))
        {
            StopRetreat();
            return BTNodeState.Failure;
        }

        return BTNodeState.Running;
    }

    /// <summary>
    /// 목적지 설정 및 후퇴 시작
    /// </summary>
    private void StartRetreat()
    {
        if (navAgent == null || !navAgent.isOnNavMesh) return;

        navAgent.updatePosition = true;
        navAgent.isStopped = false;
        navAgent.speed = enemyBlackboard.EnemyData.ChaseSpeed * retreatSpeedMultiplier;

        Vector3 fleeDir = (enemyBlackboard.Owner.position - enemyBlackboard.Target.position).normalized;
        fleeDir.y = 0f;

        Vector3 destination = enemyBlackboard.Owner.position + (fleeDir * retreatStepDistance);

        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 3.0f, NavMesh.AllAreas))
        {
            navAgent.SetDestination(hit.position);
        }
        else
        {
            navAgent.SetDestination(destination);
        }

        isRetreating = true;
    }

    /// <summary>
    /// 후퇴 중지 및 내비메시 이동 정지
    /// </summary>
    private void StopRetreat()
    {
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            if (navAgent.hasPath) navAgent.ResetPath();
            navAgent.velocity = Vector3.zero;
            navAgent.isStopped = true;
        }
        isRetreating = false;
    }

    private void OnDisable()
    {
        StopRetreat();
    }
}