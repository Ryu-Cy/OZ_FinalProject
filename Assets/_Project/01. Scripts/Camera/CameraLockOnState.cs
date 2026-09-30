using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// 락온 시점 상태: 가상 카메라의 회전(Orbit)을 락온 대상을 정면으로 응시하도록 실시간 보간 제어
/// </summary>
[RequireComponent(typeof(CinemachineCamera))]
[RequireComponent(typeof(CinemachineOrbitalFollow))]
public class CameraLockOnState : MonoBehaviour
{
    private CinemachineCamera virtualCamera;
    private CinemachineOrbitalFollow orbitalFollow;

    [Header("Tracking Settings")]
    [Tooltip("락온 시 카메라 회전 보간 속도")]
    [SerializeField] private float trackingDamping = 12f;

    [Tooltip("상하 각도 기본 보정치 (카메라가 적의 가슴을 살짝 내려다보도록 조정)")]
    [SerializeField] private float verticalOffsetAngle = 4f;

    private Transform currentTarget;

    private void Awake()
    {
        virtualCamera = GetComponent<CinemachineCamera>();
        orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
    }

    private void OnDisable()
    {
        currentTarget = null;
    }

    private void LateUpdate()
    {
        if (currentTarget == null || orbitalFollow == null) return;

        UpdateLockOnOrientation();
    }

    /// <summary>
    /// 추적할 락온 타겟 Transform 지정
    /// </summary>
    public void SetTarget(Transform targetTransform)
    {
        currentTarget = targetTransform;
    }

    /// <summary>
    /// 락온 타겟 해제
    /// </summary>
    public void ClearTarget()
    {
        currentTarget = null;
    }

    /// <summary>
    /// 플레이어 피벗과 타겟 사이의 각도를 계산하여 OrbitalFollow 축 동기화
    /// </summary>
    private void UpdateLockOnOrientation()
    {
        Transform followTarget = virtualCamera.Target.TrackingTarget;
        if (followTarget == null) return;

        // 플레이어(CameraPivot) -> 적 방향 벡터 계산
        Vector3 dirToTarget = currentTarget.position - followTarget.position;
        if (dirToTarget.sqrMagnitude < 0.001f) return;

        // 1. 수평 목표 각도 (Yaw)
        float targetHorizontalAngle = Mathf.Atan2(dirToTarget.x, dirToTarget.z) * Mathf.Rad2Deg;

        // 2. 수직 목표 각도 (Pitch)
        float horizontalDistance = new Vector2(dirToTarget.x, dirToTarget.z).magnitude;
        float targetVerticalAngle = -Mathf.Atan2(dirToTarget.y, horizontalDistance) * Mathf.Rad2Deg + verticalOffsetAngle;

        // 3. 부드러운 각도 보간 적용
        orbitalFollow.HorizontalAxis.Value = Mathf.LerpAngle(
            orbitalFollow.HorizontalAxis.Value,
            targetHorizontalAngle,
            Time.deltaTime * trackingDamping
        );

        orbitalFollow.VerticalAxis.Value = Mathf.Lerp(
            orbitalFollow.VerticalAxis.Value,
            targetVerticalAngle,
            Time.deltaTime * trackingDamping
        );
    }
}