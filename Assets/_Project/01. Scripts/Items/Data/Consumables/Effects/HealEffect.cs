using UnityEngine;

/// <summary>
/// 체력 회복 효과를 정의하는 클래스
/// </summary>
[CreateAssetMenu(fileName = "New Heal Effect", menuName = "Item/Effects/Heal Effect")]
public class HealEffect : ItemEffect
{
    [SerializeField] private int healAmount;

    // 프로퍼티
    public int HealAmount => healAmount;

    public override bool Execute(GameObject user)
    {
        Debug.Log($"{user.name}의 체력을 {healAmount}만큼 회복합니다.");
        return true;
    }
}