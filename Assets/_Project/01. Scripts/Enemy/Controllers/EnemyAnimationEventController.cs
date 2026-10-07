using UnityEngine;

/// <summary>
/// 적 캐릭터의 애니메이션 이벤트를 처리하는 컨트롤러 클래스
/// </summary>
public class EnemyAnimationEventController : MonoBehaviour
{
    [Header("Weapon Reference")]
    [SerializeField] private MeleeWeapon meleeWeapon;

    // 프로퍼티
    public bool IsAttackFinished { get; private set; }
    public bool IsDieFinished { get; private set; }
    public bool IsShootTriggered { get; private set; }

    private void Awake()
    {
        // 무기가 손 본 아래 자식에 위치하므로 자동 탐색
        if (meleeWeapon == null)
        {
            meleeWeapon = GetComponentInChildren<MeleeWeapon>();
        }
    }

    /// <summary>
    /// 공격 상태 초기화
    /// </summary>
    public void ResetAttackState()
    {
        IsAttackFinished = false;
        IsShootTriggered = false;
        if (meleeWeapon != null)
        {
            meleeWeapon.DisableAttack();
        }
    }

    /// <summary>
    /// 애니메이션 클립 공격 판정 시작 이벤트
    /// </summary>
    public void AttackHitStart()
    {
        if (meleeWeapon != null)
        {
            meleeWeapon.EnableAttack();
        }
    }

    /// <summary>
    /// 애니메이션 클립 공격 판정 종료 이벤트
    /// </summary>
    public void AttackHitEnd()
    {
        if (meleeWeapon != null)
        {
            meleeWeapon.DisableAttack();
        }
    }

    /// <summary>
    /// 애니메이션 클립 발사 시점 이벤트
    /// </summary>
    public void ShootProjectile()
    {
        IsShootTriggered = true;
    }

    /// <summary>
    /// 애니메이션 클립 공격 종료 이벤트 
    /// </summary>
    public void AttackFinished()
    {
        if (meleeWeapon != null)
        {
            meleeWeapon.DisableAttack();
        }
        IsAttackFinished = true;
    }

    /// <summary>
    /// 사망 상태 초기화
    /// </summary>
    public void ResetDieState()
    {
        IsDieFinished = false;
    }

    /// <summary>
    /// 사망 애니메이션 종료 이벤트
    /// </summary>
    public void DieFinished()
    {
        IsDieFinished = true;
    }
}