using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// 인게임 메인 카메라의 동작 및 입력 상태, 자유 시점 / 락온 시점을 제어하는 컨트롤러
/// </summary>
[RequireComponent(typeof(CinemachineInputAxisController))]
public class CameraController : MonoBehaviour
{
    private CinemachineInputAxisController axisController;

    [Header("Virtual Cameras")]
    [Tooltip("자유 시점 카메라")]
    [SerializeField] private CinemachineCamera freeLookCamera;
    [Tooltip("락온 카메라")]
    [SerializeField] private CinemachineCamera lockOnCamera;

    [Header("Target Group")]
    [Tooltip("플레이어와 타겟을 한 화면에 담기 위한 시네머신 타겟 그룹")]
    [SerializeField] private CinemachineTargetGroup targetGroup;
    [Tooltip("타겟 그룹에 등록할 플레이어 Transform")]
    [SerializeField] private Transform playerTargetTransform;

    [Header("LockOn Reference")]
    [SerializeField] private PlayerLockOnController lockOnController;

    private void Awake()
    {
        axisController = GetComponent<CinemachineInputAxisController>();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCursorLockChanged += HandleCursorLockChanged;
        }

        if (lockOnController != null)
        {
            lockOnController.OnLockOnTargetChanged += HandleLockOnTargetChanged;
        }
    }

    private void Start()
    {
        // 초기 커서 상태 동기화
        bool isCurrentLocked = (Cursor.lockState == CursorLockMode.Locked);
        ApplyAxisState(isCurrentLocked);

        // 자유 시점 활성화
        SwitchToFreeLook();
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCursorLockChanged -= HandleCursorLockChanged;
        }

        if (lockOnController != null)
        {
            lockOnController.OnLockOnTargetChanged -= HandleLockOnTargetChanged;
        }
    }

    /// <summary>
    /// 커서 잠금 상태 변경 시 호출되는 이벤트 핸들러
    /// </summary>
    /// <param name="isLocked">커서가 잠긴 상태인지 여부</param>
    private void HandleCursorLockChanged(bool isLocked)
    {
        ApplyAxisState(isLocked);
    }

    /// <summary>
    /// 락온 대상 변경 시 호출되는 이벤트 핸들러
    /// </summary>
    /// <param name="target">변경된 락온 대상</param>
    private void HandleLockOnTargetChanged(LockOnTarget target)
    {
        if (target != null)
        {
            SwitchToLockOn(target);
        }
        else
        {
            SwitchToFreeLook();
        }
    }

    /// <summary>
    /// 락온 대상이 존재할 때, 타겟 그룹에 플레이어와 적을 등록하고 락온 카메라로 전환
    /// </summary>
    /// <param name="target">락온 대상</param>
    private void SwitchToLockOn(LockOnTarget target)
    {
        // 기존 목록 정리 후 플레이어와 적 등록
        if (targetGroup != null)
        {
            while (targetGroup.Targets.Count > 0)
            {
                targetGroup.RemoveMember(targetGroup.Targets[0].Object);
            }

            // 플레이어
            if (playerTargetTransform != null)
            {
                targetGroup.AddMember(playerTargetTransform, 1.0f, 1.0f);
            }

            // 락온 대상 적
            targetGroup.AddMember(target.TargetTransform, 1.2f, 1.2f);
        }

        // 락온 카메라 전환
        if (lockOnCamera != null && freeLookCamera != null)
        {
            lockOnCamera.Priority = 20;
            freeLookCamera.Priority = 10;
        }

        // 락온 중에는 임의 마우스 회전 입력 차단
        ApplyAxisState(false);
    }

    /// <summary>
    /// 락온 대상이 없을 때, 타겟 그룹을 정리하고 자유 시점 카메라로 전환
    /// </summary>
    private void SwitchToFreeLook()
    {
        // 가상 카메라 전환
        if (lockOnCamera != null && freeLookCamera != null)
        {
            freeLookCamera.Priority = 20;
            lockOnCamera.Priority = 10;
        }

        // 타겟 그룹 정리
        if (targetGroup != null)
        {
            while (targetGroup.Targets.Count > 0)
            {
                targetGroup.RemoveMember(targetGroup.Targets[0].Object);
            }
        }

        // 커서가 잠긴 상태라면 마우스 회전 축 입력 복원
        bool isCurrentLocked = (Cursor.lockState == CursorLockMode.Locked);
        ApplyAxisState(isCurrentLocked);
    }

    /// <summary>
    /// 커서 잠금 상태에 따라 시네머신 입력 축 컨트롤러와 모든 축 컨트롤러의 활성화 상태를 적용
    /// </summary>
    /// <param name="isEnabled">축 컨트롤러가 활성화되어야 하는지 여부</param>
    private void ApplyAxisState(bool isEnabled)
    {
        if (axisController == null) return;

        axisController.enabled = isEnabled;

        for (int i = 0; i < axisController.Controllers.Count; i++)
        {
            axisController.Controllers[i].Enabled = isEnabled;
        }
    }
}