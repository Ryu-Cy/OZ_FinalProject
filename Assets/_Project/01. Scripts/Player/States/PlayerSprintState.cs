using UnityEngine;

/// <summary>
/// 플레이어 전력질주(Sprint) 상태
/// </summary>
public class PlayerSprintState : PlayerBaseState
{
    public override PlayerStateType StateType => PlayerStateType.Sprint;

    public PlayerSprintState(PlayerStateMachine stateMachine) : base(stateMachine) { }

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

        // 이동 입력 종료 시 Idle로 전환
        if (input.sqrMagnitude <= 0.01f)
        {
            stateMachine.ChangeState(stateMachine.IdleState);
            return;
        }

        // Sprint 입력 종료 시 Move로 전환
        if (!player.InputController.IsSprintPressed)
        {
            stateMachine.ChangeState(stateMachine.MoveState);
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

        Vector3 moveDirection = new Vector3(input.x, 0.0f, input.y).normalized;

        player.SetHorizontalVelocity(moveDirection * player.SprintSpeed);
        player.RotateTowards(moveDirection);
        player.UpdateAnimationSpeed(1.5f);
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