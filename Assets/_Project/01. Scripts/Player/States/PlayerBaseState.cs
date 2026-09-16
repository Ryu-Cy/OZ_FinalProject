/// <summary>
/// 플레이어 전용 상태 베이스 클래스
/// </summary>
public abstract class PlayerBaseState : IState
{
    protected readonly PlayerStateMachine stateMachine;
    protected readonly PlayerController player;

    /// <summary>
    /// 플레이어 상태 베이스 생성자
    /// </summary>
    /// <param name="stateMachine">플레이어 상태 머신</param>
    protected PlayerBaseState(PlayerStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        this.player = stateMachine.Player;
    }

    /// <summary>
    /// 상태 진입
    /// </summary>
    public virtual void Enter() { }

    /// <summary>
    /// 상태 로직
    /// </summary>
    public virtual void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// 입력 감지
    /// </summary>
    public virtual void HandleInput() { }

    /// <summary>
    /// 상태 종료
    /// </summary>
    public virtual void Exit() { }
}
