using System;
using UnityEngine;

/// <summary>
/// 퀵슬롯에 등록된 단일 소모품의 상태와 사용을 관리하는 컨트롤러
/// </summary>
public class PlayerQuickSlotController : MonoBehaviour
{
    [Header("Quick Slot Setup")]
    [Tooltip("퀵슬롯에 등록해둘 소모품")]
    [SerializeField] private ConsumableItemData currentConsumable;

    [Tooltip("시작 시 지급할 소모품 개수")]
    [SerializeField] private int consumableCount = 3;

    private PlayerInputController inputController;

    // UI 갱신용 이벤트
    public event Action<ConsumableItemData, int> OnConsumableChanged;

    // 프로퍼티
    public ConsumableItemData CurrentConsumable => currentConsumable;
    public int ConsumableCount => consumableCount;

    private void Awake()
    {
        if (inputController == null)
        {
            inputController = GetComponent<PlayerInputController>();
        }
    }

    private void Start()
    {
        // 시작 시 초기 UI 세팅
        if (currentConsumable != null)
        {
            OnConsumableChanged?.Invoke(currentConsumable, consumableCount);
        }
    }

    private void OnEnable()
    {
        if (inputController != null)
        {
            inputController.OnUseItemTriggered += UseConsumable;
        }
    }

    private void OnDisable()
    {
        if (inputController != null)
        {
            inputController.OnUseItemTriggered -= UseConsumable;
        }
    }

    /// <summary>
    /// 퀵슬롯 아이템 사용
    /// </summary>
    public void UseConsumable()
    {
        if (currentConsumable == null || consumableCount <= 0)
        {
            Debug.Log("사용할 소모품이 없거나 개수가 부족합니다.");
            return;
        }

        // 아이템 효과 실행
        bool isUsed = currentConsumable.Use(gameObject);

        if (isUsed)
        {
            consumableCount--;
            OnConsumableChanged?.Invoke(currentConsumable, consumableCount);
        }
    }

    /// <summary>
    /// 아이템 획득 시 퀵슬롯 개수 추가
    /// </summary>
    public void AddConsumable(int amount)
    {
        if (currentConsumable == null) return;

        // 최대 소지 개수(MaxStack)를 넘지 않도록 제한
        consumableCount = Mathf.Min(currentConsumable.MaxStack, consumableCount + amount);
        OnConsumableChanged?.Invoke(currentConsumable, consumableCount);
    }
}