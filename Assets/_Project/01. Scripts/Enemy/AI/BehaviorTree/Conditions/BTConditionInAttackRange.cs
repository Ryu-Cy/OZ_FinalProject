using UnityEngine;

/// <summary>
/// EnemyData에 정의된 기본 공격 사거리 내에 타겟이 존재하는지 검사하는 조건 노드
/// </summary>
public class BTConditionInAttackRange : BTConditionNode
{
    private EnemyBlackboard enemyBlackboard;

    public override void Initialize(BTBlackboard blackboard)
    {
        base.Initialize(blackboard);

        enemyBlackboard = blackboard as EnemyBlackboard;
    }

    protected override bool CheckCondition()
    {
        if (enemyBlackboard == null || !enemyBlackboard.HasTarget)
            return false;

        // EnemyData 유효성 검사
        if (enemyBlackboard.EnemyData == null)
            return false;

        Transform owner = enemyBlackboard.Owner;
        Transform target = enemyBlackboard.Target;

        if (owner == null || target == null)
            return false;

        // EnemyData에 정의된 기본 공격 사거리 가져오기
        float attackRange = enemyBlackboard.EnemyData.AttackRange;

        // 사거리 진입 판정
        float sqrDistance = (target.position - owner.position).sqrMagnitude;
        float sqrAttackRange = attackRange * attackRange;

        return sqrDistance <= sqrAttackRange;
    }
}