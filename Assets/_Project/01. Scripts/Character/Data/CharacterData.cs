using UnityEngine;

/// <summary>
/// 모든 캐릭터(Player, Enemy)가 공유하는 기본 데이터 베이스 추상 클래스
/// </summary>
public abstract class CharacterData : ScriptableObject
{
    [Header("기본 정보")]
    [Tooltip("이름")]
    [SerializeField] private string characterName = "Character";

    [Header("기초 스탯")]
    [Tooltip("최대 체력")]
    [SerializeField] private float maxHealth = 100.0f;

    [Tooltip("기본 공격력")]
    [SerializeField] private float baseAttackPower = 10.0f;

    [Tooltip("기본 방어력")]
    [SerializeField] private float baseDefense = 10.0f;

    [Tooltip("기본 이동 속도")]
    [SerializeField] private float moveSpeed = 3.0f;

    [Tooltip("기본 회전 속도")]
    [SerializeField] private float rotationSpeed = 360.0f;

    #region Properties
    public string CharacterName => characterName;
    public float MaxHealth => maxHealth;
    public float BaseAttackPower => baseAttackPower;
    public float BaseDefense => baseDefense;
    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;
    #endregion
}