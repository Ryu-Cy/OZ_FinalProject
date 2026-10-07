using UnityEngine;

/// <summary>
/// 
/// </summary>
public class BTConditionIsDead : BTConditionNode
{
    private EnemyHealth _enemyHealth;

    public override void Initialize(BTBlackboard blackboard)
    {
        base.Initialize(blackboard);
        CacheHealth();
    }

    /// <summary>
    /// EnemyHealth 컴포넌트 캐싱
    /// </summary>
    private void CacheHealth()
    {
        if (_enemyHealth == null && blackboard is EnemyBlackboard enemyBlackboard && enemyBlackboard.Owner != null)
        {
            _enemyHealth = enemyBlackboard.Owner.GetComponent<EnemyHealth>();
        }
    }

    /// <summary>
    /// 체력이 0 이하인지 확인
    /// </summary>
    protected override bool CheckCondition()
    {
        if (_enemyHealth == null)
        {
            CacheHealth();
            if (_enemyHealth == null) return false;
        }

        // 실제로 맞아서 체력이 0 이하로 떨어졌을 때만 true
        return _enemyHealth.IsDead || _enemyHealth.CurrentHealth <= 0.0f;
    }
}