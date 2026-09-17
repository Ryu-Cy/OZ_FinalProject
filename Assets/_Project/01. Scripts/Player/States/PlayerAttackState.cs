using UnityEngine;

/// <summary>
/// 플레이어 공격(Attack) 상태
/// 최대 3타까지 콤보 공격 가능
/// </summary>
public class PlayerAttackState : PlayerBaseState
{
    private static readonly int AttackTagHash = Animator.StringToHash("Attack");
    private static readonly int MoveStateHash = Animator.StringToHash("Move");

    public override PlayerStateType StateType => PlayerStateType.Attack;

    private AttackComboType currentCombo = AttackComboType.None;

    // 이벤트 기반 상태 플래그
    private bool isComboWindowOpen;   // AttackHitEnd ~ ComboInputEnd 구간
    private bool hasComboReserved;    // 유효 윈도우 내 클릭 예약 여부
    private bool isAnimationStarted;
    private float stateTimer;

    public PlayerAttackState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    /// <summary>
    /// 상태 진입
    /// </summary>
    public override void Enter()
    {
        base.Enter();

        currentCombo = AttackComboType.Combo1;
        ExecuteAttack();

        player.InputController.OnAttackTriggered += HandleAttackInput;

        if (player.AnimationEventController != null)
        {
            player.AnimationEventController.OnAttackHitStart += HandleHitStart;
            player.AnimationEventController.OnAttackHitEnd += HandleHitEnd;
            player.AnimationEventController.OnComboInputEnd += HandleComboInputEnd;
        }
    }

    /// <summary>
    /// 상태 입력 처리
    /// </summary>
    public override void HandleInput()
    {
        // 공격 모션 중 입력 차단
    }

    /// <summary>
    /// 상태 업데이트
    /// </summary>
    public override void Update()
    {
        stateTimer += Time.deltaTime;

        // 공격 중 정지
        player.SetHorizontalVelocity(Vector3.zero);

        AnimatorStateInfo currentInfo = player.Animator.GetCurrentAnimatorStateInfo(0);
        bool isInAttack = currentInfo.tagHash == AttackTagHash || currentInfo.IsName($"Attack_{(int)currentCombo}");

        if (isInAttack)
        {
            isAnimationStarted = true;
        }

        // 트랜지션 초기 완충 시간 경과 후 탈출 감지
        if (stateTimer > 0.08f && isAnimationStarted)
        {
            // 모션이 끝나서 애니메이터가 Move상태로 전환되면 탈출
            if (player.Animator.IsInTransition(0))
            {
                AnimatorStateInfo nextInfo = player.Animator.GetNextAnimatorStateInfo(0);
                if (nextInfo.shortNameHash == MoveStateHash || nextInfo.tagHash != AttackTagHash)
                {
                    stateMachine.ChangeState(stateMachine.IdleState);
                    return;
                }
            }

            // 트랜지션 없이 모션dl 끝났을 때 즉시 탈출
            // 트렌지션 문제가 생겼을 때 탈출 용
            if (currentInfo.normalizedTime >= 1.0f)
            {
                stateMachine.ChangeState(stateMachine.IdleState);
            }
        }
    }

    /// <summary>
    /// 상태 종료
    /// </summary>
    public override void Exit()
    {
        base.Exit();

        // 구독 해제
        player.InputController.OnAttackTriggered -= HandleAttackInput;

        if (player.AnimationEventController != null)
        {
            player.AnimationEventController.OnAttackHitStart -= HandleHitStart;
            player.AnimationEventController.OnAttackHitEnd -= HandleHitEnd;
            player.AnimationEventController.OnComboInputEnd -= HandleComboInputEnd;
        }

        // 무기 판정 및 콤보 초기화
        player.DisableWeaponAttack();
        currentCombo = AttackComboType.None;
        player.PlayAttackAnimation((int)currentCombo);
        player.SetHorizontalVelocity(Vector3.zero);
    }

    private void ExecuteAttack()
    {
        stateTimer = 0.0f;
        isAnimationStarted = false;
        isComboWindowOpen = false;
        hasComboReserved = false;

        player.DisableWeaponAttack();

        // 공격 시작 시점에만 방향키 입력 방향으로 회전 정렬
        Vector2 input = player.InputController.InputVector;
        if (input.sqrMagnitude > 0.01f)
        {
            Vector3 targetDir = new Vector3(input.x, 0.0f, input.y).normalized;
            player.SnapRotation(targetDir);
        }

        player.PlayAttackAnimation((int)currentCombo);
    }

    private void HandleAttackInput()
    {
        // 콤보 윈도우 구간 내 입력만 체크
        if (isComboWindowOpen && !hasComboReserved)
        {
            hasComboReserved = true;
            isComboWindowOpen = false;
        }
    }

    private void HandleHitStart()
    {
        player.EnableWeaponAttack();
    }

    private void HandleHitEnd()
    {
        player.DisableWeaponAttack();
        isComboWindowOpen = true;
    }

    private void HandleComboInputEnd()
    {
        isComboWindowOpen = false;

        // 다음 콤보 입력이 있고 3타 미만이면 다음 콤보로 전진
        if (hasComboReserved && currentCombo < AttackComboType.Combo3)
        {
            currentCombo++;
            ExecuteAttack();
        }
    }
}