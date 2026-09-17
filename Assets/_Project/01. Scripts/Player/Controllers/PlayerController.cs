using UnityEngine;

/// <summary>
/// 플레이어 컨트롤 담당 클래스
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputController))]
public class PlayerController : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int DodgeHash = Animator.StringToHash("Dodge");
    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");
    private static readonly int ComboIndexHash = Animator.StringToHash("ComboIndex");

    [Header("Debug")]
    [SerializeField] private string currentStateDisplay = "None"; // 캐릭터 현재 상태 확인용

    [Header("Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerInputController inputController;
    [SerializeField] private PlayerAnimationEventController animationEventController;

    [Header("Combat Settings")]
    [SerializeField] private Transform weaponParent; // 무기가 장착되는 부모 객체
    [SerializeField] private MeleeWeapon currentWeapon; // 현재 장착 중인 무기 컴포넌트

    [Header("Move Settings")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 5.0f;
    [SerializeField] private float sprintSpeed = 8.0f;
    [SerializeField] private float dodgeSpeed = 9.0f;
    [SerializeField] private float rotationSpeed = 12.0f;
    [SerializeField] private float gravity = -9.81f;

    // 내부 필드
    private Vector3 horizontalVelocity;
    private float verticalVelocity;

    // 프로퍼티
    public CharacterController CharacterController => characterController;
    public Animator Animator => animator;
    public PlayerInputController InputController => inputController;
    public PlayerAnimationEventController AnimationEventController => animationEventController;
    public MeleeWeapon CurrentWeapon => currentWeapon;
    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float SprintSpeed => sprintSpeed;
    public float DodgeSpeed => dodgeSpeed;
    public float RotationSpeed => rotationSpeed;
    public PlayerStateMachine StateMachine { get; private set; }

    private void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (inputController == null)
            inputController = GetComponent<PlayerInputController>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (animationEventController == null)
            animationEventController = GetComponentInChildren<PlayerAnimationEventController>();

        InitWeapon();

        StateMachine = new PlayerStateMachine(this);
        StateMachine.OnStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        if (StateMachine != null)
            StateMachine.OnStateChanged -= HandleStateChanged;
    }

    private void Start()
    {
        StateMachine.Initialize(StateMachine.IdleState);
    }

    /// <summary>
    /// 무기 컴포넌트 초기화 및 바인딩
    /// </summary>
    public void InitWeapon()
    {
        // 인스펙터에 직접 할당되어 있지 않다면 소켓 또는 자식에서 자동 탐색
        if (currentWeapon == null)
        {
            if (weaponParent != null)
            {
                currentWeapon = weaponParent.GetComponentInChildren<MeleeWeapon>();
            }
            else
            {
                currentWeapon = GetComponentInChildren<MeleeWeapon>();
            }
        }

        if (currentWeapon != null)
        {
            currentWeapon.DisableAttack();
        }
    }

    /// <summary>
    /// 상태 머신 업데이트
    /// 중력 적용 및 이동
    /// </summary>
    private void Update()
    {
        StateMachine.Update();

        ApplyGravity();

        Vector3 finalMovement = horizontalVelocity * Time.deltaTime;
        finalMovement.y = verticalVelocity * Time.deltaTime;
        characterController.Move(finalMovement);

        horizontalVelocity = Vector3.zero;
    }

    /// <summary>
    /// 상태(State)에서 호출하여 이번 프레임의 목표 수평 속도 벡터 설정
    /// </summary>
    public void SetHorizontalVelocity(Vector3 velocity)
    {
        horizontalVelocity = velocity;
    }

    /// <summary>
    /// 지정된 방향 캐릭터 회전 처리
    /// </summary>
    public void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 지정된 방향 캐릭터 즉시 회전 처리
    /// </summary>
    public void SnapRotation(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    /// <summary>
    /// 애니메이터의 Speed 파라미터를 감소시키며 갱신
    /// </summary>
    public void UpdateAnimationSpeed(float targetSpeed, float dampTime = 0.1f)
    {
        if (animator != null)
        {
            animator.SetFloat(SpeedHash, targetSpeed, dampTime, Time.deltaTime);
        }
    }

    /// <summary>
    /// 회피 애니메이션 트리거 실행
    /// </summary>
    public void PlayDodgeAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(DodgeHash);
        }
    }

    /// <summary>
    /// 공격 애니메이션 트리거 및 콤보 단계 설정
    /// </summary>
    public void PlayAttackAnimation(int comboIndex)
    {
        if (animator == null) return;
        animator.SetInteger(ComboIndexHash, comboIndex);
        animator.SetTrigger(AttackTriggerHash);
    }

    /// <summary>
    /// 무기 공격 판정 활성화
    /// </summary>
    public void EnableWeaponAttack()
    {
        if (currentWeapon != null)
        {
            currentWeapon.EnableAttack();
        }
    }

    /// <summary>
    /// 무기 공격 판정 비활성화
    /// </summary>
    public void DisableWeaponAttack()
    {
        if (currentWeapon != null)
        {
            currentWeapon.DisableAttack();
        }
    }

    /// <summary>
    /// 상태 변경 시 인스펙터 디버깅 문자열 갱신용
    /// </summary>
    private void HandleStateChanged(IState newState)
    {
        if (newState != null)
        {
            string stateName = newState.GetType().Name;
            currentStateDisplay = stateName.Replace("Player", "").Replace("State", "");
        }
        else
        {
            currentStateDisplay = "None";
        }
    }

    /// <summary>
    /// 지면 체크 및 수직 중력 가속도 연산
    /// </summary>
    private void ApplyGravity()
    {
        if (characterController.isGrounded && verticalVelocity < 0.0f)
        {
            verticalVelocity = -2.0f;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private void Reset()
    {
        characterController = GetComponent<CharacterController>();
        inputController = GetComponent<PlayerInputController>();
        animator = GetComponent<Animator>();
        animationEventController = GetComponentInChildren<PlayerAnimationEventController>();
    }
}