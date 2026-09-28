using UnityEngine;

/// <summary>
/// 적 전용 데이터 베이스 클래스
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Character/Data/Enemy Data")]
public class EnemyData : CharacterData
{
    [Header("전투 분류")]
    [Tooltip("전투 타입(근거리/원거리) 구분")]
    [SerializeField] private EnemyCombatType _combatType = EnemyCombatType.Melee;

    [Header("시야 및 감지 정보 (FOV)")]
    [Tooltip("시야 감지 최대 거리")]
    [SerializeField] private float _viewDistance = 10.0f;

    [Tooltip("시야 감지 각도")]
    [Range(0.0f, 360.0f)]
    [SerializeField] private float _viewAngle = 120.0f;

    [Tooltip("타겟 레이어마스크")]
    [SerializeField] private LayerMask _targetLayer;

    [Tooltip("장애물 레이어마스크")]
    [SerializeField] private LayerMask _obstacleLayer;

    [Header("전투 및 탐색 정보")]
    [Tooltip("기본 공격 사거리")]
    [SerializeField] private float _attackRange = 2.0f;

    [Tooltip("공격 후 대기 시간")]
    [SerializeField] private float _attackCooldown = 2.5f;

    [Tooltip("타깃 발견 후 추적 이동 속도")]
    [SerializeField] private float _chaseSpeed = 4.5f;

    [Header("어그로 유지 시간")]
    [Tooltip("시야에서 사라진 후 유예 시간 (초)")]
    [SerializeField] private float _targetMemoryDuration = 4.0f;

    #region Properties
    public EnemyCombatType CombatType => _combatType;
    public float ViewDistance => _viewDistance;
    public float ViewAngle => _viewAngle;
    public LayerMask TargetLayer => _targetLayer;
    public LayerMask ObstacleLayer => _obstacleLayer;
    public float AttackRange => _attackRange;
    public float AttackCooldown => _attackCooldown;
    public float ChaseSpeed => _chaseSpeed;
    public float TargetMemoryDuration => _targetMemoryDuration;
    #endregion
}