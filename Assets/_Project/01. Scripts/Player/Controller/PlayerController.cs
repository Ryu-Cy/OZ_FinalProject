using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 플레이어 컨트롤 담당 클래스. 상태 머신 구동, 입력 수신, 물리(중력 및 이동) 최종 결합 담당
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    [Header("Debug")]
    [SerializeField] private string currentStateDisplay = "None";   // 캐릭터 현재 상태 확인용

    [Header("Components")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 5.0f;
    [SerializeField] private float rotationSpeed = 12.0f;
    [SerializeField] private float gravity = -9.81f;

    // 내부 필드
    private PlayerInputActions inputActions;
    private Vector2 inputVector;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;

    // 프로퍼티
    public CharacterController CharacterController => characterController;
    public Animator Animator => animator;
    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;
    public Vector2 InputVector => inputVector;
    public PlayerStateMachine StateMachine { get; private set; }
    public PlayerInputActions InputActions => inputActions;

    /// <summary>
    /// 컴포넌트 캐싱, 인풋 액션 인스턴스화 및 상태 머신 구성
    /// </summary>
    private void Awake()
    {
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponent<Animator>();

        inputActions = new PlayerInputActions();

        StateMachine = new PlayerStateMachine(this);
        StateMachine.OnStateChanged += HandleStateChanged;
    }

    /// <summary>
    /// 인풋 액션 활성화 및 Move 이벤트 콜백 등록
    /// </summary>
    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
    }

    /// <summary>
    /// Move 이벤트 콜백 해제 및 인풋 액션 비활성화
    /// </summary>
    private void OnDisable()
    {
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;

        inputActions.Disable();
    }

    /// <summary>
    /// 상태 머신 이벤트 구독 해제
    /// </summary>
    private void OnDestroy()
    {
        if (StateMachine != null)
            StateMachine.OnStateChanged -= HandleStateChanged;
    }

    /// <summary>
    /// 기본 시작 상태(Idle) 진입
    /// </summary>
    private void Start()
    {
        StateMachine.Initialize(StateMachine.IdleState);
    }

    /// <summary>
    /// 상태 머신 갱신, 상시 중력 연산, 수평/수직 물리 이동을 결합하여 매 프레임 단 1회 실행
    /// </summary>
    private void Update()
    {
        // 1. 현재 상태 로직 실행 (필요한 경우 SetHorizontalVelocity 호출)
        StateMachine.Update();

        // 2. 상시 중력 가속도 연산
        ApplyGravity();

        // 3. 수평 속도와 수직 낙하 속도를 합산해 단 1회 최종 이동 실행
        Vector3 finalMovement = horizontalVelocity * Time.deltaTime;
        finalMovement.y = verticalVelocity * Time.deltaTime;
        characterController.Move(finalMovement);

        // 4. 다음 프레임 연산을 위해 수평 속도 초기화 (명시적으로 속도를 주지 않는 상태는 0으로 정지)
        horizontalVelocity = Vector3.zero;
    }

    /// <summary>
    /// 상태(State)에서 호출하여 이번 프레임의 목표 수평 속도 벡터 설정
    /// </summary>
    /// <param name="velocity">XZ 평면 수평 속도 벡터</param>
    public void SetHorizontalVelocity(Vector3 velocity)
    {
        horizontalVelocity = velocity;
    }

    /// <summary>
    /// 지정된 방향으로 캐릭터를 부드럽게 회전 처리
    /// </summary>
    /// <param name="direction">바라볼 목표 방향 벡터</param>
    public void RotateTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// 애니메이터의 Speed 파라미터를 부드럽게 감쇠(Damping)시키며 갱신
    /// </summary>
    /// <param name="targetSpeed">목표 속도 비율 (0.0: 정지, 1.0: 최대 속도)</param>
    /// <param name="dampTime">보간 시간 (기본 0.1초)</param>
    public void UpdateAnimationSpeed(float targetSpeed, float dampTime = 0.1f)
    {
        if (animator != null)
        {
            animator.SetFloat(SpeedHash, targetSpeed, dampTime, Time.deltaTime);
        }
    }

    /// <summary>
    /// 이동 키 입력 감지 시 방향 벡터 갱신
    /// </summary>
    /// <param name="context">인풋 시스템 콜백 컨텍스트</param>
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        inputVector = context.ReadValue<Vector2>();
    }

    /// <summary>
    /// 이동 키 입력 해제 시 방향 벡터 초기화
    /// </summary>
    /// <param name="context">인풋 시스템 콜백 컨텍스트</param>
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        inputVector = Vector2.zero;
    }

    /// <summary>
    /// 상태 변경 시 인스펙터 디버깅 문자열 갱신
    /// </summary>
    /// <param name="newState">새로 전환된 상태</param>
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
    /// CharacterController의 지면 체크 및 수직 중력 가속도 상시 연산
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
        animator = GetComponent<Animator>();
    }


    /// <summary>
    /// 애니메이션 클립 이벤트 수신용
    /// 에셋 애니메이션 클립에 설정되어 있는 이벤트 함수라 에러 방지를 위해 더미 함수로 일단 선언.
    /// 추후 활용할 가능성이 있기에 남겨둠.
    /// 필요없다고 판단되면 애니메이션 클립에서도 지울 예정.
    /// </summary>
    public void SwitchSocket()
    {

    }
}