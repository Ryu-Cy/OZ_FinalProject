using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 현재 씬 내의 교전 상황을 관리하는 로컬 매니저 (전술) <br/>
/// 1번: 1마리 우회와 정면 2마리 포위
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
    [Tooltip("공격 사거리 기준 진입 거리 (사거리 경계 멈춤 방지용)")]
    [SerializeField] private float _rangeOffset = 0.35f;

    [Tooltip("정면 적들 좌우 분산 각도(도 단위)")]
    [SerializeField] private float _frontSpreadAngle = 45.0f;

    [Header("Flank Settings")]
    [Tooltip("우회 시 유지할 안전 선회 반경 오프셋")]
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

    public void RegisterEnemy(EnemyController enemy)
    {
        if (enemy == null || _registeredEnemies.Contains(enemy))
            return;

        _registeredEnemies.Add(enemy);
    }

    public void UnregisterEnemy(EnemyController enemy)
    {
        if (enemy == null)
            return;

        if (_currentFlanker == enemy)
        {
            _currentFlanker = null;
        }

        if (_registeredEnemies.Remove(enemy))
        {
            if (enemy.Blackboard != null)
            {
                enemy.Blackboard.ClearAssignedTactics();
            }
        }
    }

    private void CoordinateTactics()
    {
        _meleeEnemies.Clear();
        _rangedEnemies.Clear();

        Vector3 playerPos = _playerTransform.position;
        Vector3 playerForward = _playerTransform.forward;

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

        // =========================================================================
        // 근거리 적 3개체 이상
        // =========================================================================
        if (_meleeEnemies.Count >= 3)
        {
            if (_currentFlanker != null && (!_meleeEnemies.Contains(_currentFlanker) || _currentFlanker.Blackboard == null || _currentFlanker.Blackboard.IsDead))
            {
                _currentFlanker = null;
            }

            if (_currentFlanker == null)
            {
                _meleeEnemies.Sort((a, b) =>
                {
                    float distA = (a.transform.position - playerPos).sqrMagnitude;
                    float distB = (b.transform.position - playerPos).sqrMagnitude;
                    return distA.CompareTo(distB);
                });

                _currentFlanker = _meleeEnemies[_meleeEnemies.Count - 1];

                Vector3 toEnemy = _currentFlanker.transform.position - playerPos;
                toEnemy.y = 0f;
                float initAngle = Vector3.SignedAngle(playerForward, toEnemy.normalized, Vector3.up);
                _flankerSign = (initAngle >= 0f) ? 1.0f : -1.0f;
            }

            List<EnemyController> frontAttackers = new List<EnemyController>(4);
            for (int i = 0; i < _meleeEnemies.Count; i++)
            {
                if (_meleeEnemies[i] != _currentFlanker)
                {
                    frontAttackers.Add(_meleeEnemies[i]);
                }
            }

            // 정면 1번(-45도), 2번(+45도) 슬롯
            if (frontAttackers.Count > 0) AssignFrontSlot(frontAttackers[0], playerPos, playerForward, -_frontSpreadAngle);
            if (frontAttackers.Count > 1) AssignFrontSlot(frontAttackers[1], playerPos, playerForward, _frontSpreadAngle);

            for (int i = 2; i < frontAttackers.Count; i++)
            {
                EnemyBlackboard bb = frontAttackers[i].Blackboard;
                if (bb != null)
                {
                    bb.Role = CombatRole.FrontAttacker;
                    bb.ClearAssignedTactics();
                }
            }

            CoordinateCompassFlanker(_currentFlanker, playerPos, playerForward);
            return;
        }

        // =========================================================================
        // 1~2개체일 때
        // =========================================================================
        _currentFlanker = null;
        for (int i = 0; i < _meleeEnemies.Count; i++)
        {
            float angle = (i % 2 == 0) ? -_frontSpreadAngle * 0.5f : _frontSpreadAngle * 0.5f;
            AssignFrontSlot(_meleeEnemies[i], playerPos, playerForward, angle);
        }
    }

    private void CoordinateCompassFlanker(EnemyController flanker, Vector3 playerPos, Vector3 playerForward)
    {
        EnemyBlackboard bb = flanker.Blackboard;
        if (bb == null) return;

        float attackRange = bb.EnemyData != null ? bb.EnemyData.AttackRange : 2.0f;
        Vector3 enemyPos = flanker.transform.position;

        Vector3 playerToEnemy = enemyPos - playerPos;
        playerToEnemy.y = 0f;
        float currentDist = playerToEnemy.magnitude;

        // 사거리 안쪽 진입 시 공격에 집중
        if (currentDist <= (attackRange - 0.15f))
        {
            bb.Role = CombatRole.SideAttacker;
            bb.ClearAssignedTactics();
            return;
        }

        bb.Role = CombatRole.SideAttacker;

        float currentAngle = Vector3.SignedAngle(playerForward, playerToEnemy.normalized, Vector3.up);
        bool reachedBackAngle = Mathf.Abs(currentAngle) >= 115f;

        Vector3 targetArcPos;

        if (reachedBackAngle)
        {
            Vector3 finalBackDir = Quaternion.Euler(0f, _flankerSign * _targetBackAngle, 0f) * playerForward;
            float strikeDist = Mathf.Max(0.5f, attackRange - _rangeOffset);
            targetArcPos = playerPos + (finalBackDir.normalized * strikeDist);
        }
        else
        {
            float compassRadius = attackRange + _orbitMargin;
            Vector3 currentRadialDir = playerToEnemy.normalized;
            if (currentRadialDir == Vector3.zero) currentRadialDir = -playerForward;

            Vector3 nextArcDir = Quaternion.Euler(0f, _flankerSign * _orbitStepAngle, 0f) * currentRadialDir;
            targetArcPos = playerPos + (nextArcDir.normalized * compassRadius);
        }

        bb.SetAssignedPosition(SampleNavMeshPosition(targetArcPos));
    }

    private void AssignFrontSlot(EnemyController enemy, Vector3 playerPos, Vector3 playerForward, float angleOffset)
    {
        EnemyBlackboard bb = enemy.Blackboard;
        if (bb == null) return;

        bb.Role = CombatRole.FrontAttacker;

        float attackRange = bb.EnemyData != null ? bb.EnemyData.AttackRange : 2.0f;
        // 정면 적들도 (사거리 - 0.35m) 안쪽으로 확실히 파고들게 설정
        float targetSlotDist = Mathf.Max(0.5f, attackRange - _rangeOffset);

        Vector3 slotDir = Quaternion.Euler(0f, angleOffset, 0f) * playerForward;
        Vector3 slotPos = playerPos + (slotDir.normalized * targetSlotDist);

        bb.SetAssignedPosition(SampleNavMeshPosition(slotPos));
    }

    private Vector3 SampleNavMeshPosition(Vector3 center)
    {
        if (NavMesh.SamplePosition(center, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        return center;
    }

    private void CleanUpEnemies()
    {
        for (int i = _registeredEnemies.Count - 1; i >= 0; i--)
        {
            EnemyController enemy = _registeredEnemies[i];
            if (enemy == null || !enemy.gameObject.activeInHierarchy || (enemy.Blackboard != null && enemy.Blackboard.IsDead))
            {
                if (_currentFlanker == enemy) _currentFlanker = null;
                _registeredEnemies.RemoveAt(i);
            }
        }
    }
}