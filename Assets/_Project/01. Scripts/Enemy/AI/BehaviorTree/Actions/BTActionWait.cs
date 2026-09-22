using UnityEngine;

/// <summary>
/// 지정된 시간 동안 제자리에 멈춰 대기하는 액션 노드
/// </summary>
public class BTActionWait : BTActionNode
{
    [Header("Wait Settings")]
    [Tooltip("대기 시간 (초 단위)")]
    [SerializeField] private float waitTime = 2.0f;

    private float elapsedTime = 0.0f;
    private bool isWaiting = false;

    #region Properties
    public float WaitTime
    {
        get => waitTime;
        set => waitTime = Mathf.Max(0.0f, value);
    }
    public float ElapsedTime => elapsedTime;
    #endregion

    protected override BTNodeState ExecuteAction()
    {
        // 대기 진입 시 타이머 초기화
        if (!isWaiting)
        {
            elapsedTime = 0.0f;
            isWaiting = true;
        }

        // 경과 시간 누적
        elapsedTime += Time.deltaTime;

        // 목표 대기 시간 도달 시 성공 반환 및 상태 정리
        if (elapsedTime >= waitTime)
        {
            ResetWaitState();
            return BTNodeState.Success;
        }

        // 대기 진행 중에는 Running 유지
        return BTNodeState.Running;
    }

    /// <summary>
    /// 타이머 초기화
    /// </summary>
    public void ResetWaitState()
    {
        elapsedTime = 0.0f;
        isWaiting = false;
    }
}