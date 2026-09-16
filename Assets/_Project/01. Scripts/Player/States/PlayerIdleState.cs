using UnityEngine;

/// <summary>
/// 플레이어 대기(Idle) 상태
/// </summary>
public class PlayerIdleState : PlayerBaseState
{
    public override PlayerStateType StateType => PlayerStateType.Idle;

    public PlayerIdleState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    /// <summary>
    /// 상태 진입
    /// 정지 처리
    /// </summary>
    public override void Enter()
    {
        base.Enter();

        player.SetHorizontalVelocity(Vector3.zero);
        player.UpdateAnimationSpeed(0.0f);

        // 회피 및 공격 입력 이벤트 구독
        player.InputController.OnDodgeTriggered += HandleDodge;
        player.InputController.OnAttackTriggered += HandleAttack;
    }

    /// <summary>
    /// 상태 입력 처리
    /// </summary>
    public override void HandleInput()
    {
        base.HandleInput();

        // 이동 입력 감지 시 Move 상태로 전환
        if (player.InputController.InputVector.sqrMagnitude > 0.01f)
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

        player.UpdateAnimationSpeed(0.0f);
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