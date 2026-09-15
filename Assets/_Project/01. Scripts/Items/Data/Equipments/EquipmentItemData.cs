using UnityEngine;

/// <summary>
/// 장비 아이템 데이터의 기본 클래스
/// </summary>
public abstract class EquipmentItemData : ItemData
{
    [Header("장비 기본 분류")]
    [SerializeField] private EquipmentType equipmentType;

    // 프로퍼티
    public EquipmentType EquipmentType => equipmentType;

    /// <summary>
    /// 장착
    /// </summary>
    public abstract void Equip(GameObject user);

    /// <summary>
    /// 장착 해제
    /// </summary>
    public abstract void Unequip(GameObject user);
}