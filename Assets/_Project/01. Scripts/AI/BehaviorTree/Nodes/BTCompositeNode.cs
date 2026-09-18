using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 자식 노드들을 보유하고 실행 흐름을 제어하는 복합 노드들의 추상 베이스 클래스 <br/>
/// Selector, Sequence 등
/// </summary>
public abstract class BTCompositeNode : BTNode
{
    [Header("자식 노드 목록")]
    [SerializeField] protected List<BTNode> children = new List<BTNode>();

    // 프로퍼티
    public List<BTNode> Children => children;

    /// <summary>
    /// 하이어라키 자식 노드들을 세팅하고 각 자식에 블랙보드 저장
    /// </summary>
    public override void Initialize(BTBlackboard blackboard)
    {
        base.Initialize(blackboard);

        SetChildren();

        // 세팅된 모든 자식 노드에 동일한 블랙보드 주입
        for (int i = 0; i < children.Count; i++)
        {
            children[i].Initialize(blackboard);
        }
    }

    /// <summary>
    /// 하이어라키 기반으로 자식 노드 목록 세팅
    /// </summary>
    protected virtual void SetChildren()
    {
        children.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            BTNode childNode = transform.GetChild(i).GetComponent<BTNode>();
            if (childNode != null)
            {
                children.Add(childNode);
            }
        }
    }
}