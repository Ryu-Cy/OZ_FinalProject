using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 근접 무기 타격 판정 및 콜라이더 제어 클래스
/// </summary>
public class MeleeWeapon : MonoBehaviour
{
    [Header("Hit Collider")]
    [SerializeField] private Collider hitCollider;

    // 1회 공격 궤적 동안 이미 맞은 타깃 목록
    private readonly HashSet<Collider> hitTargets = new HashSet<Collider>();

    private void Awake()
    {
        if (hitCollider == null)
        {
            hitCollider = GetComponent<Collider>();
        }

        DisableAttack();
    }

    /// <summary>
    /// 공격 판정 시작
    /// </summary>
    public void EnableAttack()
    {
        hitTargets.Clear();

        if (hitCollider != null)
        {
            hitCollider.enabled = true;
        }
    }

    /// <summary>
    /// 공격 판정 종료
    /// </summary>
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
        // 이번 휘두름에 이미 타격된 대상이면 패스
        if (hitTargets.Contains(other)) return;

        hitTargets.Add(other);

        Debug.Log($"[MeleeWeapon] 타격 성공: {other.name}");
    }

    private void Reset()
    {
        hitCollider = GetComponent<Collider>();
    }
}