using UnityEngine;

/// <summary>
/// 플레이어 이동(Move) 상태
/// </summary>
public class PlayerMoveState : PlayerBaseState
{
    /// <summary>
    /// Move 상태 생성자
    /// </summary>
    /// <param name="stateMachine">플레이어 상태 머신</param>
    public PlayerMoveState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    /// <summary>
    /// 상태 진입
    /// </summary>
    public override void Enter()
    {
        base.Enter();
    }

    /// <summary>
    /// 상태 로직
    /// 입력 방향으로 회전 및 이동
    /// 입력이 사라지면, IdleState로 전환
    /// </summary>
    public override void Update()
    {
        base.Update();

        if (player.InputVector.sqrMagnitude <= 0.01f)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
            return;
        }

        // 입력 세기를 Float 파라미터로 넘겨 블렌드 트리 제어
        float targetSpeedRatio = Mathf.Clamp01(player.InputVector.magnitude);
        player.UpdateAnimationSpeed(targetSpeedRatio);

        // 이동 방향 계산
        Vector3 moveDirection = new Vector3(player.InputVector.x, 0.0f, player.InputVector.y).normalized;

        // 회전 처리
        player.RotateTowards(moveDirection);

        // 컨트롤러에 이번 프레임의 목표 수평 속도 전달
        player.SetHorizontalVelocity(moveDirection * player.MoveSpeed);
    }

    /// <summary>
    /// 상태 종료
    /// </summary>
    public override void Exit()
    {
        base.Exit();
    }
}