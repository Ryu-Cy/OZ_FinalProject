using UnityEngine;

/// <summary>
/// 락온된 대상의 머리/가슴 위치를 실시간으로 따라다니는 단일 UI 마커
/// </summary>
public class LockOnMarkerUI : MonoBehaviour
{
    [SerializeField] private GameObject markerVisual; // 켜고 끌 마커 오브젝트
    [SerializeField] private Vector3 worldOffset = Vector3.zero; // 높이/위치 보정용

    private Transform currentTargetTransform;
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        // 시작 시 마커 숨김 처리
        if (markerVisual == null) markerVisual = gameObject;
        Hide();
    }

    private void LateUpdate()
    {
        // 락온 대상이 없으면 리턴
        if (currentTargetTransform == null) return;

        // 월드 좌표를 화면 스크린 좌표로 변환하여 추적 (Screen Space Overlay 캔버스 사용 시)
        Vector3 targetWorldPos = currentTargetTransform.position + worldOffset;

        // 카메라 뒤편으로 넘어갔을 경우 숨김
        Vector3 screenPos = mainCamera.WorldToScreenPoint(targetWorldPos);
        if (screenPos.z < 0)
        {
            if (markerVisual.activeSelf) markerVisual.SetActive(false);
            return;
        }

        if (!markerVisual.activeSelf) markerVisual.SetActive(true);

        transform.position = screenPos;
    }

    /// <summary>
    /// 락온 대상 설정 및 마커 활성화
    /// </summary>
    public void Show(Transform targetTransform)
    {
        currentTargetTransform = targetTransform;
        if (markerVisual != null) markerVisual.SetActive(true);
    }

    /// <summary>
    /// 락온 해제 시 마커 비활성화
    /// </summary>
    public void Hide()
    {
        currentTargetTransform = null;
        if (markerVisual != null) markerVisual.SetActive(false);
    }
}