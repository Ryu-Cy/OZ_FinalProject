using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 적의 체력 연산, 피격 및 사망 이벤트를 담당하는 클래스
/// IDamageable 인터페이스를 구현하여 무기 타격을 정상 수신합니다.
/// </summary>
public class EnemyHealth : MonoBehaviour, IDamageable
{
    // 1차 빌드 시각적 피드백을 위해 색상 전환용
    [Header("Hit Flash Settings")]
    [Tooltip("피격 시 변경할 머티리얼 색상")]
    [SerializeField] private Color hitColor = Color.red;

    [Tooltip("피격 색상 유지 시간 (초)")]
    [SerializeField] private float hitFlashDuration = 0.12f;

    private EnemyController enemyController;
    private MeshRenderer meshRenderer;
    private Color originalColor;
    private Coroutine hitFlashCoroutine;

    private float currentHealth;
    private bool isDead = false;

    /// <summary>
    /// 체력이 0에 도달했을 때 컨트롤러에 알리는 사망 이벤트
    /// </summary>
    public event Action OnDeath;

    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        enemyController = GetComponent<EnemyController>();
        meshRenderer = GetComponentInChildren<MeshRenderer>();

        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
    }

    private void Start()
    {
        ResetHealth();
    }

    /// <summary>
    /// EnemyData 원본을 참조하여 체력과 머티리얼 색상을 초기 상태로 복구
    /// </summary>
    public void ResetHealth()
    {
        if (enemyController == null || enemyController.EnemyData == null)
            return;

        currentHealth = enemyController.EnemyData.MaxHealth;
        isDead = false;

        if (meshRenderer != null)
        {
            meshRenderer.material.color = originalColor;
        }
    }

    /// <summary>
    /// IDamageable 인터페이스 구현 (무기 타격 연동)
    /// </summary>
    public void TakeDamage(DamageInfo damageInfo)
    {
        TakeDamage(damageInfo.Amount);
    }

    /// <summary>
    /// 외부 타격 판정에서 데미지를 전달받아 처리
    /// </summary>
    public void TakeDamage(float damageAmount)
    {
        if (isDead)
            return;

        currentHealth = Mathf.Max(0.0f, currentHealth - damageAmount);

        // 연타 피격 시 이전 점멸 루틴을 안전하게 취소하고 재가동
        if (hitFlashCoroutine != null)
        {
            StopCoroutine(hitFlashCoroutine);
        }
        hitFlashCoroutine = StartCoroutine(RoutineHitFlash());

        // 체력 고갈 시 사망 판정
        if (currentHealth <= 0.0f)
        {
            isDead = true;
            OnDeath?.Invoke();
        }
    }

    private IEnumerator RoutineHitFlash()
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = hitColor;
        }

        yield return new WaitForSeconds(hitFlashDuration);

        if (!isDead && meshRenderer != null)
        {
            meshRenderer.material.color = originalColor;
        }

        hitFlashCoroutine = null;
    }

    private void OnDisable()
    {
        if (hitFlashCoroutine != null)
        {
            StopCoroutine(hitFlashCoroutine);
            hitFlashCoroutine = null;
        }
    }
}