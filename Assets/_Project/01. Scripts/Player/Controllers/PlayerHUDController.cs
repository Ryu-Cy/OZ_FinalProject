using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 플레이어의 체력, 스태미나, 퀵슬롯, 소지 재화를 화면에 표시하는 HUD 컨트롤러
/// </summary>
public class PlayerHUDController : MonoBehaviour
{
    [Header("Player References")]
    [Tooltip("플레이어 객체에 부착된 스탯 및 퀵슬롯 컨트롤러 참조")]
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerQuickSlotController quickSlotController;

    [Header("Top-Left: Vitals")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider staminaSlider;

    [Header("Bottom-Left: Quick Slot")]
    [SerializeField] private Image consumableIcon;
    [SerializeField] private TextMeshProUGUI consumableCountText;

    [Header("Bottom-Right: Currency")]
    [SerializeField] private TextMeshProUGUI soulCountText;

    private void OnEnable()
    {
        // 플레이어 스탯 이벤트 구독
        if (playerStats != null)
        {
            playerStats.OnHealthChanged += UpdateHealthUI;
            playerStats.OnStaminaChanged += UpdateStaminaUI;
        }

        // 퀵슬롯 이벤트 구독
        if (quickSlotController != null)
        {
            quickSlotController.OnConsumableChanged += UpdateQuickSlotUI;
        }

        // 재화(소울) 이벤트 구독
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged += UpdateCurrencyUI;

            // 초기 소울 UI 동기화
            UpdateCurrencyUI(CurrencyType.Soul, CurrencyManager.Instance.GetCurrency(CurrencyType.Soul));
        }
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= UpdateHealthUI;
            playerStats.OnStaminaChanged -= UpdateStaminaUI;
        }

        if (quickSlotController != null)
        {
            quickSlotController.OnConsumableChanged -= UpdateQuickSlotUI;
        }

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.OnCurrencyChanged -= UpdateCurrencyUI;
        }
    }

    /// <summary>
    /// 체력 바 갱신
    /// </summary>
    private void UpdateHealthUI(float current, float max)
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = max;
            healthSlider.value = current;
        }
    }

    /// <summary>
    /// 스태미나 바 갱신
    /// </summary>
    private void UpdateStaminaUI(float current, float max)
    {
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = max;
            staminaSlider.value = current;
        }
    }

    /// <summary>
    /// 퀵슬롯 아이콘 및 남은 개수 갱신
    /// </summary>
    private void UpdateQuickSlotUI(ConsumableItemData itemData, int count)
    {
        if (itemData != null && consumableIcon != null)
        {
            consumableIcon.sprite = itemData.Icon;
            consumableIcon.enabled = true;
        }
        else if (consumableIcon != null)
        {
            consumableIcon.enabled = false; // 아이템이 없으면 아이콘 숨김
        }

        if (consumableCountText != null)
        {
            consumableCountText.text = count > 0 ? count.ToString() : "0";
        }
    }

    /// <summary>
    /// 획득한 소울량 갱신
    /// </summary>
    private void UpdateCurrencyUI(CurrencyType type, int amount)
    {
        // 소울 타입일 때만 UI 갱신
        if (type == CurrencyType.Soul && soulCountText != null)
        {
            soulCountText.text = amount.ToString("N0"); // 천 단위 콤마 포맷
        }
    }
}