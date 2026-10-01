using System;
using UnityEngine;

/// <summary>
/// 적 AI(Node)가 런타임 중 실시간으로 공유하는 런타임 데이터 컨테이너
/// </summary>
[Serializable]
public class EnemyBlackboard : BTBlackboard
{
    [Header("에너미 데이터")]
    [Tooltip("적의 기본 데이터")]
    [SerializeField] private EnemyData _enemyData;

    [Header("타겟 정보")]
    [Tooltip("유효 타겟")]
    [SerializeField] private Transform _target;

    [Tooltip("타겟을 놓친 마지막 위치")]
    private Vector3 _lastTargetPosition;

    [Header("어그로 유지 타이머")]
    private float _lostTargetTimer = 0.0f;
    private bool _isTargetInSight = false;

    [Header("초기 위치")]
    [Tooltip("씬에 최초 배치되었던 월드 위치")]
    private Vector3 _originPosition;

    [Tooltip("씬에 최초 배치되었을 때 바라보던 회전값")]
    private Quaternion _originRotation;

    [Header("이동 목표 위치")]
    [Tooltip("현재 AI가 이동하려는 목표 위치")]
    private Vector3 _destination;

    [Header("전투 및 공격 딜레이")]
    [Tooltip("다음 공격이 가능해지는 시점")]
    private float _nextAttackTime;

    [Header("상태 플래그")]
    [Tooltip("사망")]
    [SerializeField] private bool _isDead = false;

    [Header("집단 전술")]
    [Tooltip("역할")]
    [SerializeField] private CombatRole _combatRole = CombatRole.None;

    [Tooltip("목표 좌표 (우회 지점/길막기 위치 등)")]
    private Vector3 _assignedPosition;

    [Tooltip("유효한 목표 좌표 할당 여부")]
    private bool _hasAssignedPosition = false;

    #region Properties
    public EnemyData EnemyData
    {
        get => _enemyData;
        set => _enemyData = value;
    }

    public Transform Target
    {
        get => _target;
        set
        {
            _target = value;
            if (_target != null)
            {
                _lastTargetPosition = _target.position;
                _lostTargetTimer = 0f;
                _isTargetInSight = true;
            }
        }
    }

    public bool HasTarget => _target != null;
    public Vector3 LastTargetPosition { get => _lastTargetPosition; set => _lastTargetPosition = value; }
    public Vector3 OriginPosition => _originPosition;
    public Quaternion OriginRotation => _originRotation;
    public Vector3 Destination { get => _destination; set => _destination = value; }
    public float NextAttackTime { get => _nextAttackTime; set => _nextAttackTime = value; }
    public bool IsAttackReady => Time.time >= _nextAttackTime;
    public bool IsAttacking { get; set; } = false;
    public bool IsDead { get => _isDead; set => _isDead = value; }

    // 집단 전술
    public CombatRole Role { get => _combatRole; set => _combatRole = value; }
    public Vector3 AssignedPosition { get => _assignedPosition; private set => _assignedPosition = value; }
    public bool HasAssignedPosition => _hasAssignedPosition;

    // 시야 상태
    public bool IsTargetInSight => _isTargetInSight;
    #endregion

    /// <summary>
    /// 게임 시작 시 씬에 배치된 최초 Transform 정보 캐싱
    /// </summary>
    public override void Initialize(Transform ownerTransform)
    {
        base.Initialize(ownerTransform);

        _originPosition = ownerTransform.position;
        _originRotation = ownerTransform.rotation;

        ResetState();
    }

    /// <summary>
    /// BTRootNode의 매 틱마다 호출되는 블랙보드 데이터 갱신
    /// </summary>
    public override void Update()
    {
        base.Update();

        // 타겟이 있지만 현재 시야에서 벗어난 경우 유예 시간 누적
        if (_target != null && !_isTargetInSight)
        {
            _lostTargetTimer += Time.deltaTime;

            float memoryDuration = _enemyData != null ? _enemyData.TargetMemoryDuration : 4.0f;

            // 유예 시간이 초과되면 완전히 타겟 제거
            if (_lostTargetTimer >= memoryDuration)
            {
                ClearTarget();
            }
        }
    }

    /// <summary>
    /// 시야 센서가 매 프레임 시야 확인 결과를 블랙보드에 알릴 때 호출
    /// </summary>
    public void NotifyTargetSightStatus(bool inSight, Vector3 currentKnownPos)
    {
        _isTargetInSight = inSight;

        if (inSight)
        {
            _lostTargetTimer = 0f;
            _lastTargetPosition = currentKnownPos;
        }
    }

    /// <summary>
    /// 화톳불 휴식 또는 전투 리셋 시 런타임 동적 상태 초기화
    /// </summary>
    public void ResetState()
    {
        _destination = _originPosition;
        _lastTargetPosition = _originPosition;
        _target = null;
        _nextAttackTime = 0.0f;
        _lostTargetTimer = 0f;
        _isTargetInSight = false;
        _isDead = false;

        ClearAssignedTactics();
    }

    /// <summary>
    /// 타겟을 완전히 놓치거나 어그로가 풀렸을 때 호출
    /// </summary>
    public void ClearTarget()
    {
        _target = null;
        _lostTargetTimer = 0f;
        _isTargetInSight = false;
        ClearAssignedTactics();
    }

    /// <summary>
    /// 공격 실행 후 다음 공격 대기 쿨다운 설정
    /// </summary>
    public void SetAttackCooldown(float cooldown)
    {
        _nextAttackTime = Time.time + cooldown;
    }

    /// <summary>
    /// 전술 이동 좌표를 부여할 때 호출
    /// </summary>
    public void SetAssignedPosition(Vector3 position)
    {
        _assignedPosition = position;
        _hasAssignedPosition = true;
    }

    /// <summary>
    /// 전술 역할 및 이동 좌표 초기화
    /// </summary>
    public void ClearAssignedTactics()
    {
        _combatRole = CombatRole.None;
        _hasAssignedPosition = false;
        _assignedPosition = Vector3.zero;
    }
}