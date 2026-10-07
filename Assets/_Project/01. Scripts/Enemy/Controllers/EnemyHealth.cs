using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적의 체력 연산, 피격 점멸(URP 호환), 히트스탑 및 본 피격 반응 연동 클래스
/// </summary>
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Hit Flash Settings")]
    [Tooltip("피격 시 변경할 머티리얼 색상")]
    [SerializeField] private Color hitColor = Color.red;

    [Tooltip("피격 색상 유지 시간 (초)")]
    [SerializeField] private float hitFlashDuration = 0.12f;

    [Header("Hit Stop Settings")]
    [Tooltip("일반 피격 시 적이 멈추는 시간 (초)")]
    [SerializeField] private float defaultHitStopDuration = 0.08f;

    [Tooltip("사망 결정타(막타) 피격 시 적이 멈추는 시간 (초)")]
    [SerializeField] private float deathHitStopDuration = 0.18f;

    private EnemyController enemyController;
    private HitReaction hitReaction;
    private HitStop hitStop;

    private Renderer[] allRenderers;
    private MaterialPropertyBlock propBlock;
    private readonly Dictionary<Renderer, Coroutine> flashCoroutines = new Dictionary<Renderer, Coroutine>();

    private float currentHealth;
    private bool isDead = false;

    // URP 및 범용 셰이더 프로퍼티 ID
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int LegacyColorId = Shader.PropertyToID("_Color");
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly int MainColorId = Shader.PropertyToID("_MainColor");
    private static readonly int TintColorId = Shader.PropertyToID("_TintColor");

    // 툰 셰이더(UTS) 호환 유지
    private static readonly int FirstShadeColorId = Shader.PropertyToID("_1st_ShadeColor");
    private static readonly int SecondShadeColorId = Shader.PropertyToID("_2nd_ShadeColor");

    // 프로퍼티
    public event Action OnDeath;
    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        hitReaction = GetComponent<HitReaction>();
        hitStop = GetComponent<HitStop>();
        propBlock = new MaterialPropertyBlock();

        allRenderers = GetComponentsInChildren<Renderer>();

        ResetHealth();
    }

    private void Start()
    {
        ResetHealth();
    }

    /// <summary>
    /// 적 체력 초기화 및 사망 상태 리셋
    /// </summary>
    public void ResetHealth()
    {
        if (enemyController == null || enemyController.EnemyData == null)
            return;

        currentHealth = enemyController.EnemyData.MaxHealth;
        isDead = false;

        ClearAllFlashes();
    }

    /// <summary>
    /// 적이 대미지를 받는 함수
    /// </summary>
    public void TakeDamage(DamageInfo damageInfo)
    {
        if (isDead) return;

        // 대미지 연산
        currentHealth = Mathf.Max(0.0f, currentHealth - damageInfo.Amount);
        bool willDie = currentHealth <= 0.0f;

        // 공격자 HitStop 연동
        if (damageInfo.Attacker != null && damageInfo.Attacker.TryGetComponent<HitStop>(out var attackerHitStop))
        {
            float attackerDuration = willDie ? deathHitStopDuration : 0.05f;
            attackerHitStop.ApplyHitStop(attackerDuration);
        }

        // 피격 지점과 가장 가까운 렌더러 점멸
        Renderer targetRenderer = GetClosestRenderer(damageInfo.HitPoint);
        if (targetRenderer != null)
        {
            TriggerPartFlash(targetRenderer);
        }
        else if (allRenderers != null && allRenderers.Length > 0)
        {
            // 가까운 렌더러를 찾지 못했을 때 전체 렌더러 점멸
            for (int i = 0; i < allRenderers.Length; i++)
            {
                if (allRenderers[i] != null && allRenderers[i].enabled)
                    TriggerPartFlash(allRenderers[i]);
            }
        }

        // 본 회전 피격 반응
        if (hitReaction != null)
        {
            hitReaction.ApplyHitReaction(damageInfo.HitPoint, damageInfo.HitDirection, damageInfo.Amount);
        }

        // 사망 처리 또는 히트스탑
        if (willDie)
        {
            isDead = true;
            StartCoroutine(RoutineHandleDeath());
        }
        else
        {
            if (hitStop != null)
                hitStop.ApplyHitStop(defaultHitStopDuration);
        }
    }

    public void TakeDamage(float damageAmount)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0.0f, currentHealth - damageAmount);

        if (currentHealth <= 0.0f)
        {
            isDead = true;
            StartCoroutine(RoutineHandleDeath());
        }
    }

    /// <summary>
    /// 적 사망 시 히트스탑 적용 후 OnDeath 이벤트 호출
    /// </summary>
    private IEnumerator RoutineHandleDeath()
    {
        if (hitStop != null)
        {
            hitStop.ApplyHitStop(deathHitStopDuration);
        }

        float timer = 0f;
        while (timer < deathHitStopDuration)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        OnDeath?.Invoke();
    }

    /// <summary>
    /// 피격 지점과 가장 가까운 렌더러를 반환
    /// </summary>
    /// <param name="hitPoint">피격 지점</param>
    /// <returns>피격 지점과 가장 가까운 렌더러</returns>
    private Renderer GetClosestRenderer(Vector3 hitPoint)
    {
        if (allRenderers == null || allRenderers.Length == 0) return null;

        Renderer closest = null;
        float minSqrDist = float.MaxValue;

        for (int i = 0; i < allRenderers.Length; i++)
        {
            Renderer r = allRenderers[i];
            if (r == null || !r.enabled) continue;

            Vector3 closestPointOnBounds = r.bounds.ClosestPoint(hitPoint);
            float sqrDist = (closestPointOnBounds - hitPoint).sqrMagnitude;
            if (sqrDist < minSqrDist)
            {
                minSqrDist = sqrDist;
                closest = r;
            }
        }

        return closest;
    }

    /// <summary>
    /// 특정 렌더러에 대해 점멸 코루틴을 시작하거나 이미 실행 중인 코루틴을 중지 후 재시작
    /// </summary>
    /// <param name="targetRenderer">점멸할 렌더러</param>
    private void TriggerPartFlash(Renderer targetRenderer)
    {
        if (flashCoroutines.TryGetValue(targetRenderer, out Coroutine runningRoutine) && runningRoutine != null)
        {
            StopCoroutine(runningRoutine);
        }
        flashCoroutines[targetRenderer] = StartCoroutine(RoutinePartFlash(targetRenderer));
    }

    /// <summary>
    /// 렌더러에 대해 점멸 색상을 적용하고 일정 시간 후 원래 색상으로 복원
    /// </summary>
    /// <param name="targetRenderer">점멸할 렌더러</param>
    private IEnumerator RoutinePartFlash(Renderer targetRenderer)
    {
        ApplyFlashColor(targetRenderer, hitColor);

        // 히트스탑 중에도 정확한 시간 대기 보장
        yield return new WaitForSecondsRealtime(hitFlashDuration);

        if (targetRenderer != null)
        {
            targetRenderer.SetPropertyBlock(null);
        }

        flashCoroutines.Remove(targetRenderer);
    }

    /// <summary>
    /// 렌더러에 점멸 색상을 적용하는 함수. URP 및 범용 셰이더, 툰 셰이더 호환
    /// </summary>
    /// <param name="r">점멸할 렌더러</param>
    /// <param name="color">점멸 색상</param>
    private void ApplyFlashColor(Renderer r, Color color)
    {
        r.GetPropertyBlock(propBlock);

        // URP 표준 프로퍼티
        propBlock.SetColor(BaseColorId, color);
        propBlock.SetColor(LegacyColorId, color);
        propBlock.SetColor(EmissionColorId, color);
        propBlock.SetColor(MainColorId, color);
        propBlock.SetColor(TintColorId, color);

        // 툰 셰이더 프로퍼티
        propBlock.SetColor(FirstShadeColorId, color);
        propBlock.SetColor(SecondShadeColorId, color);

        r.SetPropertyBlock(propBlock);
    }

    /// <summary>
    /// 모든 점멸 코루틴을 중지하고 렌더러의 프로퍼티 블록을 초기화
    /// </summary>
    private void ClearAllFlashes()
    {
        foreach (var routine in flashCoroutines.Values)
        {
            if (routine != null)
                StopCoroutine(routine);
        }
        flashCoroutines.Clear();

        if (allRenderers != null)
        {
            for (int i = 0; i < allRenderers.Length; i++)
            {
                if (allRenderers[i] != null)
                    allRenderers[i].SetPropertyBlock(null);
            }
        }
    }

    private void OnDisable()
    {
        ClearAllFlashes();
    }
}