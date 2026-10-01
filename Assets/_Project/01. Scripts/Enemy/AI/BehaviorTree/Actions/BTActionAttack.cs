using UnityEngine;

/// <summary>
/// 타깃에게 근접 공격을 수행하는 액션 노드.
/// </summary>
public class BTActionAttack : BTActionNode
{
    [Header("Visual Settings")]
    [Tooltip("공격 시 변경할 머티리얼 색상")]
    [SerializeField] private Color attackColor = Color.blue;

    [Header("Attack Settings")]
    [Tooltip("공격 지속 시간")]
    [SerializeField] private float attackDuration = 0.5f;

    private EnemyBlackboard enemyBlackboard;
    private MeshRenderer meshRenderer;
    private Color originalColor;
    private UnityEngine.AI.NavMeshAgent navAgent;

    private float attackEndTime = 0.0f;
    private bool isAttacking = false;

    public bool IsAttacking => isAttacking;

    public override void Initialize(BTBlackboard blackboard)
    {
        base.Initialize(blackboard);
        enemyBlackboard = blackboard as EnemyBlackboard;

        if (enemyBlackboard != null && enemyBlackboard.Owner != null)
        {
            navAgent = enemyBlackboard.Owner.GetComponent<UnityEngine.AI.NavMeshAgent>();
            meshRenderer = enemyBlackboard.Owner.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null)
            {
                originalColor = meshRenderer.material.color;
            }
        }
    }

    protected override BTNodeState ExecuteAction()
    {
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.velocity = Vector3.zero;
            navAgent.isStopped = true;
        }

        if (enemyBlackboard == null || enemyBlackboard.EnemyData == null)
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
            attackEndTime = Time.time + attackDuration;
            SetCapsuleColor(attackColor);
            return BTNodeState.Running;
        }

        // 공격 진행 중
        if (Time.time < attackEndTime)
        {
            return BTNodeState.Running;
        }

        // 공격 완료
        FinishAttack();
        return BTNodeState.Success;
    }

    /// <summary>
    /// 공격 완료
    /// 공격 상태 초기화 및 쿨다운 설정
    /// </summary>
    private void FinishAttack()
    {
        SetCapsuleColor(originalColor);

        if (enemyBlackboard.EnemyData != null)
        {
            enemyBlackboard.SetAttackCooldown(enemyBlackboard.EnemyData.AttackCooldown);
        }

        isAttacking = false;
        if (enemyBlackboard != null)
        {
            enemyBlackboard.IsAttacking = false;
        }
        attackEndTime = 0.0f;
    }

    /// <summary>
    /// 공격 상태 초기화
    /// </summary>
    public void ResetAttackState()
    {
        if (isAttacking)
        {
            SetCapsuleColor(originalColor);
            isAttacking = false;
            if (enemyBlackboard != null)
            {
                enemyBlackboard.IsAttacking = false;
            }
            attackEndTime = 0.0f;
        }
    }

    /// <summary>
    /// 캡슐 색상 변경
    /// </summary>
    /// <param name="color">변경할 색</param>
    private void SetCapsuleColor(Color color)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = color;
        }
    }

    private void OnDisable()
    {
        ResetAttackState();
    }
}