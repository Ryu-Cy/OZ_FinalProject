using System;
using UnityEngine;

/// <summary>
/// 적(Enemy) AI가 판단 및 실행 과정에서 실시간으로 공유하는 런타임 데이터 컨테이너
/// </summary>
[Serializable]
public class EnemyBlackboard : BTBlackboard
{
    [Header("에너미 데이터")]
    [Tooltip("적의 스탯 및 기본 설정 데이터 (ScriptableObject)")]
    [SerializeField] private EnemyData enemyData;

    [Header("타겟 정보")]
    [Tooltip("유효 타겟")]
    [SerializeField] private Transform target;

    [Tooltip("타겟을 놓친 마지막 위치")]
    private Vector3 lastTargetPosition;

    [Header("초기 위치")]
    [Tooltip("씬에 최초 배치되었던 월드 위치 (화톳불 리스폰 시 복귀 좌표)")]
    private Vector3 originPosition;

    [Tooltip("씬에 최초 배치되었을 때 바라보던 회전값")]
    private Quaternion originRotation;

    [Header("이동 목표 위치")]
    [Tooltip("현재 AI가 이동하려는 목표 위치")]
    private Vector3 destination;

    [Header("전투 및 공격 딜레이")]
    [Tooltip("다음 공격이 가능해지는 시점 (Time.time 기준)")]
    private float nextAttackTime;

    [Header("상태 플래그")]
    [Tooltip("사망")]
    [SerializeField] private bool isDead = false;

    #region Properties
    public EnemyData EnemyData
    {
        get => enemyData;
        set => enemyData = value;
    }

    public Transform Target
    {
        get => target;
        set
        {
            target = value;
            if (target != null)
            {
                lastTargetPosition = target.position;
            }
        }
    }

    public bool HasTarget => target != null;
    public Vector3 LastTargetPosition { get => lastTargetPosition; set => lastTargetPosition = value; }
    public Vector3 OriginPosition => originPosition;
    public Quaternion OriginRotation => originRotation;
    public Vector3 Destination { get => destination; set => destination = value; }
    public float NextAttackTime { get => nextAttackTime; set => nextAttackTime = value; }
    public bool IsAttackReady => Time.time >= nextAttackTime;
    public bool IsDead { get => isDead; set => isDead = value; }
    #endregion

    /// <summary>
    /// 게임 시작 시 씬에 배치된 최초 Transform 정보 캐싱
    /// </summary>
    public override void Initialize(Transform ownerTransform)
    {
        base.Initialize(ownerTransform);

        // 최초 배치된 위치와 방향을 앵커로 보관
        originPosition = ownerTransform.position;
        originRotation = ownerTransform.rotation;

        ResetState();
    }

    /// <summary>
    /// 화톳불 휴식 또는 전투 리셋 시 런타임 동적 상태 초기화
    /// </summary>
    public void ResetState()
    {
        destination = originPosition;
        lastTargetPosition = originPosition;
        target = null;
        nextAttackTime = 0.0f;
        isDead = false;
    }

    /// <summary>
    /// 타깃을 완전히 놓치거나 어그로가 풀렸을 때 호출
    /// </summary>
    public void ClearTarget()
    {
        target = null;
    }

    /// <summary>
    /// 공격 실행 후 다음 공격 대기 쿨다운 설정
    /// </summary>
    public void SetAttackCooldown(float cooldown)
    {
        nextAttackTime = Time.time + cooldown;
    }
}