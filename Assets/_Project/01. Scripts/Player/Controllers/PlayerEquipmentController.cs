using System;
using UnityEngine;

/// <summary>
/// 플레이어의 장비 장착 상태를 관리하는 컨트롤러
/// </summary>
public class PlayerEquipmentController : MonoBehaviour
{
    private PlayerStats playerStats;

    [Header("Equipped Weapon")]
    [Tooltip("게임 시작 시 기본으로 장착할 무기")]
    [SerializeField] private WeaponItemData currentWeapon;

    // 장비 변경 UI 연동용 이벤트
    public event Action<WeaponItemData> OnWeaponEquipped;

    public WeaponItemData CurrentWeapon => currentWeapon;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        // 시작 시 인스펙터에 세팅된 무기 장착 처리
        if (currentWeapon != null)
        {
            EquipWeapon(currentWeapon);
        }
    }

    /// <summary>
    /// 무기 장착 및 스탯 연동
    /// </summary>
    public void EquipWeapon(WeaponItemData newWeapon)
    {
        if (newWeapon == null) return;

        if (currentWeapon != null && currentWeapon != newWeapon)
        {
            currentWeapon.Unequip(gameObject); // 기존 무기 해제
        }

        currentWeapon = newWeapon;
        currentWeapon.Equip(gameObject); // 새 무기 장착

        // PlayerStats에 무기 공격력과 스태미나 소모량을 전달하여 덮어씌움
        if (playerStats != null)
        {
            playerStats.UpdateWeaponStats(currentWeapon.AttackPower, currentWeapon.AttackStaminaCost);
        }

        OnWeaponEquipped?.Invoke(currentWeapon);
    }
}