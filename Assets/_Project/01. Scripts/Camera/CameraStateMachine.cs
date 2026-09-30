using UnityEngine;

/// <summary>
/// 카메라의 상태 전환 및 라이프사이클을 통괄하는 클래스
/// </summary>
public class CameraStateMachine : MonoBehaviour
{
    [Header("State Components")]
    [SerializeField] private CameraFreeLookState freeLookState;
    [SerializeField] private CameraLockOnState lockOnState;

    [Header("References")]
    [SerializeField] private PlayerLockOnController lockOnController;

    private CameraStateType currentState = CameraStateType.FreeLook;
    public CameraStateType CurrentState => currentState;

    private void Awake()
    {
        // 동일 오브젝트 내 컴포넌트 자동 캐싱
        if (freeLookState == null) freeLookState = GetComponent<CameraFreeLookState>();
        if (lockOnState == null) lockOnState = GetComponent<CameraLockOnState>();
    }

    private void OnEnable()
    {
        if (lockOnController != null)
        {
            lockOnController.OnLockOnTargetChanged += HandleLockOnTargetChanged;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCursorLockChanged += HandleCursorLockChanged;
        }
    }

    private void Start()
    {
        // 초기 시작 상태
        ChangeState(CameraStateType.FreeLook);

        bool isCurrentLocked = (Cursor.lockState == CursorLockMode.Locked);
        HandleCursorLockChanged(isCurrentLocked);
    }

    private void OnDisable()
    {
        if (lockOnController != null)
        {
            lockOnController.OnLockOnTargetChanged -= HandleLockOnTargetChanged;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCursorLockChanged -= HandleCursorLockChanged;
        }
    }

    private void HandleLockOnTargetChanged(LockOnTarget target)
    {
        if (target != null)
        {
            // 락온 타겟이 지정되면 타겟 정보를 넘겨주며 락온 상태로 전환
            if (lockOnState != null)
            {
                lockOnState.SetTarget(target.TargetTransform);
            }
            ChangeState(CameraStateType.LockOn);
        }
        else
        {
            // 락온 해제 시 자유 시점으로 복귀
            if (lockOnState != null)
            {
                lockOnState.ClearTarget();
            }
            ChangeState(CameraStateType.FreeLook);
        }
    }

    private void HandleCursorLockChanged(bool isLocked)
    {
        // 자유 시점 상태일 때만 커서 락 상태에 맞춰 마우스 입력 On/Off 동기화
        if (currentState == CameraStateType.FreeLook && freeLookState != null)
        {
            freeLookState.SetInputActive(isLocked);
        }
    }

    /// <summary>
    /// 상태 컴포넌트 활성화/비활성화 전환
    /// </summary>
    public void ChangeState(CameraStateType newState)
    {
        currentState = newState;

        switch (newState)
        {
            case CameraStateType.FreeLook:
                if (lockOnState != null) lockOnState.enabled = false;
                if (freeLookState != null)
                {
                    freeLookState.enabled = true;
                    bool isCurrentLocked = (Cursor.lockState == CursorLockMode.Locked);
                    freeLookState.SetInputActive(isCurrentLocked);
                }
                break;

            case CameraStateType.LockOn:
                if (freeLookState != null)
                {
                    freeLookState.SetInputActive(false);
                    freeLookState.enabled = false;
                }
                if (lockOnState != null)
                {
                    lockOnState.enabled = true;
                }
                break;
        }
    }
}