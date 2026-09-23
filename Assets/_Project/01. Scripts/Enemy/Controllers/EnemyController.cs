using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 적 캐릭터의 생명주기 및 하위 컴포넌트를 총괄 제어하는 메인 컨트롤러
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [Header("Data Source")]
    [Tooltip("적 능력치 원본")]
    [SerializeField] private EnemyData enemyData;

    [Header("Respawn Settings")]
    [Tooltip("사망 후 리스폰까지 대기하는 시간 (초)")]
    [SerializeField] private float respawnDelay = 5.0f;

    private EnemyHealth enemyHealth;
    private NavMeshAgent navMeshAgent;
    private Collider enemyCollider;
    private MeshRenderer meshRenderer;
    private BTRootNode btRootNode;
    private EnemyVision enemyVision;

    private Coroutine respawnCoroutine;

    public EnemyData EnemyData => enemyData;
    public EnemyBlackboard Blackboard => btRootNode != null ? btRootNode.Blackboard as EnemyBlackboard : null;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyCollider = GetComponent<Collider>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        enemyVision = GetComponent<EnemyVision>();
        if (enemyVision != null && enemyData != null)
        {
            enemyVision.Initialize(enemyData);
        }

        btRootNode = GetComponentInChildren<EnemyBTRootNode>();
        if (btRootNode != null && btRootNode.Blackboard is EnemyBlackboard enemyBlackboard)
        {
            enemyBlackboard.EnemyData = enemyData;
        }
    }

    private void OnEnable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath -= HandleDeath;
        }

        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }
    }

    private void HandleDeath()
    {
        // 트리 정지
        if (btRootNode != null)
            btRootNode.enabled = false;

        // 길찾기 정지
        if (navMeshAgent != null && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
        }

        // 콜라이더 및 렌더러 끄기
        if (enemyCollider != null)
            enemyCollider.enabled = false;

        if (meshRenderer != null)
            meshRenderer.enabled = false;

        // 5초 뒤 리스폰
        if (respawnCoroutine != null)
            StopCoroutine(respawnCoroutine);

        respawnCoroutine = StartCoroutine(RoutineRespawn());
    }

    private IEnumerator RoutineRespawn()
    {
        yield return new WaitForSeconds(respawnDelay);

        // 초기 위치로 워프
        Vector3 spawnPosition = transform.position;
        if (navMeshAgent != null)
        {
            navMeshAgent.Warp(spawnPosition);
            navMeshAgent.isStopped = false;
        }

        // 체력 복구
        if (enemyHealth != null)
            enemyHealth.ResetHealth();

        // 콜라이더 및 렌더러 복원
        if (enemyCollider != null)
            enemyCollider.enabled = true;

        if (meshRenderer != null)
            meshRenderer.enabled = true;

        // 비헤이비어 트리 재가동
        if (btRootNode != null)
            btRootNode.enabled = true;

        respawnCoroutine = null;
    }
}