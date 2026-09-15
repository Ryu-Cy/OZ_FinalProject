/// <summary>
/// 인게임 씬 종류 식별 열거형
/// </summary>
public enum SceneType
{
    None = 0,
    Title,      // 타이틀 화면
    Loading,    // 비동기 씬 전환용 로딩 씬
    MainGame,   // 인게임 플레이 씬
#if UNITY_EDITOR
    Test,       // 테스트 씬 공통
#endif
    Length
}
