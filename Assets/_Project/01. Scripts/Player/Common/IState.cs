/// <summary>
/// 모든 상태 클래스가 구현해야 하는 기본 인터페이스
/// </summary>
public interface IState
{
    void Enter();   // 상태 진입
    void Update();  // 매 프레임 호출
    void Exit();    // 상태 종료
}