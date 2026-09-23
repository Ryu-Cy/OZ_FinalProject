using UnityEngine;

/// <summary>
/// 타깃에게 공격을 수행하는 액션 노드 <br/>
/// 1차 빌드 시각적 피드백을 위해 머터리얼 색상 조절
/// </summary>
public class BTActionAttack : BTActionNode
{
    [Header("Visual Settings")]
    [Tooltip("공격 시 변경할 머티리얼 색상")]
    [SerializeField] private Color attackColor = Color.blue;

    [Header("Attack Settings")]
    [Tooltip("공격 지속 시간 (가상 모션 및 피드백 유지 시간)")]
    [SerializeField] private float attackDuration = 0.5f;

    private EnemyBlackboard enemyBlackboard;
    private MeshRenderer meshRenderer;
    private Color originalColor;

    private float attackEndTime = 0.0f;
    private bool isAttacking = false;

    public bool IsAttacking => isAttacking;

    private void OnDisable()
    {
        ResetAttackState();
    }

    public override void Initialize(BTBlackboard blackboard)
    {
        base.Initialize(blackboard);

        enemyBlackboard = blackboard as EnemyBlackboard;

        if (enemyBlackboard != null && enemyBlackboard.Owner != null)
        {
            meshRenderer = enemyBlackboard.Owner.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null)
            {
                originalColor = meshRenderer.material.color;
            }
        }
    }

    protected override BTNodeState ExecuteAction()
    {
        // 블랙보드 및 필수 데이터 유효성 검사
        if (enemyBlackboard == null || enemyBlackboard.EnemyData == null)
        {
            ResetAttackState();
            return BTNodeState.Failure;
        }

        // 공격 개시 전 상태 체크
        if (!isAttacking)
        {
            // 타깃이 없거나 쿨다운이 덜 풀렸다면 실패 처리
            if (!enemyBlackboard.HasTarget || !enemyBlackboard.IsAttackReady)
            {
                ResetAttackState();
                return BTNodeState.Failure;
            }

            // 공격 개시
            isAttacking = true;
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

    private void FinishAttack()
    {
        SetCapsuleColor(originalColor);

        // 쿨다운 등록
        if (enemyBlackboard.EnemyData != null)
        {
            enemyBlackboard.SetAttackCooldown(enemyBlackboard.EnemyData.AttackCooldown);
        }

        isAttacking = false;
        attackEndTime = 0.0f;
    }

    private void SetCapsuleColor(Color color)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = color;
        }
    }

    public void ResetAttackState()
    {
        if (isAttacking)
        {
            SetCapsuleColor(originalColor);
            isAttacking = false;
            attackEndTime = 0.0f;
        }
    }
}