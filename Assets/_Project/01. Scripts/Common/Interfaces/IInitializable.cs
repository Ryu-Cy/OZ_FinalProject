/// <summary>
/// 명시적인 초기화 순서를 보장받는 매니저 및 시스템 컴포넌트용 인터페이스
/// </summary>
public interface IInitializable
{
    /// <summary>
    /// CoreSystems 루트에 의해 하이어라키 자식 순서대로 호출되는 초기화 메서드
    /// </summary>
    void Initialize();
}