using UnityEngine;

/// <summary>
/// 소울을 획득하는 효과를 정의하는 클래스
/// </summary>
[CreateAssetMenu(fileName = "New Grant Soul Effect", menuName = "Item/Effects/Grant Soul Effect")]
public class GrantSoulEffect : ItemEffect
{
    [SerializeField] private int soulAmount;

    // 프로퍼티
    public int SoulAmount => soulAmount;

    public override bool Execute(GameObject user)
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddCurrency(CurrencyType.Soul, soulAmount);
            Debug.Log($"소울 {soulAmount} 획득 완료.");
            return true;
        }

        return false;
    }
}