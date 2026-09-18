/// <summary>
/// Behavior Tree 노드의 실행 및 결과를 나타내는 상태 열거형
/// </summary>
public enum BTNodeState
{
    None = 0,
    Running, // 실행 중
    Success, // 실행 성공
    Failure,  // 실행 실패
    Length
}