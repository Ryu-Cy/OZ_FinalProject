using UnityEngine;

/// <summary>
/// 적 전용 데이터 베이스 클래스
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "Character/Data/Enemy Data")]
public class EnemyData : CharacterData
{
    [Header("시야 및 감지 정보 (FOV)")]
    [Tooltip("시야 감지 최대 거리")]
    [SerializeField] private float viewDistance = 10.0f;

    [Tooltip("시야 감지 각도 (전방 기준 좌우 합산 각도)")]
    [Range(0.0f, 360.0f)]
    [SerializeField] private float viewAngle = 120.0f;

    [Tooltip("타겟 레이어마스크")]
    [SerializeField] private LayerMask targetLayer;

    [Tooltip("장애물 레이어마스크")]
    [SerializeField] private LayerMask obstacleLayer;

    [Header("전투 및 탐색 정보")]
    [Tooltip("기본 공격 사거리")]
    [SerializeField] private float attackRange = 2.0f;

    [Tooltip("공격 후 다음 공격까지의 대기 시간")]
    [SerializeField] private float attackCooldown = 2.5f;

    [Tooltip("타깃 발견 후 추적 이동 속도")]
    [SerializeField] private float chaseSpeed = 4.5f;

    #region Properties
    public float ViewDistance => viewDistance;
    public float ViewAngle => viewAngle;
    public LayerMask TargetLayer => targetLayer;
    public LayerMask ObstacleLayer => obstacleLayer;
    public float AttackRange => attackRange;
    public float AttackCooldown => attackCooldown;
    public float ChaseSpeed => chaseSpeed;
    #endregion
}