using UnityEngine;

/// <summary>
/// BT 테스트용 조건 노드 <br/>
/// 설정해준 canExecute 값에 따라 성공(Success) 또는 실패(Failure) 반환
/// </summary>
public class BTConditionTest : BTConditionNode
{
    [Header("Test Settings")]
    [SerializeField] private bool canExecute = true;

    protected override bool CheckCondition()
    {
        Debug.Log($"[{name}] 조건 검사 -> {(canExecute ? "성공(True)" : "실패(False)")}.");
        return canExecute;
    }
}