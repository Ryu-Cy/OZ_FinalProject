using System.Collections;
using UnityEngine;

/// <summary>
/// 히트스탑을 적용하는 클래스 <br/>
/// 아직 적 애니메이션 적용 전이라 애니메이션 적용 후에 수정될 가능성 높음.
/// </summary>
public class HitStop : MonoBehaviour
{
    private Animator animator;
    private HitReaction hitReaction;
    private Coroutine stopCoroutine;
    private bool isStopped = false;

    // 프로퍼티
    public bool IsInvincible => isStopped;
    public bool IsStopped => isStopped;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        hitReaction = GetComponent<HitReaction>();
    }

    /// <summary>
    /// 지정한 시간 동안 포즈 및 애니메이션 정지
    /// </summary>
    /// <param name="duration">정지 시간</param>
    public void ApplyHitStop(float duration)
    {
        if (duration <= 0f) return;

        // 이미 히트스탑이 적용 중이면 기존 코루틴을 중지하고 새로 시작
        if (stopCoroutine != null)
        {
            StopCoroutine(stopCoroutine);
        }

        stopCoroutine = StartCoroutine(RoutineHitStop(duration));
    }

    /// <summary>
    /// 히트스탑 코루틴
    /// </summary>
    /// <param name="duration">정지 시간</param>
    /// <returns></returns>
    private IEnumerator RoutineHitStop(float duration)
    {
        isStopped = true;

        float originalAnimSpeed = 1.0f;
        // 애니메이터 속도를 0으로 설정하여 애니메이션 정지
        if (animator != null)
        {
            originalAnimSpeed = animator.speed;
            animator.speed = 0.0f;
        }
        // HitReaction 비활성화
        if (hitReaction != null)
        {
            hitReaction.enabled = false;
        }

        // 지정한 시간 동안 대기
        float timer = 0.0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        // 히트스탑 종료 후 원래 상태로 복원
        if (animator != null)
        {
            animator.speed = originalAnimSpeed;
        }
        // HitReaction 활성화
        if (hitReaction != null)
        {
            hitReaction.enabled = true;
        }

        isStopped = false;
        stopCoroutine = null;
    }

    private void OnDisable()
    {
        // 히트스탑이 적용 중일 때 오브젝트가 비활성화되면 코루틴을 중지하고 상태를 복원
        if (stopCoroutine != null)
        {
            StopCoroutine(stopCoroutine);
            stopCoroutine = null;
        }

        // 히트스탑 상태를 복원
        if (animator != null)
            animator.speed = 1.0f;
        // HitReaction 활성화
        if (hitReaction != null)
            hitReaction.enabled = true;

        isStopped = false;
    }
}
