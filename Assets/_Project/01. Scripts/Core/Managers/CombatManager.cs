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
    [Tooltip("공격 사거리 대비 목표 접근 비율 (예: 0.85 = 사거리의 85% 지점까지 파고듦)")]
    [Range(0.6f, 0.95f)]
    [SerializeField] private float _attackRangeRatio = 0.85f;

    [Tooltip("정면 적들 좌우 분산 각도(도)")]
    [SerializeField] private float _frontSpreadAngle = 45.0f;

    [Header("Flank Settings")]
    [Tooltip("매 갱신마다 적이 우회전진할 각도")]
    [SerializeField] private float _orbitStepAngle = 40.0f;

    [Tooltip("최종적으로 파고들 플레이어 등 뒤 각도")]
    [Range(110f, 160f)]
    [SerializeField] private float _targetBackAngle = 135.0f;

    private readonly List<EnemyController> _registeredEnemies = new List<EnemyController>(16);
    private readonly List<EnemyController> _meleeEnemies = new List<EnemyController>(16);
    private readonly List<EnemyController> _rangedEnemies = new List<EnemyController>(8);

    private EnemyController _currentFlanker = null;
    private EnemyController _currentBlocker = null;
    private float _flankerSign = 1.0f;

    private float _timer = 0f;

    // 프로퍼티
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
    public void RegisterEnemy(EnemyController enemy)
    {
        if (enemy == null || _registeredEnemies.Contains(enemy))
            return;

        _registeredEnemies.Add(enemy);
    }

    /// <summary>
    /// 씬 내의 에너미를 전술 조율 대상에서 해제
    /// </summary>
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

            if (_currentBlocker != null && (!availableMelee.Contains(_currentBlocker) || _currentBlocker.Blackboard == null || _currentBlocker.Blackboard.IsDead))
            {
                _currentBlocker = null;
            }

            // 원거리 적과 플레이어 사이에 가장 가까운 근접 적을 바디블로커로 선택
            if (_currentBlocker == null)
            {
                Vector3 playerToRanged = (primaryRanged.transform.position - playerPos).normalized;
                Vector3 blockMidPoint = playerPos + playerToRanged * 2.0f;

                availableMelee.Sort((a, b) =>
                {
                    float distA = (a.transform.position - blockMidPoint).sqrMagnitude;
                    float distB = (b.transform.position - blockMidPoint).sqrMagnitude;
                    return distA.CompareTo(distB);
                });

                _currentBlocker = availableMelee[0];
            }

            // 바디블로커 좌표 계산 및 할당
            CoordinateBodyBlocker(_currentBlocker, playerPos, primaryRanged.transform.position);
            availableMelee.Remove(_currentBlocker);

            // 블로커를 제외하고 남은 근접 적이 2마리 이상일 때 우회(SideAttacker) 할당
            if (availableMelee.Count >= 2)
            {
                AssignFlankerRole(availableMelee, playerPos, playerForward);
            }
            else
            {
                _currentFlanker = null;
            }

            // 남은 근접 적 정면 분산 배치
            AssignRemainingFrontAttackers(availableMelee, playerPos, playerForward);
            return;
        }

        // =========================================================================
        // [전술 2] 원거리 적이 없는 경우
        // =========================================================================
        _currentBlocker = null;

        if (availableMelee.Count >= 3)
        {
            AssignFlankerRole(availableMelee, playerPos, playerForward);
        }
        else
        {
            _currentFlanker = null;
        }

        AssignRemainingFrontAttackers(availableMelee, playerPos, playerForward);
    }

    /// <summary>
    /// 원거리 적과 플레이어 사이 경로를 가로막되, 해당 근접 적의 사거리 비율 지점에 정확히 배치
    /// </summary>
    private void CoordinateBodyBlocker(EnemyController blocker, Vector3 playerPos, Vector3 rangedPos)
    {
        EnemyBlackboard bb = blocker.Blackboard;
        if (bb == null) return;

        bb.Role = CombatRole.BodyBlocker;

        float attackRange = bb.EnemyData != null ? bb.EnemyData.AttackRange : 2.0f;
        float targetDist = attackRange * _attackRangeRatio;

        Vector3 toRanged = (rangedPos - playerPos).normalized;
        toRanged.y = 0f;

        Vector3 blockTargetPos = playerPos + (toRanged * targetDist);
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

        // 가장 먼 적을 우회 공격자로 선택
        if (_currentFlanker == null)
        {
            pool.Sort((a, b) =>
            {
                float distA = (a.transform.position - playerPos).sqrMagnitude;
                float distB = (b.transform.position - playerPos).sqrMagnitude;
                return distA.CompareTo(distB);
            });

            _currentFlanker = pool[pool.Count - 1];

            // 플레이어 정면 기준 좌우 어느 쪽이 더 가까운지 판단
            Vector3 toEnemy = _currentFlanker.transform.position - playerPos;
            toEnemy.y = 0.0f;
            float initAngle = Vector3.SignedAngle(playerForward, toEnemy.normalized, Vector3.up);
            _flankerSign = (initAngle >= 0.0f) ? 1.0f : -1.0f;
        }

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
    /// 플레이어 주변을 우회하며 해당 적의 사거리 비율 지점으로 파고듦
    /// </summary>
    private void CoordinateCompassFlanker(EnemyController flanker, Vector3 playerPos, Vector3 playerForward)
    {
        EnemyBlackboard bb = flanker.Blackboard;
        if (bb == null) return;

        float attackRange = bb.EnemyData != null ? bb.EnemyData.AttackRange : 2.0f;
        float targetDist = attackRange * _attackRangeRatio;

        Vector3 enemyPos = flanker.transform.position;
        Vector3 playerToEnemy = enemyPos - playerPos;
        playerToEnemy.y = 0.0f;
        float currentDist = playerToEnemy.magnitude;

        // 현재 각도 계산
        float currentAngle = Vector3.SignedAngle(playerForward, playerToEnemy.normalized, Vector3.up);
        bool reachedBackAngle = Mathf.Abs(currentAngle) >= 115.0f; // 등 뒤 각도 도달 여부

        bb.Role = CombatRole.SideAttacker;

        // 등 뒤(115도 이상)에 도달했고 + 사거리 안쪽까지 좁혀졌을 때만 우회 완료 및 공격 전환
        if (reachedBackAngle && currentDist <= targetDist)
        {
            bb.ClearAssignedTactics();
            return;
        }

        Vector3 targetArcPos;
        if (reachedBackAngle)
        {
            // 등 뒤에 도달했으면 최종 타격 지점(등 뒤 사거리 비율 지점)으로 돌진
            Vector3 finalBackDir = Quaternion.Euler(0.0f, _flankerSign * _targetBackAngle, 0.0f) * playerForward;
            targetArcPos = playerPos + (finalBackDir.normalized * targetDist);
        }
        else
        {
            // 아직 등 뒤가 아니면 사거리보다 살짝 바깥 궤도(attackRange + 0.3f)를 타고 플레이어 시야 밖으로 크게 돌아감
            float orbitRadius = attackRange + 0.3f;
            Vector3 currentRadialDir = playerToEnemy.normalized;
            if (currentRadialDir == Vector3.zero) currentRadialDir = -playerForward;

            Vector3 nextArcDir = Quaternion.Euler(0.0f, _flankerSign * _orbitStepAngle, 0.0f) * currentRadialDir;
            targetArcPos = playerPos + (nextArcDir.normalized * orbitRadius);
        }

        bb.SetAssignedPosition(SampleNavMeshPosition(targetArcPos));
    }

    /// <summary>
    /// 플레이어 정면 좌우 분산 각도에 맞춰 해당 적의 사거리 비율 위치 할당
    /// </summary>
    private void AssignFrontSlot(EnemyController enemy, Vector3 playerPos, Vector3 playerForward, float angleOffset)
    {
        EnemyBlackboard bb = enemy.Blackboard;
        if (bb == null) return;

        bb.Role = CombatRole.FrontAttacker;

        float attackRange = bb.EnemyData != null ? bb.EnemyData.AttackRange : 2.0f;
        float targetSlotDist = attackRange * _attackRangeRatio;

        Vector3 slotDir = Quaternion.Euler(0.0f, angleOffset, 0.0f) * playerForward;
        Vector3 slotPos = playerPos + (slotDir.normalized * targetSlotDist);

        bb.SetAssignedPosition(SampleNavMeshPosition(slotPos));
    }

    /// <summary>
    /// NavMesh 상에서 유효한 좌표를 샘플링하여 반환
    /// </summary>
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