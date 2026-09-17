using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 근접 무기 타격 판정 및 콜라이더 제어 클래스
/// </summary>
public class MeleeWeapon : MonoBehaviour
{
    [Header("Hit Collider")]
    [SerializeField] private Collider hitCollider;

    [Header("Weapon Stats")]
    [SerializeField] private float baseDamage = 25f; // 기본 무기 대미지

    private readonly HashSet<Collider> hitTargets = new HashSet<Collider>();

    private void Awake()
    {
        if (hitCollider == null)
        {
            hitCollider = GetComponent<Collider>();
        }

        DisableAttack();
    }

    public void EnableAttack()
    {
        hitTargets.Clear();

        if (hitCollider != null)
        {
            hitCollider.enabled = true;
        }
    }

    public void DisableAttack()
    {
        if (hitCollider != null)
        {
            hitCollider.enabled = false;
        }

        hitTargets.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1회 공격 중 중복 타격 방지
        if (hitTargets.Contains(other)) return;

        // 피격 대상 인터페이스 탐색
        if (other.TryGetComponent<IDamageable>(out var damageable) ||
            other.GetComponentInParent<IDamageable>() is { } parentDamageable && (damageable = parentDamageable) != null)
        {
            hitTargets.Add(other);

            // 타격 지점 및 방향 연산
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 hitDirection = (other.transform.position - transform.position).normalized;

            // Named Arguments 방식
            // 가독성 향상: 어떤 데이터를 입력했는지 파악에 용이
            // 타입 혼동 방지: 매개변수 이름을 통해 타입 혼동을 방지
            // 유연셩: 매개변수의 순서에 구애받지 않고 더 나은 코드 유지보수를 가능하게 함
            DamageInfo damageInfo = new DamageInfo(
                amount: baseDamage,
                hitPoint: hitPoint,
                hitNormal: -hitDirection,
                hitDirection: hitDirection,
                instigator: transform.root.gameObject
            );
             
            damageable.TakeDamage(damageInfo);
        }
    }

    private void Reset()
    {
        hitCollider = GetComponent<Collider>();
    }
}