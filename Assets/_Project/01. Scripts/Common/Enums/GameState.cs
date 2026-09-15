/// <summary>
/// 게임의 런타임 진행 상태 열거형
/// </summary>
public enum GameState
{
    None = 0,
    Title,      // 타이틀 씬에 머무는 상태
    InGame,     // 인게임 정상 플레이 진행 중
    Paused,     // 일시정지
    GameOver,   // 플레이어 사망
    Length
}