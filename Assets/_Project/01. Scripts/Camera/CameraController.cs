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

    private void Update()
    {
        if (axisController == null) return;

        // 마우스가 화면 중앙에 완벽히 잠겨 있을 때만 회전 입력 허용
        bool isCursorLocked = (Cursor.lockState == CursorLockMode.Locked);

        // 시네머신 축 컴포넌트 활성화/비활성화
        if (axisController.enabled != isCursorLocked)
        {
            axisController.enabled = isCursorLocked;
        }

        // 축 내부 개별 컨트롤러 동기화
        for (int i = 0; i < axisController.Controllers.Count; i++)
        {
            if (axisController.Controllers[i].Enabled != isCursorLocked)
            {
                axisController.Controllers[i].Enabled = isCursorLocked;
            }
        }
    }
}