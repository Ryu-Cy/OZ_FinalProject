using UnityEngine;

/// <summary>
/// EnemyVision을 통해 시야(FOV) 내 유효 타겟을 감지하고 블랙보드를 갱신하는 조건 노드
/// </summary>
public class BTConditionFOV : BTConditionNode
{
    private EnemyVision enemyVision;
    private EnemyBlackboard enemyBlackboard;

    public override void Initialize(BTBlackboard blackboard)
    {
        base.Initialize(blackboard);

        enemyBlackboard = blackboard as EnemyBlackboard;

        if (enemyBlackboard?.Owner != null)
        {
            enemyVision = enemyBlackboard.Owner.GetComponent<EnemyVision>();
        }
    }

    /// <summary>
    /// 시야 검사 및 블랙보드 동기화
    /// </summary>
    /// <returns>타겟 발견 시 true(Success), 미발견 시 false(Failure)</returns>
    protected override bool CheckCondition()
    {
        if (enemyVision == null || enemyBlackboard == null)
            return false;

        // EnemyVision을 통해 유효 타겟 탐색
        if (enemyVision.TryFindTarget(out Transform spottedTarget))
        {
            enemyBlackboard.Target = spottedTarget;
            return true;
        }

        // 시야 내에 타겟이 없다면 블랙보드 타겟 해제
        enemyBlackboard.ClearTarget();
        return false;
    }
}