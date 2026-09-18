using UnityEngine;

/// <summary>
/// 피격 및 대미지 판정 테스트용
/// </summary>
public class AttackTest : MonoBehaviour, IDamageable
{
    [Header("Dummy Stats")]
    [SerializeField] private float maxHealth = 100.0f;
    [SerializeField] private float currentHealth;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(DamageInfo damageInfo)
    {
        currentHealth = Mathf.Max(0, currentHealth - damageInfo.Amount);

        Debug.Log($"<color=red>[Dummy Hit]</color> {name} 피격! " +
                  $"대미지: {damageInfo.Amount} | 남은 체력: {currentHealth}/{maxHealth} | " +
                  $"타격 위치: {damageInfo.HitPoint} | 공격자: {damageInfo.Attacker?.name}");

        if (currentHealth <= 0)
        {
            Debug.Log($"<color=yellow>[Dummy Destroyed]</color> {name} 체력이 소진되었습니다. (체력 리셋)");
            currentHealth = maxHealth; // 테스트를 위해 자동 회복
        }
    }
}