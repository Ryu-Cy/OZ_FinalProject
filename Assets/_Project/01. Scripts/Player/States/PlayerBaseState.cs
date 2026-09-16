/// <summary>
/// 플레이어 모든 상태 클래스의 최상위 추상 베이스
/// </summary>
public abstract class PlayerBaseState : IState
{
    protected PlayerStateMachine stateMachine;
    protected PlayerController player;

    // 각 상태별 고유 식별 열거형
    public abstract PlayerStateType StateType { get; }

    public PlayerBaseState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        this.player = stateMachine.Player;
    }

    /// <summary>
    /// 상태 진입
    /// </summary>
    public virtual void Enter() { }

    /// <summary>
    /// 상태 실제 실행 로직
    /// </summary>
    public virtual void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// 입력 처리
    /// </summary>
    public virtual void HandleInput() { }

    /// <summary>
    /// 상태 종료
    /// </summary>
    public virtual void Exit() { }

    // 공통 회피 전이
    protected virtual void HandleDodge()
    {
        stateMachine.ChangeState(stateMachine.DodgeState);
    }

    // 공통 공격 전이
    protected virtual void HandleAttack()
    {
        stateMachine.ChangeState(stateMachine.AttackState);
    }
}