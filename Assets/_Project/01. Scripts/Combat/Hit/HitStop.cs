using System.Collections;
using UnityEngine;

/// <summary>
/// 히트스탑을 적용하는 클래스
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
    public void ApplyHitStop(float duration)
    {
        if (duration <= 0f) return;

        // 이미 히트스탑 실행 중이면 이전 코루틴을 중지하고 복구 후 재시작
        if (stopCoroutine != null)
        {
            StopCoroutine(stopCoroutine);
            RestoreState();
        }

        stopCoroutine = StartCoroutine(RoutineHitStop(duration));
    }

    private IEnumerator RoutineHitStop(float duration)
    {
        isStopped = true;

        if (animator != null)
        {
            animator.speed = 0.0f;
        }

        if (hitReaction != null)
        {
            hitReaction.enabled = false;
        }

        float timer = 0.0f;
        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        RestoreState();
    }

    /// <summary>
    /// 히트스탑 상태 복구
    /// </summary>
    private void RestoreState()
    {
        if (animator != null)
        {
            animator.speed = 1.0f;
        }

        if (hitReaction != null)
        {
            hitReaction.enabled = true;
        }

        isStopped = false;
        stopCoroutine = null;
    }

    private void OnDisable()
    {
        if (stopCoroutine != null)
        {
            StopCoroutine(stopCoroutine);
        }
        RestoreState();
    }
}