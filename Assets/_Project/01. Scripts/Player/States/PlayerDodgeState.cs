using UnityEngine;

/// <summary>
/// 플레이어 전방 퀵시프트 회피(Dodge) 상태
/// </summary>
public class PlayerDodgeState : PlayerBaseState
{
    private static readonly int DodgeTagHash = Animator.StringToHash("Dodge");

    public override PlayerStateType StateType => PlayerStateType.Dodge;

    private Vector3 dodgeDirection;
    private float stateTimer;

    public PlayerDodgeState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    /// <summary>
    /// 상태 진입
    /// </summary>
    public override void Enter()
    {
        base.Enter();

        stateTimer = 0.0f;

        Vector2 input = player.InputController.InputVector;

        // 회피 입력 순간의 방향으로 즉시 회전, 없으면 정면 유지
        if (input.sqrMagnitude > 0.01f)
        {
            dodgeDirection = new Vector3(input.x, 0.0f, input.y).normalized;
            player.SnapRotation(dodgeDirection);
        }
        else
        {
            dodgeDirection = player.transform.forward;
        }

        player.PlayDodgeAnimation();
    }

    /// <summary>
    /// 상태 입력 처리
    /// </summary>
    public override void HandleInput()
    {
        // 회피 진행 중에는 입력을 차단
    }

    /// <summary>
    /// 상태 업데이트
    /// </summary>
    public override void Update()
    {
        base.Update();

        stateTimer += Time.deltaTime;

        AnimatorStateInfo stateInfo = player.Animator.GetCurrentAnimatorStateInfo(0);
        bool isCurrentStateDodge = stateInfo.tagHash == DodgeTagHash || stateInfo.IsName("Dodge");

        // 첫 프레임 트랜지션 오인식 방지 시간
        if (stateTimer > 0.08f && isCurrentStateDodge)
        {
            // 애니메이션이 완전히 끝났거나 다른 상태로 트랜지션이 시작되었을 때
            if (stateInfo.normalizedTime >= 1.0f || player.Animator.IsInTransition(0))
            {
                player.SetHorizontalVelocity(Vector3.zero);
                stateMachine.ChangeState(stateMachine.IdleState);
                return;
            }
        }

        // 회피 속도로 목표 방향 이동
        player.SetHorizontalVelocity(dodgeDirection * player.DodgeSpeed);
    }

    /// <summary>
    /// 상태 종료
    /// </summary>
    public override void Exit()
    {
        base.Exit();
        player.SetHorizontalVelocity(Vector3.zero);
    }
}