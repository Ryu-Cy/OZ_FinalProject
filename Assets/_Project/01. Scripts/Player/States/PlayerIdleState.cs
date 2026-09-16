using UnityEngine;

/// <summary>
/// 플레이어 대기(Idle) 상태
/// </summary>
public class PlayerIdleState : PlayerBaseState
{
    /// <summary>
    /// Idle 상태 생성자
    /// </summary>
    /// <param name="stateMachine">플레이어 상태 머신</param>
    public PlayerIdleState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    /// <summary>
    /// 상태 진입
    /// </summary>
    public override void Enter()
    {
        base.Enter();
    }

    /// <summary>
    /// 상태 로직
    /// 방향 입력 시 MoveState로 전환
    /// </summary>
    public override void Update()
    {
        base.Update();

        // 애니메이터 파라미터 조절
        player.UpdateAnimationSpeed(0.0f);

        // 입력이 감지되면 MoveState로 전환
        if (player.InputVector.sqrMagnitude > 0.01f)
        {
            stateMachine.ChangeState(stateMachine.MoveState);
        }
    }

    /// <summary>
    /// 상태 종료
    /// </summary>
    public override void Exit()
    {
        base.Exit();
    }
}