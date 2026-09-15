using UnityEngine;

/// <summary>
/// 무기 아이템 데이터 클래스
/// </summary>
[CreateAssetMenu(fileName = "New Weapon Item", menuName = "Item/Equipment/Weapon Data")]
public class WeaponItemData : EquipmentItemData
{
    [Header("무기 전투 스탯")]
    [SerializeField] private int attackPower;

    // 프로퍼티
    public int AttackPower => attackPower;

    public override void Equip(GameObject user)
    {
        Debug.Log($"[무기 장착] {user.name}이(가) {ItemName}(공격력: {attackPower})을(를) 장착했습니다.");
    }

    public override void Unequip(GameObject user)
    {
        Debug.Log($"[무기 해제] {user.name}이(가) {ItemName} 장착을 해제했습니다.");
    }
}