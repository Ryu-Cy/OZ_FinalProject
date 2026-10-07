using UnityEngine;

/// <summary>
/// 타깃에게 근접 공격을 수행하는 액션 노드.
/// </summary>
public class BTActionAttack : BTActionNode
{
    private EnemyBlackboard enemyBlackboard;
    private EnemyController enemyController;
    private UnityEngine.AI.NavMeshAgent navAgent;

    private bool isAttacking = false;

    // 프로퍼티
    public bool IsAttacking => isAttacking;

    public override void Initialize(BTBlackboard blackboard)
    {
        base.Initialize(blackboard);
        enemyBlackboard = blackboard as EnemyBlackboard;

        if (enemyBlackboard != null && enemyBlackboard.Owner != null)
        {
            enemyController = enemyBlackboard.Owner.GetComponent<EnemyController>();
            navAgent = enemyBlackboard.Owner.GetComponent<UnityEngine.AI.NavMeshAgent>();
        }
    }

    protected override BTNodeState ExecuteAction()
    {
        // 공격 중 이동 정지
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.velocity = Vector3.zero;
            navAgent.isStopped = true;
        }

        if (enemyBlackboard == null || enemyBlackboard.EnemyData == null || enemyController == null)
        {
            ResetAttackState();
            return BTNodeState.Failure;
        }

        // 공격 개시
        if (!isAttacking)
        {
            if (!enemyBlackboard.HasTarget || !enemyBlackboard.IsAttackReady)
            {
                ResetAttackState();
                return BTNodeState.Failure;
            }

            isAttacking = true;
            enemyBlackboard.IsAttacking = true;

            // 이전 공격 플래그 리셋 및 애니메이션 트리거 발동
            enemyController.ResetAttackState();
            enemyController.TriggerAttackAnimation();

            return BTNodeState.Running;
        }

        // 공격 진행 중
        if (enemyBlackboard.Target != null)
        {
            Vector3 direction = (enemyBlackboard.Target.position - enemyController.transform.position).normalized;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                enemyController.transform.rotation = Quaternion.Slerp(
                    enemyController.transform.rotation,
                    lookRotation,
                    Time.deltaTime * 6f
                );
            }
        }

        // 애니메이션 클립의 AttackFinished 이벤트 수신 확인
        if (enemyController.IsAttackFinished())
        {
            FinishAttack();
            return BTNodeState.Success;
        }

        // 아직 애니메이션 동작 중
        return BTNodeState.Running;
    }

    /// <summary>
    /// 공격 완료 처리
    /// </summary>
    private void FinishAttack()
    {
        if (enemyBlackboard != null && enemyBlackboard.EnemyData != null)
        {
            enemyBlackboard.SetAttackCooldown(enemyBlackboard.EnemyData.AttackCooldown);
        }

        isAttacking = false;
        if (enemyBlackboard != null)
        {
            enemyBlackboard.IsAttacking = false;
        }
    }

    /// <summary>
    /// 외부 요인이나 취소 시 공격 상태 초기화
    /// </summary>
    public void ResetAttackState()
    {
        if (isAttacking)
        {
            isAttacking = false;

            if (enemyBlackboard != null)
            {
                enemyBlackboard.IsAttacking = false;
            }

            if (enemyController != null)
            {
                enemyController.ResetAttackState();
            }
        }
    }

    private void OnDisable()
    {
        ResetAttackState();
    }
}