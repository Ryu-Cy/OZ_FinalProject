/// <summary>
/// 에너미의 기본 전투 스타일 분류
/// </summary>
public enum EnemyCombatType
{
    None = 0,
    Melee,      // 근거리
    Ranged,     // 원거리
    Length
}

/// <summary>
/// 전투 조율자(Coordinator)가 전술에 따라 부여하는 동적 역할
/// </summary>
public enum CombatRole
{
    None = 0,           // 기본 단독 행동
    FrontAttacker,      // 전술 1: 플레이어 정면 1자 압박
    SideAttacker,       // 전술 1: 플레이어 시야 밖 측/후방 우회 기습
    BodyBlocker,        // 전술 2: 플레이어와 원거리 적 사이를 가로막는 보디가드/차단
    RangedAttacker,     // 전술 2: 후방 거리 유지 및 사격 지원
    Length
}