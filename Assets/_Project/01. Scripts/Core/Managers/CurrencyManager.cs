using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 내 재화 관리 전담 매니저 클래스
/// </summary>
public class CurrencyManager : Singleton<CurrencyManager>, IInitializable
{
    // 재화 타입별 보유량 저장소
    private readonly Dictionary<CurrencyType, int> currencies = new Dictionary<CurrencyType, int>();

    // UI 갱신 등을 위한 변경 알림 이벤트
    public event Action<CurrencyType, int> OnCurrencyChanged;

    public bool IsInitialized { get; private set; }

    public void Initialize()
    {
        if (IsInitialized) return;

        // 기본 재화 초기화
        currencies.Clear();
        for (int i = 1; i < (int)CurrencyType.Length; i++)
        {
            currencies[(CurrencyType)i] = 0;
        }

        IsInitialized = true;
    }

    /// <summary>
    /// 특정 재화의 현재 잔액을 반환
    /// </summary>
    public int GetCurrency(CurrencyType type)
    {
        if (type == CurrencyType.None || type == CurrencyType.Length) return 0;

        return currencies.TryGetValue(type, out int amount) ? amount : 0;
    }

    /// <summary>
    /// 재화 획득
    /// </summary>
    public void AddCurrency(CurrencyType type, int amount)
    {
        if (amount <= 0 || type == CurrencyType.None || type == CurrencyType.Length) return;

        int current = GetCurrency(type);
        int updated = current + amount;
        currencies[type] = updated;

        OnCurrencyChanged?.Invoke(type, updated);
    }

    /// <summary>
    /// 재화 소모 검증
    /// </summary>
    public bool HasCurrency(CurrencyType type, int amount)
    {
        if (amount <= 0) return false;
        return GetCurrency(type) >= amount;
    }

    /// <summary>
    /// 재화 소모
    /// </summary>
    public bool TrySpendCurrency(CurrencyType type, int amount)
    {
        if (!HasCurrency(type, amount)) return false;

        int updated = currencies[type] - amount;
        currencies[type] = updated;

        OnCurrencyChanged?.Invoke(type, updated);
        return true;
    }

    /// <summary>
    /// 소울 드랍 대비 소울 초기화 함수
    /// </summary>
    public int ClearCurrency(CurrencyType type)
    {
        int current = GetCurrency(type);
        if (current <= 0) return 0;

        currencies[type] = 0;
        OnCurrencyChanged?.Invoke(type, 0);
        return current;
    }
}