using UnityEngine;

/// <summary>
/// 행동 트리(BT)의 최상위 진입점(RootNode) 클래스
/// </summary>
public class BTRootNode : MonoBehaviour
{
    [Header("Tree Settings")]
    [Tooltip("트리 실행 주기(초)")]
    [SerializeField] private float updateInterval = 0.1f;

    [Header("Debug")]
    [Tooltip("현재 트리의 최종 반환 상태")]
    [SerializeField] private BTNodeState currentTreeState = BTNodeState.None;

    [Tooltip("현재 Running 중이거나 마지막으로 결과를 낸 하위 노드 이름")]
    [SerializeField] private string currentActiveNodeName = "None";

    [SerializeField] private BTBlackboard blackboard;

    private BTNode rootChildNode;

    // 주기 실행용 타이머
    private float timer = 0f;

    // 프로퍼티
    public BTBlackboard Blackboard => blackboard;

    protected virtual void Awake()
    {
        // 자식 클래스에 맞는 타입의 블랙보드 할당
        blackboard = CreateBlackboard();

        Transform ownerTransform = transform.parent != null ? transform.parent : transform;
        blackboard.Initialize(ownerTransform);

        // 루트 자식 노드 탐색
        rootChildNode = GetComponentInChildren<BTNode>();
        if (rootChildNode == null)
        {
            Debug.LogError($"[{name}] BTRootNode: 하위에 실행할 BTNode가 존재하지 않습니다.");
            enabled = false;
            return;
        }

        // 트리 전체에 블랙보드 저장 및 초기화
        rootChildNode.Initialize(blackboard);
    }

    protected virtual void Update()
    {
        if (rootChildNode == null) return;

        // 실행 주기(Interval) 제어
        if (updateInterval > 0f)
        {
            timer += Time.deltaTime;
            if (timer < updateInterval) return;
            timer = 0f;
        }

        // BTBlackboard Update() 호출
        blackboard.Update();

        // 트리 검사
        currentTreeState = rootChildNode.Evaluate();

        // 현재 실행 중이거나 결과를 낸 노드 이름 추적
        currentActiveNodeName = FindActiveNodeName(rootChildNode);
    }

    /// <summary>
    /// 하위 트리에서 현재 실행(Running) 중이거나 마지막으로 활성화된 리프 노드 이름을 탐색
    /// </summary>
    private string FindActiveNodeName(BTNode node)
    {
        if (node == null) return "None";

        // 복합 노드(Composite)인 경우 Children 프로퍼티를 통해 자식을 추적
        if (node is BTCompositeNode compositeNode && compositeNode.Children != null)
        {
            // 자식 중 Running 중인 노드가 있다면 그 안으로 더 깊이 탐색
            for (int i = 0; i < compositeNode.Children.Count; i++)
            {
                var child = compositeNode.Children[i];
                if (child != null && child.NodeState == BTNodeState.Running)
                {
                    return FindActiveNodeName(child);
                }
            }

            // Running이 없다면 마지막으로 유효한 결과를 낸 자식 노드 반환
            for (int i = compositeNode.Children.Count - 1; i >= 0; i--)
            {
                var child = compositeNode.Children[i];
                if (child != null && child.NodeState != BTNodeState.None)
                {
                    return $"{child.gameObject.name} ({child.NodeState})";
                }
            }
        }

        // Action이나 Condition 등 단일 노드인 경우
        return $"{node.gameObject.name} ({node.NodeState})";
    }

    /// <summary>
    /// 블랙보드 인스턴스 생성
    /// 팩토리 메서드 패턴 활용
    /// </summary>
    protected virtual BTBlackboard CreateBlackboard()
    {
        return new BTBlackboard();
    }
}