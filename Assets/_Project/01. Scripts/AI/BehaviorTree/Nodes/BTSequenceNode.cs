using UnityEngine;

/// <summary>
/// 모든 자식 노드가 순차적으로 성공(Success)해야 최종 성공하는 AND 복합 노드 <br/>
/// 자식 중 하나라도 실패(Failure)하면 즉시 실패를 반환하고 평가를 중단
/// </summary>
public class BTSequenceNode : BTCompositeNode
{
    public override BTNodeState Evaluate()
    {
        for (int i = 0; i < children.Count; i++)
        {
            BTNodeState childState = children[i].Evaluate();

            switch (childState)
            {
                // 실패한 자식이 하나라도 있으면 시퀀스 즉시 중단 및 실패
                case BTNodeState.Failure:
                    nodeState = BTNodeState.Failure;
                    return nodeState;

                // 진행 중인 행동이 있다면 시퀀스를 유지하며 실행 중 반환
                case BTNodeState.Running:
                    nodeState = BTNodeState.Running;
                    return nodeState;

                // 성공했다면 다음 단계 노드로 넘어가기 위해 루프 지속
                case BTNodeState.Success:
                    continue;

                // None, Length 등 의도하지 않은 상태가 반환되면 에러 로그를 출력하고 실패 상태를 반환
                default:
                    Debug.LogError($"[BTSequenceNode] {name}의 자식 노드 {children[i].name}가 유효하지 않은 상태({childState})를 반환했습니다.");
                    nodeState = BTNodeState.Failure;
                    return nodeState;
            }
        }

        // 모든 자식이 무사히 통과했다면 최종 성공
        nodeState = BTNodeState.Success;
        return nodeState;
    }
}