using UnityEngine;

public class BTConditionInAttackRange : BTConditionNode
{
    [SerializeField] private float _rangeBuffer = 0.5f;
    private EnemyBlackboard _enemyBlackboard;
    private BTActionAttack _attackAction;

    public override void Initialize(BTBlackboard blackboard)
    {
        base.Initialize(blackboard);
        _enemyBlackboard = blackboard as EnemyBlackboard;

        if (_enemyBlackboard?.Owner != null)
        {
            _attackAction = _enemyBlackboard.Owner.GetComponentInChildren<BTActionAttack>();
        }
    }

    protected override bool CheckCondition()
    {
        if (_enemyBlackboard == null) return false;

        Transform owner = _enemyBlackboard.Owner;
        Transform target = _enemyBlackboard.Target;

        if (owner == null || target == null) return false;

        float attackRange = (_enemyBlackboard.EnemyData != null ? _enemyBlackboard.EnemyData.AttackRange : 2.0f) + _rangeBuffer;
        Vector3 diff = target.position - owner.position;
        diff.y = 0f;
        float currentDist = diff.magnitude;

        bool isReady = _enemyBlackboard.IsAttackReady;
        bool inRange = currentDist <= attackRange;

        if (!inRange || !isReady)
        {
            return false;
        }

        return true;
    }
}