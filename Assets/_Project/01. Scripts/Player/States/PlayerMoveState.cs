using UnityEngine;

/// <summary>
/// 플레이어 기본 이동(Move) 상태
/// </summary>
public class PlayerMoveState : PlayerBaseState
{
    public override PlayerStateType StateType => PlayerStateType.Move;

    public PlayerMoveState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    /// <summary>
    /// 상태 진입
    /// </summary>
    public override void Enter()
    {
        base.Enter();

        player.InputController.OnDodgeTriggered += HandleDodge;
        player.InputController.OnAttackTriggered += HandleAttack;
    }

    /// <summary>
    /// 상태 입력 처리
    /// </summary>
    public override void HandleInput()
    {
        base.HandleInput();

        Vector2 input = player.InputController.InputVector;

        // 이동 입력이 멈추면 Idle로 전환
        if (input.sqrMagnitude <= 0.01f)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
            return;
        }

        // LShift 누름 감지 시 Sprint로 전환
        if (player.InputController.IsSprintPressed)
        {
            stateMachine.ChangeState(stateMachine.SprintState);
        }
    }

    /// <summary>
    /// 상태 업데이트
    /// </summary>
    public override void Update()
    {
        base.Update();

        Vector2 input = player.InputController.InputVector;
        if (input.sqrMagnitude <= 0.01f) return;

        // 카메라 시선 방향 기준의 이동 벡터 계산
        Vector3 moveDirection = player.GetCameraRelativeMovement(input);

        // 조작 방식에 따른 속도 및 애니메이션 값 처리
        float targetSpeed = player.InputController.IsWalkPressed ? player.WalkSpeed : player.RunSpeed;
        float animSpeedValue = player.InputController.IsWalkPressed ? 0.5f : 1.0f;

        player.SetHorizontalVelocity(moveDirection * targetSpeed);
        player.RotateTowards(moveDirection);
        player.UpdateAnimationSpeed(animSpeedValue);
    }

    /// <summary>
    /// 상태 종료
    /// </summary>
    public override void Exit()
    {
        base.Exit();

        player.InputController.OnDodgeTriggered -= HandleDodge;
        player.InputController.OnAttackTriggered -= HandleAttack;
    }
}