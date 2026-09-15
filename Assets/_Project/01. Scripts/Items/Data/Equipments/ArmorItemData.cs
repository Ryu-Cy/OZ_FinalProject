using UnityEngine;

/// <summary>
/// 방어구 아이템 데이터 클래스
/// </summary>
[CreateAssetMenu(fileName = "New Armor Item", menuName = "Item/Equipment/Armor Data")]
public class ArmorItemData : EquipmentItemData
{
    [Header("방어구 전투 스탯")]
    [SerializeField] private int physicalDefense;

    // 프로퍼티
    public int PhysicalDefense => physicalDefense;

    public override void Equip(GameObject user)
    {
        Debug.Log($"[방어구 장착] {user.name}이(가) {ItemName}(방어력: {physicalDefense})을(를) 착용했습니다.");
    }

    public override void Unequip(GameObject user)
    {
        Debug.Log($"[방어구 해제] {user.name}이(가) {ItemName} 착용을 해제했습니다.");
    }
}