using UnityEngine;

/// <summary>
/// 타겟이 공격 사거리 내에 있고 공격 준비가 완료되었는지 검사하는 조건 노드.
/// </summary>
public class BTConditionInAttackRange : BTConditionNode
{
    [SerializeField] private float _rangeBuffer = 0.5f;
    private EnemyBlackboard _enemyBlackboard;

    public override void Initialize(BTBlackboard blackboard)
    {
        base.Initialize(blackboard);
        _enemyBlackboard = blackboard as EnemyBlackboard;
    }

    protected override bool CheckCondition()
    {
        if (_enemyBlackboard == null) return false;

        // 이미 공격 모션이 진행 중이라면 사거리나 쿨다운과 무관하게 시퀀스 유지
        if (_enemyBlackboard.IsAttacking)
        {
            return true;
        }

        Transform owner = _enemyBlackboard.Owner;
        Transform target = _enemyBlackboard.Target;

        if (owner == null || target == null) return false;

        // 공격 사거리 계산
        float attackRange = (_enemyBlackboard.EnemyData != null ? _enemyBlackboard.EnemyData.AttackRange : 2.0f) + _rangeBuffer;
        Vector3 diff = target.position - owner.position;
        diff.y = 0f;
        float currentDist = diff.magnitude;

        // 공격 개시 조건 검사
        bool isReady = _enemyBlackboard.IsAttackReady;
        bool inRange = currentDist <= attackRange;

        if (!inRange || !isReady)
        {
            return false;
        }

        return true;
    }
}