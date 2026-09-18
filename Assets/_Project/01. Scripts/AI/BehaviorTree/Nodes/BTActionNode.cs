/// <summary>
/// 실제 행동을 수행하며 Running, Success, Failure를 반환하는 액션 노드 추상 클래스
/// </summary>
public abstract class BTActionNode : BTNode
{
    public override BTNodeState Evaluate()
    {
        // 노드가 구현한 행동 로직을 실행하고 결과를 상태로 저장
        nodeState = ExecuteAction();
        return nodeState;
    }

    /// <summary>
    /// 행동 로직
    /// </summary>
    /// <returns>행동 진행 중(Running), 완료(Success), 실패/취소(Failure)</returns>
    protected abstract BTNodeState ExecuteAction();
}