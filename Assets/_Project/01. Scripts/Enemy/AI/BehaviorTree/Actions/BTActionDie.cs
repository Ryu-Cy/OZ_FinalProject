using UnityEngine;

/// <summary>
/// 사망 애니메이션을 재생하고 시체 비활성화 및 리스폰 요청을 처리하는 액션 노드
/// </summary>
public class BTActionDie : BTActionNode
{
    private EnemyController _controller;
    private bool _hasStartedDeath = false;

    public override void Initialize(BTBlackboard blackboard)
    {
        base.Initialize(blackboard);

        if (blackboard is EnemyBlackboard enemyBlackboard && enemyBlackboard.Owner != null)
        {
            _controller = enemyBlackboard.Owner.GetComponent<EnemyController>();
        }
    }

    protected override BTNodeState ExecuteAction()
    {
        if (_controller == null) return BTNodeState.Failure;

        // 사망 시작
        if (!_hasStartedDeath)
        {
            _hasStartedDeath = true;
            _controller.StartDeathSequence();
            return BTNodeState.Running;
        }

        // 애니메이션 클립의 DieFinished 이벤트 수신 확인
        if (_controller.IsDieFinished())
        {
            // 쓰러짐 연출 완료 후 모델 비활성화 및 리스폰 카운트다운 시작
            _controller.CompleteDeath();
            return BTNodeState.Success;
        }

        return BTNodeState.Running;
    }

    /// <summary>
    /// BT 노드가 다시 시작될 때 상태 초기화
    /// </summary>
    public void ResetNode()
    {
        _hasStartedDeath = false;
    }

    private void OnDisable()
    {
        _hasStartedDeath = false;
    }
}