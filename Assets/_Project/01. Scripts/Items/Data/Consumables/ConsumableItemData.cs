using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 소모품 아이템 데이터를 정의하는 클래스
/// </summary>
[CreateAssetMenu(fileName = "New Consumable Item", menuName = "Item/Consumable Item Data")]
public class ConsumableItemData : ItemData
{
    [Header("소모품 효과 목록")]
    [SerializeField] private List<ItemEffect> effects = new List<ItemEffect>();

    // 프로퍼티
    public IReadOnlyList<ItemEffect> Effects => effects;

    /// <summary>
    /// 등록된 모든 효과를 순차적으로 실행
    /// </summary>
    public bool Use(GameObject user)
    {
        if (effects == null || effects.Count == 0)
        {
            Debug.LogWarning($"{ItemName}: 적용할 효과가 등록되어 있지 않습니다.");
            return false;
        }

        bool isAnyEffectApplied = false;

        foreach (var effect in effects)
        {
            if (effect != null && effect.Execute(user))
            {
                isAnyEffectApplied = true;
            }
        }

        return isAnyEffectApplied;
    }
}