using UnityEngine;

/// <summary>
/// 플레이어 전용 데이터 베이스 클래스
/// </summary>
[CreateAssetMenu(fileName = "NewPlayerData", menuName = "Character/Data/Player Data")]
public class PlayerData : CharacterData
{
    [Header("플레이어 고유 자원")]
    [Tooltip("최대 스태미나")]
    [SerializeField] private float maxStamina = 100.0f;

    [Tooltip("초당 스태미나 회복량")]
    [SerializeField] private float staminaRecoveryRate = 20.0f;

    [Tooltip("스태미나 회복 시작 전 지연 시간(초)")]
    [SerializeField] private float staminaRecoveryDelay = 1.0f;

    [Tooltip("전력 질주(Sprint) 속도")]
    [SerializeField] private float sprintSpeed = 6.5f;

    [Header("자원 소모값")]
    [Tooltip("구르기/회피 시 스태미나 소모량")]
    [SerializeField] private float dodgeStaminaCost = 20.0f;

    [Tooltip("질주 시 초당 스태미나 소모량")]
    [SerializeField] private float sprintStaminaCostPerSecond = 15.0f;

    #region Properties
    public float MaxStamina => maxStamina;
    public float StaminaRecoveryRate => staminaRecoveryRate;
    public float StaminaRecoveryDelay => staminaRecoveryDelay;
    public float SprintSpeed => sprintSpeed;
    public float DodgeStaminaCost => dodgeStaminaCost;
    public float SprintStaminaCostPerSecond => sprintStaminaCostPerSecond;
    #endregion
}