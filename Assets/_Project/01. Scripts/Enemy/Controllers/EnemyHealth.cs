using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적의 체력 연산, 툰 셰이더 점멸, 히트스탑 및 본 피격 반응 연동 클래스
/// 너무 많은 일을 하나 ...?
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

    // UTS 및 기본 프로퍼티 캐싱
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int FirstShadeColorId = Shader.PropertyToID("_1st_ShadeColor");
    private static readonly int SecondShadeColorId = Shader.PropertyToID("_2nd_ShadeColor");
    private static readonly int LegacyColorId = Shader.PropertyToID("_Color");

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
    /// <param name="damageInfo">피격 정보</param>
    public void TakeDamage(DamageInfo damageInfo)
    {
        if (isDead) return;

        // 대미지 선연산
        currentHealth = Mathf.Max(0.0f, currentHealth - damageInfo.Amount);
        bool willDie = currentHealth <= 0.0f;

        // 공격자 HitStop 연동
        // 공격자가 HitStop 컴포넌트를 가지고 있다면, 공격자에게도 히트스탑을 적용
        if (damageInfo.Attacker != null && damageInfo.Attacker.TryGetComponent<HitStop>(out var attackerHitStop))
        {
            float attackerDuration = willDie ? deathHitStopDuration : 0.05f;
            attackerHitStop.ApplyHitStop(attackerDuration);
        }

        // 피격 지점과 가장 가까운 렌더러를 찾아서 점멸 효과를 적용
        Renderer targetRenderer = GetClosestRenderer(damageInfo.HitPoint);
        if (targetRenderer != null)
        {
            TriggerPartFlash(targetRenderer);
        }

        // 본 회전 피격 반응
        if (hitReaction != null)
        {
            hitReaction.ApplyHitReaction(damageInfo.HitPoint, damageInfo.HitDirection, damageInfo.Amount);
        }

        // 사망 시 사망 처리 코루틴 실행
        // 아니면 일반 피격 히트스탑 적용
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

    /// <summary>
    /// 적이 대미지를 받는 함수
    /// </summary>
    /// <param name="damageAmount">받은 대미지 양</param>
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
    /// 사망 시 히트스탑으로 꺾인 포즈를 잠시 얼려둔 후 사망 이벤트를 호출
    /// </summary>
    private IEnumerator RoutineHandleDeath()
    {
        if (hitStop != null)
        {
            hitStop.ApplyHitStop(deathHitStopDuration);
        }

        // 히트스탑 시간 동안 대기
        float timer = 0f;
        while (timer < deathHitStopDuration)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        // 히트스탑 연출 종료 후 사망 처리 진행
        OnDeath?.Invoke();
    }

    /// <summary>
    /// 피격 지점과 가장 가까운 렌더러를 찾는 함수
    /// </summary>
    /// <param name="hitPoint">피격 지점</param>
    /// <returns>가장 가까운 렌더러</returns>
    private Renderer GetClosestRenderer(Vector3 hitPoint)
    {
        if (allRenderers == null || allRenderers.Length == 0) return null;

        Renderer closest = null;
        float minSqrDist = float.MaxValue;

        // 모든 렌더러를 순회하며 피격 지점과의 거리를 계산
        for (int i = 0; i < allRenderers.Length; i++)
        {
            Renderer r = allRenderers[i];
            if (r == null || !r.enabled) continue;

            // 렌더러의 바운드에 가장 가까운 점을 계산하여 피격 지점과의 제곱 거리 계산
            Vector3 closestPointOnBounds = r.bounds.ClosestPoint(hitPoint);
            float sqrDist = (closestPointOnBounds - hitPoint).sqrMagnitude;
            // 가장 가까운 렌더러를 찾기 위해 최소 제곱 거리와 비교
            if (sqrDist < minSqrDist)
            {
                minSqrDist = sqrDist;
                closest = r;
            }
        }

        return closest;
    }

    /// <summary>
    /// 렌더러에 점멸 효과를 적용하는 함수
    /// </summary>
    /// <param name="targetRenderer">점멸 효과를 적용할 렌더러</param>
    private void TriggerPartFlash(Renderer targetRenderer)
    {
        // 이미 점멸 코루틴이 실행 중이면 중지
        if (flashCoroutines.TryGetValue(targetRenderer, out Coroutine runningRoutine) && runningRoutine != null)
        {
            StopCoroutine(runningRoutine);
        }
        // 새로운 점멸 코루틴 시작
        flashCoroutines[targetRenderer] = StartCoroutine(RoutinePartFlash(targetRenderer));
    }

    /// <summary>
    /// 렌더러에 점멸 효과를 적용하는 코루틴
    /// </summary>
    /// <param name="targetRenderer">점멸 효과를 적용할 렌더러</param>
    /// <returns></returns>
    private IEnumerator RoutinePartFlash(Renderer targetRenderer)
    {
        ApplyFlashColor(targetRenderer, hitColor);

        yield return new WaitForSeconds(hitFlashDuration);

        if (targetRenderer != null)
        {
            targetRenderer.SetPropertyBlock(null);
        }

        flashCoroutines.Remove(targetRenderer);
    }

    /// <summary>
    /// 렌더러에 점멸 색상을 적용하는 함수
    /// </summary>
    /// <param name="r">점멸 색상을 적용할 렌더러</param>
    /// <param name="color">적용할 점멸 색상</param>
    private void ApplyFlashColor(Renderer r, Color color)
    {
        r.GetPropertyBlock(propBlock);

        propBlock.SetColor(BaseColorId, color);
        propBlock.SetColor(FirstShadeColorId, color);
        propBlock.SetColor(SecondShadeColorId, color);
        propBlock.SetColor(LegacyColorId, color);

        r.SetPropertyBlock(propBlock);
    }

    /// <summary>
    /// 모든 점멸 코루틴을 중지하고 렌더러의 색상을 초기화하는 함수
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