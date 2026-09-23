using UnityEngine;

/// <summary>
/// 제자리에서 타깃을 향해 몸을 회전시키는 액션 노드.
/// </summary>
public class BTActionLookAtTarget : BTActionNode
{
    [Tooltip("타겟을 향한 회전 보간 속도")]
    [SerializeField] private float rotationSpeed = 5.0f;

    [Tooltip("타겟을 정면으로 포착했다고 판단하는 허용 각도")]
    [SerializeField] private float facingAngleTolerance = 10.0f;

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

    protected override BTNodeState ExecuteAction()
    {
        // 1. 이미 공격 모션이 진행 중인 경우:
        // 회전을 멈추고 즉시 Success를 반환하여 시퀀스가 뒤쪽 BTActionAttack을 계속 실행하도록 통과시킵니다.
        if (attackAction != null && attackAction.IsAttacking)
        {
            return BTNodeState.Success;
        }

        // 2. 공격 중이 아닐 때 타깃 유효성 검사
        if (enemyBlackboard == null || !enemyBlackboard.HasTarget)
            return BTNodeState.Failure;

        Transform owner = enemyBlackboard.Owner;
        Transform target = enemyBlackboard.Target;

        if (owner == null || target == null)
            return BTNodeState.Failure;

        // 3. 타깃을 향한 회전 방향 계산 (수평 기준)
        Vector3 directionToTarget = target.position - owner.position;
        directionToTarget.y = 0f;

        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            owner.rotation = Quaternion.Slerp(owner.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // 4. 공격 쿨다운 대기 상태: 주시하며 대치(Running) 유지
        if (!enemyBlackboard.IsAttackReady)
        {
            return BTNodeState.Running;
        }

        // 5. 공격 준비 완료 상태: 타깃을 정면으로 포착했는지 확인
        float currentAngle = Vector3.Angle(owner.forward, directionToTarget.normalized);
        if (currentAngle <= facingAngleTolerance)
        {
            // 정면 조준 완료 -> Attack 노드로 즉시 진입
            return BTNodeState.Success;
        }

        return BTNodeState.Running;
    }
}