using System;
using UnityEngine;

/// <summary>
/// 플레이어의 런타임 스탯 관리 및 UI 갱신용 이벤트를 발생시키는 클래스
/// </summary>
public class PlayerStats : MonoBehaviour, IDamageable
{
    [Header("Base Data")]
    [Tooltip("플레이어 기본 SO 데이터")]
    [SerializeField] private PlayerData baseData;

    [Header("Runtime Stats")]
    [Tooltip("현재 체력")]
    [SerializeField] private float currentHealth;
    [Tooltip("현재 스태미나")]
    [SerializeField] private float currentStamina;
    [Tooltip("무기 장착 후 최종 공격력")]
    [SerializeField] private float totalAttackPower;
    [Tooltip("현재 장착 무기의 공격 1회당 스태미나 소모량")]
    [SerializeField] private float currentWeaponStaminaCost;

    private float bonusAttackPower = 0.0f;

    // 프로퍼티
    public float CurrentHealth => currentHealth;
    public float CurrentStamina => currentStamina;

    // UI 연동용 이벤트 (현재값, 최대값)
    public event Action<float, float> OnHealthChanged;
    public event Action<float, float> OnStaminaChanged;

    // 사망 이벤트
    public event Action OnDeath;

    // 최종 스탯 프로퍼티
    public float TotalAttackPower => totalAttackPower;
    public float AttackStaminaCost => currentWeaponStaminaCost;

    private void Start()
    {
        if (baseData != null)
        {
            currentHealth = baseData.MaxHealth;
            currentStamina = baseData.MaxStamina;
            UpdateTotalAttack();

            // 초기 UI 갱신을 위해 이벤트 호출
            NotifyHealthUI();
            NotifyStaminaUI();
        }
    }

    /// <summary>
    /// PlayerEquipmentController가 장비 셋업 시 호출하여 스탯을 덮어씌웁니다.
    /// </summary>
    public void UpdateWeaponStats(float weaponAttack, float weaponStaminaCost)
    {
        bonusAttackPower = weaponAttack;
        currentWeaponStaminaCost = weaponStaminaCost;
        UpdateTotalAttack();
    }

    private void UpdateTotalAttack()
    {
        totalAttackPower = (baseData != null ? baseData.BaseAttackPower : 0.0f) + bonusAttackPower;
    }

    /// <summary>
    /// IDamageable 인터페이스 구현
    /// </summary>
    public void TakeDamage(DamageInfo damageInfo)
    {
        if (currentHealth <= 0.0f) return;

        currentHealth = Mathf.Max(0.0f, currentHealth - damageInfo.Amount);
        NotifyHealthUI();

        if (currentHealth <= 0.0f)
        {
            OnDeath?.Invoke();
        }
    }

    /// <summary>
    /// 체력 회복 물약 사용 시 호출
    /// </summary>
    public void Heal(float amount)
    {
        if (currentHealth <= 0.0f || baseData == null) return;

        currentHealth = Mathf.Min(baseData.MaxHealth, currentHealth + amount);
        NotifyHealthUI();
    }

    /// <summary>
    /// 공격이나 회피 등 스태미나가 필요한 액션 시 호출
    /// </summary>
    /// <param name="cost">소모할 스태미나 양</param>
    /// <returns>스태미나가 충분해 액션이 가능한지 여부</returns>
    public bool TryConsumeStamina(float cost)
    {
        if (currentStamina >= cost)
        {
            currentStamina -= cost;
            NotifyStaminaUI();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 스태미나 회복
    /// </summary>
    public void RecoverStamina(float amount)
    {
        if (baseData == null || currentStamina >= baseData.MaxStamina) return;

        currentStamina = Mathf.Min(baseData.MaxStamina, currentStamina + amount);
        NotifyStaminaUI();
    }

    private void NotifyHealthUI()
    {
        if (baseData != null) OnHealthChanged?.Invoke(currentHealth, baseData.MaxHealth);
    }

    private void NotifyStaminaUI()
    {
        if (baseData != null) OnStaminaChanged?.Invoke(currentStamina, baseData.MaxStamina);
    }
}