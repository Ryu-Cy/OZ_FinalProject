using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// 기본 자유 시점 상태: 마우스 입력을 수신하여 가상 카메라의 회전(Orbit)을 제어
/// </summary>
[RequireComponent(typeof(CinemachineInputAxisController))]
public class CameraFreeLookState : MonoBehaviour
{
    private CinemachineInputAxisController axisController;

    private void Awake()
    {
        axisController = GetComponent<CinemachineInputAxisController>();
    }

    private void OnDisable()
    {
        // 상태 컴포넌트가 비활성화될 때 축 입력도 안전하게 차단
        SetInputActive(false);
    }

    /// <summary>
    /// 마우스 축 입력 활성화/비활성화 제어
    /// </summary>
    public void SetInputActive(bool isActive)
    {
        if (axisController == null) return;

        axisController.enabled = isActive;

        for (int i = 0; i < axisController.Controllers.Count; i++)
        {
            axisController.Controllers[i].Enabled = isActive;
        }
    }
}