using UnityEngine;

/// <summary>
/// 방패 아이템 데이터 클래스
/// </summary>
[CreateAssetMenu(fileName = "New Shield Item", menuName = "Item/Equipment/Shield Data")]
public class ShieldItemData : EquipmentItemData
{
    [Header("방패 방어 스탯")]
    [Range(0f, 100f)]
    [SerializeField] private float damageReduction;

    // 프로퍼티
    public float DamageReduction => damageReduction;

    public override void Equip(GameObject user)
    {
        Debug.Log($"[방패 장착] {user.name}이(가) {ItemName}(경감률: {damageReduction}%)을(를) 장착했습니다.");
    }

    public override void Unequip(GameObject user)
    {
        Debug.Log($"[방패 해제] {user.name}이(가) {ItemName} 장착을 해제했습니다.");
    }
}