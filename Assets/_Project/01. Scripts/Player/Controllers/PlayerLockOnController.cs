using System;
using UnityEngine;

/// <summary>
/// 플레이어 주변의 락온 대상 탐색 및 락온 상태 컨트롤러
/// </summary>
public class PlayerLockOnController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputController inputController;
    [SerializeField] private LockOnMarkerUI markerUI;

    [Header("Detection Settings")]
    [Tooltip("락온 탐색 반경")]
    [SerializeField] private float detectionRadius = 15.0f;
    [Tooltip("락온 유지 최대 거리")]
    [SerializeField] private float maxLockOnDistance = 18.0f;
    [Tooltip("락온 탐색 각도")]
    [SerializeField] private float maxViewAngle = 70.0f;

    [Header("Layer Masks")]
    [Tooltip("적 레이어")]
    [SerializeField] private LayerMask targetLayer;
    [Tooltip("장애물 레이어")]
    [SerializeField] private LayerMask obstacleLayer;

    private LockOnTarget currentTarget;
    private Camera mainCamera;

    public LockOnTarget CurrentTarget => currentTarget;
    public bool IsLockedOn => currentTarget != null;

    // 타겟 변경 알림 이벤트
    public event Action<LockOnTarget> OnLockOnTargetChanged;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (inputController == null)
        {
            inputController = GetComponent<PlayerInputController>();
        }
    }

    private void OnEnable()
    {
        if (inputController != null)
        {
            inputController.OnLockOnTriggered += HandleLockOnTriggered;
        }
    }

    private void OnDisable()
    {
        if (inputController != null)
        {
            inputController.OnLockOnTriggered -= HandleLockOnTriggered;
        }

        ClearLockOn();
    }

    private void Update()
    {
        // 락온 유지 상태 검증
        if (IsLockedOn)
        {
            ValidateCurrentTarget();
        }
    }

    private void HandleLockOnTriggered()
    {
        // GameManager 메뉴가 열려 있다면 무시
        if (GameManager.Instance != null && GameManager.Instance.IsMenuOpened)
            return;

        if (IsLockedOn)
        {
            ClearLockOn();
        }
        else
        {
            TryFindTarget();
        }
    }

    /// <summary>
    /// 반경 및 카메라 시야각 내 최적의 락온 대상 탐색
    /// </summary>
    private void TryFindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, targetLayer);
        LockOnTarget bestTarget = null;
        float bestScore = float.MaxValue;

        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 cameraForward = mainCamera.transform.forward;

        foreach (var col in hits)
        {
            if (!col.TryGetComponent<LockOnTarget>(out var candidate) || !candidate.CanBeLockedOn)
                continue;

            Vector3 targetPos = candidate.TargetTransform.position;
            Vector3 dirToTarget = (targetPos - cameraPos).normalized;
            float angle = Vector3.Angle(cameraForward, dirToTarget);

            // 카메라 시야각 벗어남
            if (angle > maxViewAngle)
                continue;

            // 카메라와 타겟 사이 장애물 가림 확인
            if (Physics.Linecast(cameraPos, targetPos, obstacleLayer))
                continue;

            // 화면 중심(각도) 가중치 + 거리
            float distance = Vector3.Distance(transform.position, targetPos);
            float score = angle * 1.5f + distance;

            if (score < bestScore)
            {
                bestScore = score;
                bestTarget = candidate;
            }
        }

        if (bestTarget != null)
        {
            SetTarget(bestTarget);
        }
    }

    /// <summary>
    /// 현재 락온 대상의 유효성 검사
    /// </summary>
    private void ValidateCurrentTarget()
    {
        // 대상이 비활성화되거나 락온 불가능 상태(사망 등)가 된 경우
        if (!currentTarget.CanBeLockedOn)
        {
            ClearLockOn();
            return;
        }

        // 최대 허용 거리 초과 시 해제
        float distance = Vector3.Distance(transform.position, currentTarget.TargetTransform.position);
        if (distance > maxLockOnDistance)
        {
            ClearLockOn();
            return;
        }

        // 장애물에 가려진 경우 해제
        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 targetPos = currentTarget.TargetTransform.position;
        if (Physics.Linecast(cameraPos, targetPos, obstacleLayer))
        {
            ClearLockOn();
        }
    }

    private void SetTarget(LockOnTarget newTarget)
    {
        currentTarget = newTarget;

        // UI 마커 활성화
        if (markerUI != null)
        {
            markerUI.Show(currentTarget.TargetTransform);
        }

        OnLockOnTargetChanged?.Invoke(currentTarget);
    }

    public void ClearLockOn()
    {
        currentTarget = null;

        // UI 마커 비활성화
        if (markerUI != null)
        {
            markerUI.Hide();
        }

        OnLockOnTargetChanged?.Invoke(null);
    }

    private void OnDrawGizmosSelected()
    {
        // 탐색 반경 디버그
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxLockOnDistance);
    }
}