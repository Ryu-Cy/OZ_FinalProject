/// <summary>
/// 플레이어 세부 상태들을 캐싱하고 상태 전이를 담당하는 머신
/// </summary>
public class PlayerStateMachine : StateMachine
{
    public PlayerController Player { get; }

    // 개별 상태 캐싱
    public PlayerIdleState IdleState { get; }
    public PlayerMoveState MoveState { get; }

    /// <summary>
    /// 상태 머신 생성자
    /// </summary>
    /// <param name="player">플레이어 컨트롤러</param>
    public PlayerStateMachine(PlayerController player)
    {
        Player = player;

        // 상태 할당
        IdleState = new PlayerIdleState(this);
        MoveState = new PlayerMoveState(this);
    }
}