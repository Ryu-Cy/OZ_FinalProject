using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// 인게임 메인 카메라의 동작 및 입력 상태를 제어하는 컨트롤러
/// </summary>
[RequireComponent(typeof(CinemachineInputAxisController))]
public class CameraController : MonoBehaviour
{
    private CinemachineInputAxisController axisController;

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
    }

    private void Start()
    {
        // 초기 커서 상태 동기화
        bool isCurrentLocked = (Cursor.lockState == CursorLockMode.Locked);
        ApplyAxisState(isCurrentLocked);
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnCursorLockChanged -= HandleCursorLockChanged;
        }
    }

    /// <summary>
    /// 커서 락 상태 변경 이벤트 수신부
    /// </summary>
    private void HandleCursorLockChanged(bool isLocked)
    {
        ApplyAxisState(isLocked);
    }

    /// <summary>
    /// 시네머신 축 입력 On/Off 처리
    /// </summary>
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