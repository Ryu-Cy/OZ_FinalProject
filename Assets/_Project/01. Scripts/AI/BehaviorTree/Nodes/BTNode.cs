using UnityEngine;

/// <summary>
/// Behavior Tree 노드의 최상위 추상 베이스 클래스
/// </summary>
public abstract class BTNode : MonoBehaviour
{
    protected BTNodeState nodeState;
    protected BTBlackboard blackboard;

    // 프로퍼티
    public BTNodeState NodeState => nodeState;

    /// <summary>
    /// 블랙보드 저장
    /// </summary>
    public virtual void Initialize(BTBlackboard blackboard)
    {
        this.blackboard = blackboard;
    }

    /// <summary>
    /// 노드의 로직 갱신
    /// </summary>
    public abstract BTNodeState Evaluate();
}