using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 순찰 지점의 좌표와 대기 여부를 담는 구조체
/// </summary>
[Serializable]
public struct PatrolPointData
{
    [Tooltip("순찰 지점 좌표")]
    public Vector3 Position;

    [Tooltip("도착 시 대기 여부")]
    public bool ShouldWait;

    public PatrolPointData(Vector3 position, bool shouldWait = true)
    {
        Position = position;
        ShouldWait = shouldWait;
    }
}

/// <summary>
/// 지정된 순찰 지점 목록을 순회하며 이동하는 액션 노드
/// </summary>
public class BTActionPatrol : BTActionNode
{
    [Header("Patrol Points")]
    [Tooltip("순찰 지점 목록")]
    [SerializeField] private List<PatrolPointData> patrolPoints = new List<PatrolPointData>();

    [Header("Patrol Options")]
    [Tooltip("체크 시 왕복(A->B->C->B->A), 미체크 시 순환(A->B->C->A)")]
    [SerializeField] private bool isPingPong = true;

    [Tooltip("도착 판정 거리")]
    [SerializeField] private float arrivalDistance = 0.5f;

    private EnemyBlackboard enemyBlackboard;
    private NavMeshAgent navAgent;

    private int currentPointIndex = 0;
    private int directionModifier = 1;
    private bool hasDestination = false;

    #region Properties
    public List<PatrolPointData> PatrolPoints
    {
        get => patrolPoints;
        set => patrolPoints = value;
    }
    public int CurrentPointIndex => currentPointIndex;
    public bool IsPingPong
    {
        get => isPingPong;
        set => isPingPong = value;
    }
    #endregion

    public override void Initialize(BTBlackboard blackboard)
    {
        base.Initialize(blackboard);

        enemyBlackboard = blackboard as EnemyBlackboard;

        if (enemyBlackboard?.Owner != null)
        {
            navAgent = enemyBlackboard.Owner.GetComponent<NavMeshAgent>();
        }
    }

    protected override BTNodeState ExecuteAction()
    {
        if (enemyBlackboard == null)
            return BTNodeState.Failure;

        if (navAgent == null)
        {
            navAgent = GetComponentInParent<NavMeshAgent>();
            if (navAgent == null)
                return BTNodeState.Failure;
        }

        if (patrolPoints == null || patrolPoints.Count == 0)
            return BTNodeState.Failure;

        // 네브메시 활성화 및 정지 플래그 강제 해제 보장
        if (navAgent.isOnNavMesh && navAgent.isStopped)
        {
            navAgent.isStopped = false;
        }

        // EnemyData에 정의된 순찰 이동 속도로 복구 (속도가 0으로 잠기는 현상 방지)
        if (enemyBlackboard.EnemyData != null)
        {
            navAgent.speed = enemyBlackboard.EnemyData.MoveSpeed;
        }

        // 목적지가 없거나, 경로가 지워졌거나, 목적지와의 거리가 안 잡힐 때 새로 갱신
        if (!hasDestination || (!navAgent.pathPending && !navAgent.hasPath))
        {
            SetDestinationToCurrentPoint();
            return BTNodeState.Running;
        }

        // 경로 계산 중 대기
        if (navAgent.pathPending)
            return BTNodeState.Running;

        // 목표 지점 도착 판정
        if (navAgent.remainingDistance <= arrivalDistance)
        {
            PatrolPointData arrivedPoint = patrolPoints[currentPointIndex];

            // 다음 순찰 포인트 인덱스로 갱신
            AdvanceToNextPoint();

            // 대기 지점인 경우
            if (arrivedPoint.ShouldWait)
            {
                StopMovement();
                hasDestination = false;
                return BTNodeState.Success;
            }

            // 대기하지 않는 지점인 경우
            SetDestinationToCurrentPoint();
            return BTNodeState.Running;
        }

        return BTNodeState.Running;
    }

    /// <summary>
    /// 현재 인덱스의 지점으로 이동 경로 지정
    /// </summary>
    private void SetDestinationToCurrentPoint()
    {
        Vector3 targetPosition = patrolPoints[currentPointIndex].Position;
        enemyBlackboard.Destination = targetPosition;

        navAgent.isStopped = false;
        navAgent.SetDestination(targetPosition);
        hasDestination = true;
    }

    /// <summary>
    /// 순환 또는 왕복 규칙에 따라 다음 순찰 지점 인덱스 계산
    /// </summary>
    private void AdvanceToNextPoint()
    {
        if (patrolPoints.Count <= 1)
            return;

        if (isPingPong)
        {
            currentPointIndex += directionModifier;

            if (currentPointIndex >= patrolPoints.Count)
            {
                currentPointIndex = patrolPoints.Count - 2;
                directionModifier = -1;
            }
            else if (currentPointIndex < 0)
            {
                currentPointIndex = 1;
                directionModifier = 1;
            }
        }
        else
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Count;
        }
    }

    /// <summary>
    /// 이동 정지 및 경로 초기화
    /// </summary>
    private void StopMovement()
    {
        if (navAgent != null && navAgent.isOnNavMesh)
        {
            navAgent.isStopped = true;
            navAgent.ResetPath();
        }
    }

    private void OnDisable()
    {
        hasDestination = false;
    }
}