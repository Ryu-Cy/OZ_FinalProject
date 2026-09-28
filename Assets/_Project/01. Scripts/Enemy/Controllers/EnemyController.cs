using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 적의 주기 및 하위 컴포넌트를 총괄 제어하는 메인 컨트롤러 <br/>
/// 사망 등 처리는 추후 옮겨야할듯.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [Header("Data Source")]
    [Tooltip("적 능력치 원본")]
    [SerializeField] private EnemyData _enemyData;

    [Header("Respawn Settings")]
    [Tooltip("사망 후 리스폰까지 대기하는 시간(초)")]
    [SerializeField] private float _respawnDelay = 5.0f;

    private EnemyHealth _enemyHealth;
    private NavMeshAgent _navMeshAgent;
    private Collider _enemyCollider;
    private MeshRenderer _meshRenderer;
    private BTRootNode _btRootNode;
    private EnemyVision _enemyVision;

    private Coroutine _respawnCoroutine;

    public EnemyData EnemyData => _enemyData;
    public EnemyBlackboard Blackboard => _btRootNode != null ? _btRootNode.Blackboard as EnemyBlackboard : null;

    private void Awake()
    {
        _enemyHealth = GetComponent<EnemyHealth>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _enemyCollider = GetComponent<Collider>();
        _meshRenderer = GetComponentInChildren<MeshRenderer>();
        _enemyVision = GetComponent<EnemyVision>();

        if (_enemyVision != null && _enemyData != null)
        {
            _enemyVision.Initialize(_enemyData);
        }

        _btRootNode = GetComponentInChildren<EnemyBTRootNode>();

        // 1차 바인딩 시도
        BindDataToBlackboard();
    }

    private void Start()
    {
        // 2차 바인딩 시도
        BindDataToBlackboard();
    }

    /// <summary>
    /// 블랙보드에 EnemyData 에셋 주입
    /// </summary>
    private void BindDataToBlackboard()
    {
        if (_btRootNode != null && _btRootNode.Blackboard is EnemyBlackboard enemyBlackboard)
        {
            if (enemyBlackboard.EnemyData == null && _enemyData != null)
            {
                enemyBlackboard.EnemyData = _enemyData;
            }
        }
    }

    private void OnEnable()
    {
        if (_enemyHealth != null)
        {
            _enemyHealth.OnDeath += HandleDeath;
        }
    }

    private void OnDisable()
    {
        if (_enemyHealth != null)
        {
            _enemyHealth.OnDeath -= HandleDeath;
        }

        if (_respawnCoroutine != null)
        {
            StopCoroutine(_respawnCoroutine);
            _respawnCoroutine = null;
        }

        // 오브젝트 비활성화 시 CombatManager에서 등록 해제
        if (CombatManager.HasInstance)
        {
            CombatManager.Instance.UnregisterEnemy(this);
        }
    }

    private void HandleDeath()
    {
        // 사망 시 CombatManager 등록 해제
        if (CombatManager.HasInstance)
        {
            CombatManager.Instance.UnregisterEnemy(this);
        }

        // 트리 정지
        if (_btRootNode != null)
            _btRootNode.enabled = false;

        // 길찾기 정지
        if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.isStopped = true;
            _navMeshAgent.ResetPath();
        }

        // 콜라이더 및 렌더러 끄기
        if (_enemyCollider != null)
            _enemyCollider.enabled = false;

        if (_meshRenderer != null)
            _meshRenderer.enabled = false;

        // 리스폰 코루틴 가동
        if (_respawnCoroutine != null)
            StopCoroutine(_respawnCoroutine);

        _respawnCoroutine = StartCoroutine(RoutineRespawn());
    }

    private IEnumerator RoutineRespawn()
    {
        yield return new WaitForSeconds(_respawnDelay);

        // 초기 위치로 복귀
        Vector3 spawnPosition = transform.position;
        if (_navMeshAgent != null)
        {
            _navMeshAgent.Warp(spawnPosition);
            _navMeshAgent.isStopped = false;
        }

        // 체력 복구
        if (_enemyHealth != null)
            _enemyHealth.ResetHealth();

        // 콜라이더 및 렌더러 복원
        if (_enemyCollider != null)
            _enemyCollider.enabled = true;

        if (_meshRenderer != null)
            _meshRenderer.enabled = true;

        // 비헤이비어 트리 재가동
        if (_btRootNode != null)
            _btRootNode.enabled = true;

        _respawnCoroutine = null;
    }
}