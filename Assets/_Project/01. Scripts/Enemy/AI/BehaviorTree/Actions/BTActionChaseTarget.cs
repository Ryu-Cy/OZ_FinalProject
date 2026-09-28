using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 지정된 슬롯 좌표 또는 플레이어 사거리를 향해 추적하는 액션 노드.
/// 사거리 밖에서 어정쩡하게 굳는 현상을 원천 방지합니다.
/// </summary>
public class BTActionChaseTarget : BTActionNode
{
    [Header("Arrival Settings")]
    [Tooltip("슬롯 좌표 도착 판정 거리")]
    [SerializeField] private float _slotArrivalDistance = 0.4f;

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
            LockAgent();
            return BTNodeState.Failure;
        }

        Transform owner = enemyBlackboard.Owner;
        Transform target = enemyBlackboard.Target;

        if (owner == null || target == null)
        {
            LockAgent();
            return BTNodeState.Failure;
        }

        Vector3 toTarget = target.position - owner.position;
        toTarget.y = 0f;
        float distance = toTarget.magnitude;

        float attackRange = enemyBlackboard.EnemyData != null ? enemyBlackboard.EnemyData.AttackRange : 2.0f;

        // 플레이어와의 실제 거리가 공격 사거리 안쪽에 완전히 들어왔을 때
        if (distance <= attackRange)
        {
            // 타깃 정면 조준
            if (toTarget.sqrMagnitude > 0.001f)
            {
                owner.rotation = Quaternion.LookRotation(toTarget);
            }

            LockAgent();
            return BTNodeState.Success;
        }

        // 이동 수행
        UnlockAgent();

        if (enemyBlackboard.EnemyData != null)
        {
            navAgent.speed = enemyBlackboard.EnemyData.ChaseSpeed;
        }

        // 할당된 슬롯 좌표가 있더라도, 플레이어 사거리 안쪽 지점을 향하도록 보장
        Vector3 destination = enemyBlackboard.HasAssignedPosition
            ? enemyBlackboard.AssignedPosition
            : target.position - (toTarget.normalized * (attackRange - 0.35f));

        if (navAgent.isOnNavMesh)
        {
            navAgent.SetDestination(destination);
        }

        if (navAgent.pathPending)
            return BTNodeState.Running;

        // 슬롯 도착 판정
        if (navAgent.isOnNavMesh && navAgent.hasPath)
        {
            if (navAgent.remainingDistance <= _slotArrivalDistance && distance <= attackRange)
            {
                LockAgent();
                return BTNodeState.Success;
            }
        }

        return BTNodeState.Running;
    }

    private void LockAgent()
    {
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.velocity = Vector3.zero;
            navAgent.isStopped = true;
            navAgent.ResetPath();
            navAgent.updatePosition = false;
        }
    }

    private void UnlockAgent()
    {
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.updatePosition = true;
            navAgent.isStopped = false;
        }
    }

    private void OnDisable()
    {
        LockAgent();
    }
}