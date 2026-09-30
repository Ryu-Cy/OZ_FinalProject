using UnityEngine;

/// <summary>
/// 락온 대상이 되는 오브젝트에 부착하여 타겟 좌표와 타겟팅 가능 상태를 제공하는 컴포넌트
/// </summary>
public class LockOnTarget : MonoBehaviour
{
    [Tooltip("수동 지정용 락온 피벗")]
    [SerializeField] private Transform lockOnPoint;

    [Tooltip("본을 찾지 못했을 때 적용할 기본 높이 오프셋")]
    [SerializeField] private float fallbackHeightOffset = 1.3f;

    [SerializeField] private bool canBeLockedOn = true;

    // 프로퍼티
    /// <summary>
    /// 실제 조준할 월드 좌표를 가진 Transform
    /// </summary>
    public Transform TargetTransform => lockOnPoint != null ? lockOnPoint : transform;

    /// <summary>
    /// 현재 락온 가능한 유효 상태인지 확인
    /// </summary>
    public bool CanBeLockedOn => canBeLockedOn && gameObject.activeInHierarchy;

    private void Awake()
    {
        // 수동 할당된 포인트가 있다면 우선 사용
        if (lockOnPoint != null) return;

        // HitReaction의 체스트 본 매칭 시도
        HitReaction hitReaction = GetComponentInChildren<HitReaction>();
        if (hitReaction != null && hitReaction.ChestTransform != null)
        {
            lockOnPoint = hitReaction.ChestTransform;
            return;
        }

        // Animator 휴머노이드 본 매칭 시도
        var animator = GetComponentInChildren<Animator>();
        if (animator != null && animator.isHuman)
        {
            Transform chest = animator.GetBoneTransform(HumanBodyBones.Chest);
            if (chest == null) chest = animator.GetBoneTransform(HumanBodyBones.Spine);

            if (chest != null)
            {
                lockOnPoint = chest;
            }
        }
    }

    /// <summary>
    /// 락온 가능 상태 설정
    /// </summary>
    /// <param name="isEnabled">락온 가능 상태</param>
    public void SetLockOnEnabled(bool isEnabled)
    {
        canBeLockedOn = isEnabled;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 targetPos = lockOnPoint != null ? lockOnPoint.position : transform.position + Vector3.up * fallbackHeightOffset;
        Gizmos.DrawWireSphere(targetPos, 0.15f);
    }
}