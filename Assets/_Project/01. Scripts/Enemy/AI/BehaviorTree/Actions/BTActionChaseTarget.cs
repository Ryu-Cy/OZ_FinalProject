using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 블랙보드에 등록된 타겟을 향해 NavMeshAgent로 이동하는 액션 노드
/// </summary>
public class BTActionChaseTarget : BTActionNode
{
    private EnemyBlackboard enemyBlackboard;
    private NavMeshAgent navAgent;

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
        if (enemyBlackboard == null || !enemyBlackboard.HasTarget || navAgent == null)
        {
            HardStop();
            return BTNodeState.Failure;
        }

        Transform owner = enemyBlackboard.Owner;
        Transform target = enemyBlackboard.Target;

        if (owner == null || target == null)
        {
            HardStop();
            return BTNodeState.Failure;
        }

        // 시야 실시간 검사
        EnemyVision vision = owner.GetComponent<EnemyVision>();
        if (vision != null && !vision.TryFindTarget(out _))
        {
            enemyBlackboard.ClearTarget();
            HardStop();
            return BTNodeState.Failure;
        }

        // 수평 거리 및 방향 벡터 계산
        Vector3 toTarget = target.position - owner.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        float attackRange = enemyBlackboard.EnemyData != null ? enemyBlackboard.EnemyData.AttackRange : 2.0f;

        // 사거리 도달 판정 -> 감속 없이 칼제동 걸고 공격 시퀀스로 턴 넘김
        if (distance <= attackRange)
        {
            HardStop();
            return BTNodeState.Success;
        }

        // 전속력 질주 세팅
        if (enemyBlackboard.EnemyData != null)
        {
            navAgent.speed = enemyBlackboard.EnemyData.ChaseSpeed;
        }

        // 목적지를 플레이어 몸통(0m)이 아니라 공격 사거리로 설정
        Vector3 targetEdge = target.position - (toTarget.normalized * attackRange);

        navAgent.isStopped = false;
        navAgent.SetDestination(targetEdge);

        return BTNodeState.Running;
    }

    /// <summary>
    /// 물리 관성과 네브메시 잔여 이동량을 즉시 0으로 소멸시키는 급제동
    /// </summary>
    private void HardStop()
    {
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
            navAgent.velocity = Vector3.zero;
            navAgent.ResetPath();
        }
    }
}