using UnityEngine;

/// <summary>
/// 자식 노드 중 하나라도 성공(Success)하거나 실행 중(Running)이면 즉시 해당 상태를 반환하고 중단하는 OR 복합 노드
/// </summary>
public class BTSelectorNode : BTCompositeNode
{
    public override BTNodeState Evaluate()
    {
        for (int i = 0; i < children.Count; i++)
        {
            BTNodeState childState = children[i].Evaluate();

            switch (childState)
            {
                // 실행 중인 노드가 있다면 부모에게 그대로 전달하고 실행 중단
                case BTNodeState.Running:
                    nodeState = BTNodeState.Running;
                    return nodeState;

                // 성공한 자식이 있다면 즉시 성공을 반환하고 실행 중단
                case BTNodeState.Success:
                    nodeState = BTNodeState.Success;
                    return nodeState;

                // 실패했다면 다음 자식을 평가하기 위해 루프 지속
                case BTNodeState.Failure:
                    continue;

                // None, Length 등 의도하지 않은 상태가 반환되면 에러 로그를 출력하고 실패 상태를 반환
                default:
                    Debug.LogError($"[BTSelectorNode] {name}의 자식 노드 {children[i].name}가 유효하지 않은 상태({childState})를 반환했습니다.");
                    nodeState = BTNodeState.Failure;
                    return nodeState;
            }
        }

        // 모든 자식이 실패했다면 Selector도 최종 실패
        nodeState = BTNodeState.Failure;
        return nodeState;
    }
}