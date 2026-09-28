using UnityEngine;

/// <summary>
/// EnemyVision을 통해 시야(FOV) 내 유효 타겟 감지
/// </summary>
public class BTConditionFOV : BTConditionNode
{
    private EnemyVision _enemyVision;
    private EnemyBlackboard _enemyBlackboard;
    private EnemyController _enemyController;

    public override void Initialize(BTBlackboard blackboard)
    {
        base.Initialize(blackboard);

        _enemyBlackboard = blackboard as EnemyBlackboard;

        if (_enemyBlackboard?.Owner != null)
        {
            _enemyVision = _enemyBlackboard.Owner.GetComponent<EnemyVision>();
            _enemyController = _enemyBlackboard.Owner.GetComponent<EnemyController>();
        }
    }

    /// <summary>
    /// 시야 검사 및 블랙보드/전투 매니저 동기화
    /// </summary>
    /// <returns>타겟 유효 시 true(Success), 완전 유실 시 false(Failure)</returns>
    protected override bool CheckCondition()
    {
        if (_enemyVision == null || _enemyBlackboard == null)
            return false;

        bool inSight = false;
        Transform spottedTarget = null;

        // EnemyVision을 통해 실시간 시야 탐색 시도
        if (_enemyVision.TryFindTarget(out spottedTarget))
        {
            inSight = true;
            _enemyBlackboard.Target = spottedTarget;

            // CombatManager에 교전 대상으로 등록 유지
            if (CombatManager.HasInstance && _enemyController != null)
            {
                CombatManager.Instance.RegisterEnemy(_enemyController);
            }
        }

        // 이미 타깃을 보유하고 있는 경우 (유예 시간 동안 유지)
        if (_enemyBlackboard.HasTarget)
        {
            Vector3 targetPos = (inSight && spottedTarget != null)
                ? spottedTarget.position
                : _enemyBlackboard.Target.position;

            // 시야 상태를 블랙보드에 실시간 갱신
            _enemyBlackboard.NotifyTargetSightStatus(inSight, targetPos);

            // 유예 시간이 남아있는 동안은 계속 Success 반환 (Patrol로 떨어지는 것 방지)
            return true;
        }

        // 유예 시간이 만료되어 타깃이 null이 된 경우에 정리 및 순찰 허용
        if (CombatManager.HasInstance && _enemyController != null)
        {
            CombatManager.Instance.UnregisterEnemy(_enemyController);
        }

        return false;
    }
}