using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 현재 씬 내의 교전 상황을 관리하는 로컬 매니저 (전술 조율자) <br/>
/// 전술 1: 원거리 1 + 바디블로커 1 매칭 기반 협공 및 포위 <br/>
/// 전술 2: 순수 근접 3마리 이상 시 우회 1마리 + 정면 포위
/// </summary>
public class CombatManager : LocalSingleton<CombatManager>
{
    [Header("Target Reference")]
    [Tooltip("플레이어 Transform")]
    [SerializeField] private Transform _playerTransform;

    [Header("Tactic Timing Settings")]
    [Tooltip("전술 대형 및 좌표를 계산하는 주기(초)")]
    [SerializeField] private float _tacticUpdateInterval = 0.2f;

    [Header("Distance & Slot Settings")]
    [Tooltip("공격 사거리 기준 진입 거리")]
    [SerializeField] private float _rangeOffset = 0.35f;

    [Tooltip("정면 적들 좌우 분산 각도(도)")]
    [SerializeField] private float _frontSpreadAngle = 45.0f;

    [Header("Flank Settings")]
    [Tooltip("우회 시 유지할 선회 반경")]
    [SerializeField] private float _orbitMargin = 0.5f;

    [Tooltip("매 갱신마다 적이 우회전진할 각도")]
    [SerializeField] private float _orbitStepAngle = 40.0f;

    [Tooltip("최종적으로 파고들 플레이어 등 뒤 각도")]
    [Range(110f, 160f)]
    [SerializeField] private float _targetBackAngle = 135f;

    [Header("BodyBlock Settings")]
    [Tooltip("원거리 적과 플레이어 사이에서 길을 막을 지점 비율")]
    [Range(0.2f, 0.8f)]
    [SerializeField] private float _blockRatio = 0.45f;

    private readonly List<EnemyController> _registeredEnemies = new List<EnemyController>(16);
    private readonly List<EnemyController> _meleeEnemies = new List<EnemyController>(16);
    private readonly List<EnemyController> _rangedEnemies = new List<EnemyController>(8);

    private EnemyController _currentFlanker = null;
    private EnemyController _currentBlocker = null;
    private float _flankerSign = 1.0f;

    private float _timer = 0f;

    public Transform PlayerTransform => _playerTransform;

    protected override void Awake()
    {
        base.Awake();

        if (_playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                _playerTransform = playerObj.transform;
            }
        }
    }

    private void Update()
    {
        CleanUpEnemies();

        if (_registeredEnemies.Count == 0)
            return;

        if (_playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                _playerTransform = playerObj.transform;
            }
            else
            {
                return;
            }
        }

        _timer += Time.deltaTime;
        if (_timer >= _tacticUpdateInterval)
        {
            _timer = 0f;
            CoordinateTactics();
        }
    }

    /// <summary>
    /// 씬 내의 에너미를 전술 조율 대상에 등록
    /// </summary>
    /// <param name="enemy">등록할 적</param>
    public void RegisterEnemy(EnemyController enemy)
    {
        if (enemy == null || _registeredEnemies.Contains(enemy))
            return;

        _registeredEnemies.Add(enemy);
    }

    /// <summary>
    /// 씬 내의 에너미를 전술 조율 대상에서 해제
    /// </summary>
    /// <param name="enemy">등록 해제할 적</param>
    public void UnregisterEnemy(EnemyController enemy)
    {
        if (enemy == null)
            return;

        if (_currentFlanker == enemy) _currentFlanker = null;
        if (_currentBlocker == enemy) _currentBlocker = null;

        if (_registeredEnemies.Remove(enemy))
        {
            if (enemy.Blackboard != null)
            {
                enemy.Blackboard.ClearAssignedTactics();
            }
        }
    }

    /// <summary>
    /// 씬 내 등록된 적들을 플레이어 기준으로 전술 분류 및 좌표 할당
    /// </summary>
    private void CoordinateTactics()
    {
        _meleeEnemies.Clear();
        _rangedEnemies.Clear();

        Vector3 playerPos = _playerTransform.position;
        Vector3 playerForward = _playerTransform.forward;

        // 등록된 적 타입별 분류
        for (int i = 0; i < _registeredEnemies.Count; i++)
        {
            EnemyController enemy = _registeredEnemies[i];
            if (enemy == null || enemy.EnemyData == null) continue;

            if (enemy.EnemyData.CombatType == EnemyCombatType.Ranged)
            {
                _rangedEnemies.Add(enemy);
            }
            else
            {
                _meleeEnemies.Add(enemy);
            }
        }

        // 근접 적이 없으면 전술 분기 종료
        if (_meleeEnemies.Count == 0)
        {
            _currentBlocker = null;
            _currentFlanker = null;
            return;
        }

        // 가용 근접 적 풀 복사
        List<EnemyController> availableMelee = new List<EnemyController>(_meleeEnemies);

        // =========================================================================
        // [전술 1] 원거리 적이 존재하는 경우
        // =========================================================================
        if (_rangedEnemies.Count > 0)
        {
            EnemyController primaryRanged = _rangedEnemies[0];

            // 바디블로커가 유효한지 확인, 유효하지 않으면 null 처리
            if (_currentBlocker != null && (!availableMelee.Contains(_currentBlocker) || _currentBlocker.Blackboard == null || _currentBlocker.Blackboard.IsDead))
            {
                _currentBlocker = null;
            }
            // 바디블로커가 없거나 유효하지 않은 경우, 가장 가까운 근접 적을 선택
            if (_currentBlocker == null)
            {
                // 원거리 적과 플레이어 사이의 중간 지점 계산
                Vector3 blockMidPoint = Vector3.Lerp(playerPos, primaryRanged.transform.position, _blockRatio);
                availableMelee.Sort((a, b) =>
                {
                    float distA = (a.transform.position - blockMidPoint).sqrMagnitude;
                    float distB = (b.transform.position - blockMidPoint).sqrMagnitude;
                    return distA.CompareTo(distB);
                });
                // 가장 가까운 근접 적을 바디블로커로 선택
                _currentBlocker = availableMelee[0];
            }
            // 바디블로커 좌표 계산 및 할당
            CoordinateBodyBlocker(_currentBlocker, playerPos, primaryRanged.transform.position);
            availableMelee.Remove(_currentBlocker);

            // 블로커를 제외하고 남은 근접 적이 2마리 이상일 때만 우회(SideAttacker) 허용
            if (availableMelee.Count >= 2)
            {
                AssignFlankerRole(availableMelee, playerPos, playerForward);
            }
            else
            {
                _currentFlanker = null;
            }

            // 남은 근접 적은 정면 배치
            AssignRemainingFrontAttackers(availableMelee, playerPos, playerForward);
            return;
        }

        // =========================================================================
        // [전술 2] 원거리 적이 없는 경우
        // =========================================================================
        _currentBlocker = null;

        // 근접 적이 3마리 이상일 때만 1마리 우회 기습
        if (availableMelee.Count >= 3)
        {
            AssignFlankerRole(availableMelee, playerPos, playerForward);
        }
        else
        {
            _currentFlanker = null;
        }

        // 남은 적 정면 배치
        AssignRemainingFrontAttackers(availableMelee, playerPos, playerForward);
    }

    /// <summary>
    /// 원거리 적과 플레이어 사이 경로를 가로막는 위치 좌표를 계산하여 할당
    /// </summary>
    private void CoordinateBodyBlocker(EnemyController blocker, Vector3 playerPos, Vector3 rangedPos)
    {
        EnemyBlackboard bb = blocker.Blackboard;
        if (bb == null) return;

        bb.Role = CombatRole.BodyBlocker;

        Vector3 blockTargetPos = Vector3.Lerp(playerPos, rangedPos, _blockRatio);
        blockTargetPos.y = playerPos.y;

        bb.SetAssignedPosition(SampleNavMeshPosition(blockTargetPos));
    }

    /// <summary>
    /// 가용 근접 풀에서 플레이어 기준 가장 먼 적을 우회 공격자로 선택
    /// </summary>
    private void AssignFlankerRole(List<EnemyController> pool, Vector3 playerPos, Vector3 playerForward)
    {
        if (_currentFlanker != null && (!pool.Contains(_currentFlanker) || _currentFlanker.Blackboard == null || _currentFlanker.Blackboard.IsDead))
        {
            _currentFlanker = null;
        }

        // 우회 공격자가 없으면 플레이어 기준 가장 먼 적을 선택
        if (_currentFlanker == null)
        {
            // 플레이어 기준 가장 먼 적을 선택
            pool.Sort((a, b) =>
            {
                float distA = (a.transform.position - playerPos).sqrMagnitude;
                float distB = (b.transform.position - playerPos).sqrMagnitude;
                return distA.CompareTo(distB);
            });
            
            _currentFlanker = pool[pool.Count - 1];

            // 플레이어 기준 우회 방향 결정
            Vector3 toEnemy = _currentFlanker.transform.position - playerPos;
            toEnemy.y = 0.0f;
            float initAngle = Vector3.SignedAngle(playerForward, toEnemy.normalized, Vector3.up);
            _flankerSign = (initAngle >= 0.0f) ? 1.0f : -1.0f;
        }

        // 우회 공격자 좌표 계산 및 할당
        CoordinateCompassFlanker(_currentFlanker, playerPos, playerForward);
        pool.Remove(_currentFlanker);
    }

    /// <summary>
    /// 남은 모든 근접 적을 플레이어 정면 각도에 분산 배치
    /// </summary>
    private void AssignRemainingFrontAttackers(List<EnemyController> pool, Vector3 playerPos, Vector3 playerForward)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            float angle = (pool.Count == 1) ? 0f : ((i % 2 == 0) ? -_frontSpreadAngle : _frontSpreadAngle);
            AssignFrontSlot(pool[i], playerPos, playerForward, angle);
        }
    }

    /// <summary>
    /// 플레이어를 중심으로 우회 공격자 좌표를 계산하여 할당
    /// </summary>
    /// <param name="flanker">우회 공격자</param>
    /// <param name="playerPos">플레이어 위치</param>
    /// <param name="playerForward">플레이어 전방 방향</param>
    private void CoordinateCompassFlanker(EnemyController flanker, Vector3 playerPos, Vector3 playerForward)
    {
        EnemyBlackboard bb = flanker.Blackboard;
        if (bb == null) return;

        // 우회 공격자가 플레이어 측후방에 도달했는지 확인
        float attackRange = bb.EnemyData != null ? bb.EnemyData.AttackRange : 2.0f;
        Vector3 enemyPos = flanker.transform.position;

        // 플레이어와 우회 공격자 사이의 수평 거리 계산
        Vector3 playerToEnemy = enemyPos - playerPos;
        playerToEnemy.y = 0.0f;
        float currentDist = playerToEnemy.magnitude;

        // 플레이어 기준 우회 공격자의 현재 각도 계산
        if (currentDist <= (attackRange - 0.15f))
        {
            bb.Role = CombatRole.SideAttacker;
            bb.ClearAssignedTactics();
            return;
        }

        bb.Role = CombatRole.SideAttacker;

        // 플레이어 기준 우회 공격자의 현재 각도 계산
        float currentAngle = Vector3.SignedAngle(playerForward, playerToEnemy.normalized, Vector3.up);
        bool reachedBackAngle = Mathf.Abs(currentAngle) >= 115.0f;

        Vector3 targetArcPos;
        // 플레이어 기준 우회 공격자가 목표 후방 각도에 도달했으면 최종 후방 좌표 계산
        if (reachedBackAngle)
        {
            Vector3 finalBackDir = Quaternion.Euler(0.0f, _flankerSign * _targetBackAngle, 0.0f) * playerForward;
            float strikeDist = Mathf.Max(0.5f, attackRange - _rangeOffset);
            targetArcPos = playerPos + (finalBackDir.normalized * strikeDist);
        }
        // 플레이어 기준 우회 공격자가 목표 후방 각도에 도달하지 않았으면 선회 좌표 계산
        else
        {
            float compassRadius = attackRange + _orbitMargin;
            Vector3 currentRadialDir = playerToEnemy.normalized;
            if (currentRadialDir == Vector3.zero) currentRadialDir = -playerForward;

            Vector3 nextArcDir = Quaternion.Euler(0.0f, _flankerSign * _orbitStepAngle, 0.0f) * currentRadialDir;
            targetArcPos = playerPos + (nextArcDir.normalized * compassRadius);
        }

        // 우회 공격자 좌표 할당
        bb.SetAssignedPosition(SampleNavMeshPosition(targetArcPos));
    }

    /// <summary>
    /// 플레이어 정면 좌우 분산 각도에 따라 근접 적 좌표를 계산하여 할당
    /// </summary>
    /// <param name="enemy">적</param>
    /// <param name="playerPos">플레이어 위치</param>
    /// <param name="playerForward">플레이어 전방 벡터</param>
    /// <param name="angleOffset">각도 오프셋</param>
    private void AssignFrontSlot(EnemyController enemy, Vector3 playerPos, Vector3 playerForward, float angleOffset)
    {
        EnemyBlackboard bb = enemy.Blackboard;
        if (bb == null) return;

        bb.Role = CombatRole.FrontAttacker;

        // 공격 사거리 기준 진입 거리 계산
        float attackRange = bb.EnemyData != null ? bb.EnemyData.AttackRange : 2.0f;
        float targetSlotDist = Mathf.Max(0.5f, attackRange - _rangeOffset);
        // 플레이어 기준 좌우 분산 각도에 따라 좌표 계산
        Vector3 slotDir = Quaternion.Euler(0.0f, angleOffset, 0.0f) * playerForward;
        Vector3 slotPos = playerPos + (slotDir.normalized * targetSlotDist);

        // 플레이어 기준 좌우 분산 각도에 따라 근접 적 좌표 할당
        bb.SetAssignedPosition(SampleNavMeshPosition(slotPos));
    }

    /// <summary>
    /// NavMesh 상에서 유효한 좌표를 샘플링하여 반환
    /// </summary>
    /// <param name="center">중앙 좌표</param>
    /// <returns></returns>
    private Vector3 SampleNavMeshPosition(Vector3 center)
    {
        if (NavMesh.SamplePosition(center, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return center;
    }

    /// <summary>
    /// 씬 내 등록된 적들 중 null, 비활성화, 사망한 적들을 제거
    /// </summary>
    private void CleanUpEnemies()
    {
        for (int i = _registeredEnemies.Count - 1; i >= 0; i--)
        {
            EnemyController enemy = _registeredEnemies[i];
            if (enemy == null || !enemy.gameObject.activeInHierarchy || (enemy.Blackboard != null && enemy.Blackboard.IsDead))
            {
                if (_currentFlanker == enemy) _currentFlanker = null;
                if (_currentBlocker == enemy) _currentBlocker = null;
                _registeredEnemies.RemoveAt(i);
            }
        }
    }
}