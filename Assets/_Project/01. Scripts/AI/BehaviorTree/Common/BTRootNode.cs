using UnityEngine;

/// <summary>
/// 행동 트리(BT)의 최상위 진입점(RootNode) 클래스
/// </summary>
public class BTRootNode : MonoBehaviour
{
    [Header("Tree Settings")]
    [Tooltip("트리를 실행할 주기(초). 0이면 매 프레임(Update) 실행")]
    [SerializeField] private float updateInterval = 0.1f;

    [Header("Debug")]
    [Tooltip("현재 트리의 최종 반환 상태")]
    [SerializeField] private BTNodeState currentTreeState = BTNodeState.None;

    [SerializeField] private BTBlackboard blackboard;

    private BTNode rootChildNode;

    // 주기 실행용 타이머
    private float timer = 0f;

    protected virtual void Awake()
    {
        // 팩토리 메서드를 호출하여 자식 클래스가 원하는 타입의 블랙보드를 강제 할당
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
    }

    /// <summary>
    /// 블랙보드 인스턴스 생성 팩토리 메서드 패턴 활용
    /// 해당 패턴을 활용했을 때 장점
    /// 1. 의존성 분리: BTRootNode는 어떤 블랙보드가 들어오는지 몰라도 된다.
    /// 2. 확장성 (OCP): 새로운 BT가 추가되더라도 기존 코드를 수정할 필요 없다. (몬스터 종류별 조건문 등)
    /// 3. 안정성: 인스펙터 연결 등 타 방식에서 생길 수 있는 휴먼에러 차단
    /// </summary>
    protected virtual BTBlackboard CreateBlackboard()
    {
        return new BTBlackboard();
    }

    /// <summary>
    /// 외부에서 블랙보드에 접근할 수 있도록 열어둔 프로퍼티
    /// </summary>
    public BTBlackboard Blackboard => blackboard;
}