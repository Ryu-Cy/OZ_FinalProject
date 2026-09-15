using UnityEngine;

/// <summary>
/// 아이템 데이터의 기본 클래스
/// </summary>
public abstract class ItemData : ScriptableObject
{
    [Header("기본 식별 정보")]
    [SerializeField] private int id;
    [SerializeField] private string itemName;
    [TextArea(3, 5)]
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;

    [Header("아이템 규칙")]
    [SerializeField] private ItemType itemType;
    [SerializeField] private int maxStack = 1;

    // 프로퍼티
    public int Id => id;
    public string ItemName => itemName;
    public string Description => description;
    public Sprite Icon => icon;
    public ItemType ItemType => itemType;
    public int MaxStack => maxStack;
}