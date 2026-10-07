using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 애니메이션 이벤트 타이밍에 투사체를 풀에서 꺼내 발사하고 모션 완료 시 Success를 반환하는 원거리 공격 노드.
/// </summary>
public class BTActionRangedAttack : BTActionNode
{
    [Header("Bullet Prefab")]
    [Tooltip("사용할 탄환 프리팹")]
    [SerializeField] private GameObject bulletPrefab;

    [Tooltip("탄환 발사 위치")]
    [SerializeField] private Transform firePoint;

    private EnemyBlackboard enemyBlackboard;
    private EnemyController enemyController;
    private EnemyAnimationEventController animEventController;
    private NavMeshAgent navAgent;

    private bool isAttacking = false;
    private bool hasShot = false;

    public bool IsAttacking => isAttacking;

    public override void Initialize(BTBlackboard blackboard)
    {
        base.Initialize(blackboard);
        enemyBlackboard = blackboard as EnemyBlackboard;

        if (enemyBlackboard?.Owner != null)
        {
            enemyController = enemyBlackboard.Owner.GetComponent<EnemyController>();
            animEventController = enemyBlackboard.Owner.GetComponentInChildren<EnemyAnimationEventController>();
            navAgent = enemyBlackboard.Owner.GetComponent<NavMeshAgent>();

            TryRegisterBulletPool();
        }
    }

    private void Start()
    {
        TryRegisterBulletPool();
    }

    /// <summary>
    /// 탄환 풀 등록.
    /// </summary>
    private void TryRegisterBulletPool()
    {
        if (ObjectPoolManager.HasInstance && bulletPrefab != null && enemyBlackboard?.Owner != null)
        {
            ObjectPoolManager.Instance.RegisterPool(enemyBlackboard.Owner.gameObject, bulletPrefab, 3);
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

        if (enemyBlackboard == null || enemyBlackboard.EnemyData == null || !enemyBlackboard.HasTarget || enemyController == null)
        {
            ResetAttackState();
            return BTNodeState.Failure;
        }

        // 공격 개시
        if (!isAttacking)
        {
            if (!enemyBlackboard.IsAttackReady)
            {
                ResetAttackState();
                return BTNodeState.Failure;
            }

            isAttacking = true;
            hasShot = false;
            enemyBlackboard.IsAttacking = true;

            enemyController.ResetAttackState();
            enemyController.TriggerAttackAnimation();

            return BTNodeState.Running;
        }

        // 애니메이션의 발사 이벤트 타이밍 감지
        if (!hasShot && animEventController != null && animEventController.IsShootTriggered)
        {
            ShootBullet();
            hasShot = true;
        }

        // 모션 완료 대기
        if (enemyController.IsAttackFinished())
        {
            if (!hasShot)
            {
                ShootBullet();
                hasShot = true;
            }

            FinishAttack();
            return BTNodeState.Success;
        }

        return BTNodeState.Running;
    }

    /// <summary>
    /// 탄환을 풀에서 꺼내 발사.
    /// </summary>
    private void ShootBullet()
    {
        if (enemyBlackboard == null || enemyBlackboard.Target == null || enemyBlackboard.Owner == null) return;

        TryRegisterBulletPool();

        if (ObjectPoolManager.Instance == null) return;

        // 탄환 발사 위치와 방향 계산
        Vector3 spawnPos = firePoint != null
            ? firePoint.position
            : enemyBlackboard.Owner.position + Vector3.up * 1.3f;

        Vector3 targetAimPos = enemyBlackboard.Target.position + Vector3.up * 1.0f;
        Vector3 fireDir = (targetAimPos - spawnPos).normalized;

        // 탄환 풀에서 꺼내 발사
        EnemyBullet bullet = ObjectPoolManager.Instance.Spawn<EnemyBullet>(enemyBlackboard.Owner.gameObject);
        if (bullet != null)
        {
            bullet.Fire(spawnPos, fireDir, enemyBlackboard.EnemyData.BaseAttackPower, enemyBlackboard.Owner.gameObject);
        }
    }

    /// <summary>
    /// 공격 완료 처리
    /// </summary>
    private void FinishAttack()
    {
        if (enemyBlackboard.EnemyData != null)
        {
            enemyBlackboard.SetAttackCooldown(enemyBlackboard.EnemyData.AttackCooldown);
        }

        isAttacking = false;
        hasShot = false;
        if (enemyBlackboard != null)
        {
            enemyBlackboard.IsAttacking = false;
        }
    }

    /// <summary>
    /// 공격 상태 초기화
    /// </summary>
    public void ResetAttackState()
    {
        if (isAttacking)
        {
            isAttacking = false;
            hasShot = false;
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