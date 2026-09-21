using UnityEngine;

/// <summary>
/// BT 테스트용 행동 노드 <br/>
/// targetTickCount만큼 수행하면 성공(Success) 반환
/// </summary>
public class BTActionTest : BTActionNode
{
    [Header("Test Settings")]
    [SerializeField] private int targetTickCount = 3;
    private int currentTick = 0;

    protected override BTNodeState ExecuteAction()
    {
        currentTick++;
        Debug.Log($"[{name}] ing... ({currentTick}/{targetTickCount})");

        if (currentTick < targetTickCount)
        {
            return BTNodeState.Running;
        }

        currentTick = 0;
        Debug.Log($"[{name}] 행동 완료. (Success)");
        return BTNodeState.Success;
    }
}