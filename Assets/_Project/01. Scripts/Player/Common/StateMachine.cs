using System;

/// <summary>
/// 현재 상태의 라이프사이클과 상태 전환을 담당하는 상태 머신
/// </summary>
public abstract class StateMachine
{
    public IState CurrentState { get; private set; }

    // 상태 변경 알림 이벤트
    public event Action<IState> OnStateChanged;

    /// <summary>
    /// 상태 초기화 함수
    /// </summary>
    /// <param name="startingState">초기 상태</param>
    public void Initialize(IState startingState)
    {
        CurrentState = startingState;
        CurrentState?.Enter();
        OnStateChanged?.Invoke(CurrentState);
    }

    /// <summary>
    /// 상태 전환 함수
    /// </summary>
    /// <param name="newState">전환될 상태</param>
    public void ChangeState(IState newState)
    {
        if (newState == null || CurrentState == newState)
            return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
        OnStateChanged?.Invoke(CurrentState);
    }

    /// <summary>
    /// 상태 머신의 매 프레임 호출 함수
    /// </summary>
    public void Update()
    {
        CurrentState?.Update();
    }
}