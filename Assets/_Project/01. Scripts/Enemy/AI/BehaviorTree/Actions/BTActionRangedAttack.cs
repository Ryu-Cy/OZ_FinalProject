using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 진입 즉시 풀에서 탄환을 꺼내 발사, 후딜레이 동안 정지 후 쿨다운을 적용하는 원거리 공격 노드.
/// </summary>
public class BTActionRangedAttack : BTActionNode
{
    [Header("Bullet Prefab")]
    [Tooltip("사용할 탄환 프리팹")]
    [SerializeField] private GameObject bulletPrefab;

    [Tooltip("탄환 발사 위치")]
    [SerializeField] private Transform firePoint;

    [Header("Timing")]
    [Tooltip("공격 후딜레이 지속 시간(초)")]
    [SerializeField] private float attackDuration = 1.0f;

    [Header("Visual Feedback")]
    [Tooltip("공격 중 표시할 머티리얼 색상")]
    [SerializeField] private Color castingColor = Color.magenta;

    private EnemyBlackboard enemyBlackboard;
    private NavMeshAgent navAgent;
    private MeshRenderer meshRenderer;
    private Color originalColor;

    private float attackStartTime;
    private bool isAttacking = false;

    public bool IsAttacking => isAttacking;

    public override void Initialize(BTBlackboard blackboard)
    {
        base.Initialize(blackboard);
        enemyBlackboard = blackboard as EnemyBlackboard;

        if (enemyBlackboard?.Owner != null)
        {
            navAgent = enemyBlackboard.Owner.GetComponent<NavMeshAgent>();
            meshRenderer = enemyBlackboard.Owner.GetComponentInChildren<MeshRenderer>();
            if (meshRenderer != null)
            {
                originalColor = meshRenderer.material.color;
            }

            if (ObjectPoolManager.HasInstance && bulletPrefab != null)
            {
                ObjectPoolManager.Instance.RegisterPool(enemyBlackboard.Owner.gameObject, bulletPrefab, 3);
            }
        }
    }

    protected override BTNodeState ExecuteAction()
    {
        // 이동 정지 및 트랜스폼-내비메시 강제 동기화 유지
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.updatePosition = true;
            navAgent.velocity = Vector3.zero;
            navAgent.isStopped = true;
        }

        if (enemyBlackboard == null || enemyBlackboard.EnemyData == null || !enemyBlackboard.HasTarget)
        {
            ResetAttackState();
            return BTNodeState.Failure;
        }

        // 공격 시작
        if (!isAttacking)
        {
            if (!enemyBlackboard.IsAttackReady)
            {
                ResetAttackState();
                return BTNodeState.Failure;
            }

            isAttacking = true;
            enemyBlackboard.IsAttacking = true;
            attackStartTime = Time.time;
            SetRendererColor(castingColor);

            ShootBullet();

            return BTNodeState.Running;
        }

        // 후딜레이 모션 완료 대기
        float elapsed = Time.time - attackStartTime;
        if (elapsed < attackDuration)
        {
            return BTNodeState.Running;
        }

        // 공격 종료 처리
        FinishAttack();
        return BTNodeState.Success;
    }

    /// <summary>
    /// 투사체를 풀에서 꺼내 발사
    /// </summary>
    private void ShootBullet()
    {
        if (ObjectPoolManager.Instance == null || enemyBlackboard.Target == null) return;

        // 발사 위치와 방향 계산
        Vector3 spawnPos = firePoint != null
            ? firePoint.position
            : enemyBlackboard.Owner.position + Vector3.up * 1.3f;

        // 목표 위치를 약간 위로 조정하여 발사 방향 계산
        Vector3 targetAimPos = enemyBlackboard.Target.position + Vector3.up * 1.0f;
        Vector3 fireDir = (targetAimPos - spawnPos).normalized;

        // 풀에서 탄환을 꺼내 발사
        EnemyBullet bullet = ObjectPoolManager.Instance.Spawn<EnemyBullet>(enemyBlackboard.Owner.gameObject);
        if (bullet != null)
        {
            bullet.Fire(spawnPos, fireDir, enemyBlackboard.EnemyData.BaseAttackPower, enemyBlackboard.Owner.gameObject);
        }
    }

    /// <summary>
    /// 공격 종료 후 상태 초기화 및 쿨다운을 적용합니다.
    /// </summary>
    private void FinishAttack()
    {
        SetRendererColor(originalColor);

        if (enemyBlackboard.EnemyData != null)
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
    /// 공격 상태 및 시각적 피드백 초기화
    /// </summary>
    public void ResetAttackState()
    {
        if (isAttacking)
        {
            SetRendererColor(originalColor);
            isAttacking = false;
            if (enemyBlackboard != null)
            {
                enemyBlackboard.IsAttacking = false;
            }
        }
    }

    /// <summary>
    /// 색상 변환
    /// </summary>
    /// <param name="color">변경할 색상</param>
    private void SetRendererColor(Color color)
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