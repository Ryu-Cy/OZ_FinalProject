using UnityEngine;

/// <summary>
/// 적 본체의 시야각(FOV) 및 장애물 차폐를 물리적으로 검사하고 시각화하는 컴포넌트
/// </summary>
public class EnemyVision : MonoBehaviour
{
    [Header("Eye Offset")]
    [Tooltip("레이캐스트 기준점 보정을 위한 눈높이 오프셋 (지면 발밑 기준)")]
    [SerializeField] private Vector3 eyeOffset = new Vector3(0.0f, 1.5f, 0.0f);

    // 컨트롤러로부터 주입받는 데이터 참조
    private EnemyData enemyData;

    // 가비지 컬렉션(GC) 방지를 위한 충돌체 캐싱 버퍼
    private readonly Collider[] hitColliders = new Collider[10];

    // 프로퍼티
    public EnemyData Data => enemyData;
    public Vector3 EyePosition => transform.position + eyeOffset;

    /// <summary>
    /// EnemyController의 Awake 시점에 호출되어 데이터를 주입받는 초기화 메서드
    /// </summary>
    public void Initialize(EnemyData data)
    {
        enemyData = data;
    }

    /// <summary>
    /// 시야각 및 장애물 레이캐스트를 검사하여 가장 가까운 유효 타겟 탐색
    /// </summary>
    /// <param name="spottedTarget">발견된 타겟의 Transform (미발견 시 null)</param>
    /// <returns>유효 타겟 발견 시 true, 미발견 시 false</returns>
    public bool TryFindTarget(out Transform spottedTarget)
    {
        spottedTarget = null;

        if (enemyData == null)
            return false;

        Vector3 eyePos = EyePosition;

        // 최대 시야 반경 내 타겟 레이어 충돌체 탐색
        int hitCount = Physics.OverlapSphereNonAlloc(
            position: eyePos,
            radius: enemyData.ViewDistance,
            results: hitColliders,
            layerMask: enemyData.TargetLayer
        );

        if (hitCount == 0)
            return false;

        Transform closestTarget = null;
        float closestDistanceSqr = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            Transform candidate = hitColliders[i].transform;
            if (candidate == transform) continue;

            Vector3 directionToTarget = candidate.position - eyePos;
            float distanceSqr = directionToTarget.sqrMagnitude;

            // 수평 시야각(FOV) 검사
            Vector3 flatDirection = new Vector3(directionToTarget.x, 0.0f, directionToTarget.z).normalized;
            float angle = Vector3.Angle(transform.forward, flatDirection);

            // 전방 기준 좌우 절반 각도 안에 포함되는지 검사
            if (angle <= enemyData.ViewAngle * 0.5f)
            {
                float distance = Mathf.Sqrt(distanceSqr);

                // 장애물 차폐 여부 레이캐스트 검사
                bool isBlocked = Physics.Raycast(
                    origin: eyePos,
                    direction: directionToTarget.normalized,
                    maxDistance: distance,
                    layerMask: enemyData.ObstacleLayer
                );

                if (isBlocked)
                {
                    // 벽이나 지형지물에 가려진 경우 탐색 제외
                    continue;
                }

                // 가장 가까운 타겟 우선 선택
                if (distanceSqr < closestDistanceSqr)
                {
                    closestDistanceSqr = distanceSqr;
                    closestTarget = candidate;
                }
            }
        }

        if (closestTarget != null)
        {
            spottedTarget = closestTarget;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 씬 뷰에서 적 본체 선택 시 시야 범위 및 각도 시각화
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 런타임 전 에디터 상태에서도 기즈모를 볼 수 있도록 부모/본체의 EnemyController 탐색
        EnemyData targetData = enemyData;
        if (targetData == null)
        {
            var controller = GetComponent<EnemyController>();
            if (controller != null)
            {
                targetData = controller.Data;
            }
        }

        if (targetData == null) return;

        Vector3 eyePos = EyePosition;

        // 최대 시야 반경 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePos, targetData.ViewDistance);

        // 시야각 부채꼴 가이드라인 (하늘색)
        Vector3 forward = transform.forward;
        Quaternion leftRot = Quaternion.AngleAxis(-targetData.ViewAngle * 0.5f, Vector3.up);
        Quaternion rightRot = Quaternion.AngleAxis(targetData.ViewAngle * 0.5f, Vector3.up);

        Vector3 leftDir = leftRot * forward;
        Vector3 rightDir = rightRot * forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(eyePos, leftDir * targetData.ViewDistance);
        Gizmos.DrawRay(eyePos, rightDir * targetData.ViewDistance);
    }
}