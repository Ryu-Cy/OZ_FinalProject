/// <summary>
/// 특정 조건을 검사하여 Success 또는 Failure를 즉시 반환하는 조건 노드 추상 클래스
/// </summary>
public abstract class BTConditionNode : BTNode
{
    /// <summary>
    /// CheckCondition()의 결과에 따라 상태를 결정
    /// </summary>
    public override BTNodeState Evaluate()
    {
        // 노드 조건 검사
        bool isConditionMet = CheckCondition();

        nodeState = isConditionMet ? BTNodeState.Success : BTNodeState.Failure;
        return nodeState;
    }

    /// <summary>
    /// 조건 검사
    /// </summary>
    /// <returns>조건 충족 시 true(Success), 미충족 시 false(Failure)</returns>
    protected abstract bool CheckCondition();
}