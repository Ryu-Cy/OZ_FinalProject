using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 적 캐릭터의 행동과 상태를 관리하는 컨트롤러 클래스
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [Header("Data Source")]
    [SerializeField] private EnemyData _enemyData;

    [Header("Respawn Settings")]
    [Tooltip("사망 후 리스폰까지 대기 시간(초)")]
    [SerializeField] private float _respawnDelay = 5.0f;

    private EnemyHealth _enemyHealth;
    private NavMeshAgent _navMeshAgent;
    private Collider _enemyCollider;
    private BTRootNode _btRootNode;
    private EnemyVision _enemyVision;

    private GameObject _modelObject;
    private Animator _animator;
    private EnemyAnimationEventController _animEventController;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int DieHash = Animator.StringToHash("Die");

    private Coroutine _respawnCoroutine;

    // 프로퍼티
    public EnemyData EnemyData => _enemyData;
    public EnemyBlackboard Blackboard => _btRootNode != null ? _btRootNode.Blackboard as EnemyBlackboard : null;

    private void Awake()
    {
        _enemyHealth = GetComponent<EnemyHealth>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _enemyCollider = GetComponent<Collider>();
        _enemyVision = GetComponent<EnemyVision>();

        _animator = GetComponentInChildren<Animator>();
        if (_animator != null)
        {
            _modelObject = _animator.gameObject;
        }
        _animEventController = GetComponentInChildren<EnemyAnimationEventController>();

        if (_enemyVision != null && _enemyData != null)
        {
            _enemyVision.Initialize(_enemyData);
        }

        _btRootNode = GetComponentInChildren<EnemyBTRootNode>();
        BindDataToBlackboard();
    }

    private void Update()
    {
        UpdateLocomotionAnimation();
    }

    /// <summary>
    /// NavMeshAgent의 속도에 따라 애니메이션 파라미터 갱신
    /// </summary>
    private void UpdateLocomotionAnimation()
    {
        if (_animator == null || _navMeshAgent == null) return;
        _animator.SetFloat(SpeedHash, _navMeshAgent.velocity.magnitude);
    }

    /// <summary>
    /// 공격 애니메이션 트리거
    /// </summary>
    public void TriggerAttackAnimation()
    {
        if (_animator != null) _animator.SetTrigger(AttackHash);
    }

    /// <summary>
    /// 공격 애니메이션 종료 여부 확인
    /// </summary>
    public bool IsAttackFinished() => _animEventController != null && _animEventController.IsAttackFinished;
    /// <summary>
    /// 공격 상태 초기화
    /// </summary>
    public void ResetAttackState() => _animEventController?.ResetAttackState();

    /// <summary>
    /// 사망 시퀀스 시작
    /// </summary>
    public void StartDeathSequence()
    {
        if (CombatManager.HasInstance)
        {
            CombatManager.Instance.UnregisterEnemy(this);
        }

        if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh)
        {
            _navMeshAgent.isStopped = true;
            _navMeshAgent.velocity = Vector3.zero;
            _navMeshAgent.ResetPath();
        }

        if (_enemyCollider != null)
            _enemyCollider.enabled = false;

        _animEventController?.ResetDieState();

        if (_animator != null)
        {
            _animator.SetTrigger(DieHash);
        }
    }

    /// <summary>
    /// 사망 애니메이션 종료 확인
    /// </summary>
    public bool IsDieFinished() => _animEventController != null && _animEventController.IsDieFinished;

    /// <summary>
    /// 사망 시퀀스 완료 후 리스폰 준비
    /// </summary>
    public void CompleteDeath()
    {
        if (_modelObject != null)
        {
            _modelObject.SetActive(false);
        }

        if (_btRootNode != null)
        {
            _btRootNode.enabled = false;
        }

        if (_respawnCoroutine != null)
            StopCoroutine(_respawnCoroutine);

        _respawnCoroutine = StartCoroutine(RoutineRespawn());
    }

    /// <summary>
    /// 리스폰 루틴
    /// </summary>
    private IEnumerator RoutineRespawn()
    {
        yield return new WaitForSeconds(_respawnDelay);

        Vector3 spawnPosition = transform.position;
        if (_navMeshAgent != null)
        {
            _navMeshAgent.Warp(spawnPosition);
            _navMeshAgent.isStopped = false;
        }

        if (_enemyHealth != null)
            _enemyHealth.ResetHealth();

        if (_enemyCollider != null)
            _enemyCollider.enabled = true;

        if (_modelObject != null)
        {
            _modelObject.SetActive(true);
        }

        if (_btRootNode != null)
        {
            _btRootNode.enabled = true;
        }

        _respawnCoroutine = null;
    }

    /// <summary>
    /// Blackboard에 EnemyData 바인딩
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
}