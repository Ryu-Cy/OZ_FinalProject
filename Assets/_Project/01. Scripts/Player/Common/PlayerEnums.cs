/// <summary>
/// 플레이어 FSM 상태 식별용 Enum
/// </summary>
public enum PlayerStateType
{
    None = 0,
    Idle,       // 대기
    Move,       // 이동
    Sprint,     // 질주
    Dodge,      // 회피
    Attack,     // 공격
    Length
}

/// <summary>
/// 좌클릭 기본 3단 공격 콤보 단계
/// </summary>
public enum AttackComboType
{
    None = 0,
    Combo1,     // 1단 공격
    Combo2,     // 2단 공격
    Combo3,     // 3단 공격
    Length
}