using UnityEngine;

/// <summary>
/// EnemyData에 정의된 기본 공격 사거리 내에 타겟이 존재하는지 검사하는 조건 노드
/// </summary>
public class BTConditionInAttackRange : BTConditionNode
{
    private EnemyBlackboard enemyBlackboard;
    private BTActionAttack attackAction;

    public override void Initialize(BTBlackboard blackboard)
    {
        base.Initialize(blackboard);
        enemyBlackboard = blackboard as EnemyBlackboard;

        if (enemyBlackboard?.Owner != null)
        {
            attackAction = enemyBlackboard.Owner.GetComponentInChildren<BTActionAttack>();
        }
    }

    protected override bool CheckCondition()
    {
        // [1순위] 이미 칼을 휘두르는 중이라면, 타깃이 사라졌든 도망쳤든 0.5초 끝날 때까지 무조건 통과!
        if (attackAction != null && attackAction.IsAttacking)
        {
            return true;
        }

        // 공격 중이 아닐 때만 타깃 유효성 검사
        if (enemyBlackboard == null || !enemyBlackboard.HasTarget)
            return false;

        Transform owner = enemyBlackboard.Owner;
        Transform target = enemyBlackboard.Target;

        if (owner == null || target == null)
            return false;

        float attackRange = enemyBlackboard.EnemyData != null ? enemyBlackboard.EnemyData.AttackRange : 2.0f;
        Vector3 diff = target.position - owner.position;
        diff.y = 0f;

        return diff.sqrMagnitude <= attackRange * attackRange;
    }
}