using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 플레이어가 일정 거리 안으로 좁혀왔을 때 후퇴하는 액션 노드.
/// </summary>
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

    [Header("Obstacle Evasion (벽/벼랑 우회 탐색)")]
    [Tooltip("직후방이 막혔을 때 순차적으로 검사할 좌우 각도 오프셋 목록")]
    [SerializeField] private float[] escapeAngles = new float[] { 0.0f, 35.0f, -35.0f, 65.0f, -65.0f, 90.0f, -90.0f };

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

    /// <summary>
    /// 후퇴 중일 때는 계속 이동, 안전 거리 확보 시 후퇴 중지
    /// </summary>
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

        // 공격 준비가 완료되었다면 거리 벌리기를 멈추고 즉시 실패 반환
        if (enemyBlackboard.IsAttackReady)
        {
            StopRetreat();
            return BTNodeState.Failure;
        }

        Transform owner = enemyBlackboard.Owner;
        Transform target = enemyBlackboard.Target;

        if (owner == null || target == null)
        {
            StopRetreat();
            return BTNodeState.Failure;
        }

        // 수평 평면 기준 거리 계산
        Vector3 diff = target.position - owner.position;
        diff.y = 0f;
        float currentDist = diff.magnitude;

        float attackRange = enemyBlackboard.EnemyData.AttackRange;
        float triggerDist = attackRange * triggerRetreatRatio;
        float stopDist = attackRange * stopRetreatRatio;

        // 아직 후퇴 중이 아닐 때
        if (!isRetreating)
        {
            // 위험 거리 내로 진입하면 후퇴 시작
            if (currentDist <= triggerDist)
            {
                if (StartRetreat(owner.position, target.position))
                {
                    return BTNodeState.Running;
                }
            }
            return BTNodeState.Failure;
        }

        // 안전 거리 확보 시 정지
        if (currentDist >= stopDist)
        {
            StopRetreat();
            return BTNodeState.Failure;
        }

        // 목적지 도달 시 정지
        if (navAgent != null && !navAgent.pathPending && navAgent.remainingDistance <= 0.4f)
        {
            StopRetreat();
            return BTNodeState.Failure;
        }

        return BTNodeState.Running;
    }

    /// <summary>
    /// 벽을 회피하는 다각도 탐색
    /// </summary>
    private bool StartRetreat(Vector3 ownerPos, Vector3 targetPos)
    {
        if (navAgent == null || !navAgent.isOnNavMesh) return false;

        // 장애물과 절벽을 회피할 수 있는 유효 좌표 산출
        if (TryFindSafeRetreatPosition(ownerPos, targetPos, retreatStepDistance, out Vector3 targetDestination))
        {
            navAgent.updatePosition = true;
            navAgent.isStopped = false;
            navAgent.speed = enemyBlackboard.EnemyData.ChaseSpeed * retreatSpeedMultiplier;

            navAgent.SetDestination(targetDestination);
            isRetreating = true;
            return true;
        }

        // 모든 방향이 막혀 피할 곳이 없는 코너 상태라면 이동 불가 처리
        return false;
    }

    /// <summary>
    /// 플레이어 반대 방향을 기준으로 부채꼴 각도를 순회하며 벽이 없는 좌표 탐색
    /// </summary>
    private bool TryFindSafeRetreatPosition(Vector3 ownerPos, Vector3 targetPos, float distance, out Vector3 safePos)
    {
        // 플레이어 반대 방향을 기준으로 후퇴 후보 좌표를 탐색
        Vector3 baseFleeDir = ownerPos - targetPos;
        baseFleeDir.y = 0f;

        if (baseFleeDir.sqrMagnitude < 0.001f)
        {
            baseFleeDir = -enemyBlackboard.Owner.forward;
        }
        baseFleeDir.Normalize();

        // 후보 좌표 중 가장 멀리 이동 가능한 좌표 저장
        Vector3 bestAlternative = ownerPos;
        float maxWalkableDistance = 0f;

        for (int i = 0; i < escapeAngles.Length; i++)
        {
            // 각도별로 회전된 후보 방향 벡터 계산
            Vector3 rotatedDir = Quaternion.Euler(0f, escapeAngles[i], 0f) * baseFleeDir;
            Vector3 candidatePos = ownerPos + (rotatedDir * distance);

            // 후보 위치 주변의 유효 NavMesh 점 샘플링
            if (NavMesh.SamplePosition(candidatePos, out NavMeshHit sampleHit, 1.5f, NavMesh.AllAreas))
            {
                // 적 위치에서 후보 지점까지 NavMesh 벽이나 절벽 경계선에 걸리지 않는지 직선 레이 검사
                if (!NavMesh.Raycast(ownerPos, sampleHit.position, out NavMeshHit rayHit, NavMesh.AllAreas))
                {
                    // 장애물 없이 도달 가능한 완벽한 지점을 찾음
                    safePos = sampleHit.position;
                    return true;
                }
                else
                {
                    // 벽이나 벼랑에 부딪히기 직전 지점까지의 거리를 계산해 대안으로 설정
                    float reachableDist = Vector3.Distance(ownerPos, rayHit.position);
                    if (reachableDist > maxWalkableDistance && reachableDist > 1.0f)
                    {
                        maxWalkableDistance = reachableDist;
                        // 충돌 모서리 바로 앞 지점을 대안 좌표로 설정
                        bestAlternative = rayHit.position;
                    }
                }
            }
        }

        // 직선으로 완전히 뚫린 각도가 없더라도 부분 이동 가능한 벽면 앞 공간이 있다면 그 지점으로 우회
        if (maxWalkableDistance > 1.0f)
        {
            safePos = bestAlternative;
            return true;
        }

        safePos = ownerPos;
        return false;
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