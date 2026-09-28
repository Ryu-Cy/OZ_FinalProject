using UnityEngine;

/// <summary>
/// 에너미의 블랙보드에 전술 역할이 부여되어 있는지 검사하는 조건 노드
/// </summary>
public class BTConditionHasRole : BTConditionNode
{
    [Header("Role Check Settings")]
    [Tooltip("검사할 대상 역할")]
    [SerializeField] private CombatRole _targetRole = CombatRole.SideAttacker;

    private EnemyBlackboard _enemyBlackboard;

    public override void Initialize(BTBlackboard blackboard)
    {
        base.Initialize(blackboard);
        _enemyBlackboard = blackboard as EnemyBlackboard;
    }

    protected override bool CheckCondition()
    {
        if (_enemyBlackboard == null)
            return false;

        // 타겟이 유효하고, 전술 좌표가 존재하며, 지정된 역할과 일치하는지 검사
        return _enemyBlackboard.HasTarget &&
               _enemyBlackboard.HasAssignedPosition &&
               _enemyBlackboard.Role == _targetRole;
    }
}