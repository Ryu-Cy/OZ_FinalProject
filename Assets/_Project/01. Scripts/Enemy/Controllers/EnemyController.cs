using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 적 캐릭터의 생명주기 및 하위 컴포넌트(BT, AI, 데이터)를 총괄 제어하는 메인 컨트롤러
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [Header("Enemy Data")]
    [Tooltip("적의 스탯 및 기본 데이터")]
    [SerializeField] private EnemyData enemyData;

    [Header("AI Blackboard")]
    [SerializeField] private EnemyBlackboard enemyBlackboard;

    private NavMeshAgent navAgent;

    #region Properties
    public EnemyData EnemyData => enemyData;
    public EnemyBlackboard Blackboard => enemyBlackboard;
    #endregion

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();

        InitializeBlackboard();
    }

    /// <summary>
    /// 블랙보드 초기 참조 확인 및 초기화 라이프사이클 실행
    /// </summary>
    private void InitializeBlackboard()
    {
        if (enemyBlackboard == null)
        {
            enemyBlackboard = GetComponent<EnemyBlackboard>();
        }

        if (enemyBlackboard != null)
        {
            // 트랜스폼 기반 블랙보드 원점 및 상태 초기화
            enemyBlackboard.Initialize(transform);

            // 컨트롤러가 들고 있는 ScriptableObject 데이터 주입
            enemyBlackboard.EnemyData = enemyData;
        }
    }
}