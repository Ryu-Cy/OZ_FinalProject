using UnityEngine;

/// <summary>
/// 발사 시 일직선으로 날아가며, 지정된 충돌 레이어와 부딪히거나 수명 만료 시 ObjectPoolManager로 반환되는 탄환
/// 자식 오브젝트에 Collider와 Rigidbody가 있어도 자동으로 감지하여 초기화합니다.
/// </summary>
public class EnemyBullet : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("탄환 비행 속도")]
    [SerializeField] private float speed = 15.0f;

    [Tooltip("최대 생존 시간(초)")]
    [SerializeField] private float lifeTime = 5.0f;

    [Header("Collision Filter")]
    [Tooltip("충돌을 감지할 대상 레이어 (Player, Obstacle 등)")]
    [SerializeField] private LayerMask collisionLayer;

    private Rigidbody rb;
    private Collider bulletCollider;

    private GameObject owner;
    private float damage;
    private float timer = 0.0f;
    private bool isLaunched = false;

    private void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>();
        bulletCollider = GetComponentInChildren<Collider>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (bulletCollider != null)
        {
            bulletCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// 탄환 발사 설정 및 활성화
    /// </summary>
    public void Fire(Vector3 spawnPosition, Vector3 direction, float damageAmount, GameObject ownerObject)
    {
        transform.position = spawnPosition;
        if (direction != Vector3.zero)
        {
            transform.forward = direction.normalized;
        }

        owner = ownerObject;
        damage = damageAmount;
        timer = 0.0f;
        isLaunched = true;

        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isLaunched) return;

        float moveStep = speed * Time.deltaTime;

        // 고속 비행 시 충돌 누락(터널링) 방지 레이캐스트
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, moveStep, collisionLayer))
        {
            // 발사자 본인 및 자식 제외
            if (owner == null || (!hit.collider.gameObject.Equals(owner) && !hit.transform.IsChildOf(owner.transform)))
            {
                ProcessHit(hit.collider, hit.point);
                return;
            }
        }

        // 전방 직선 이동
        transform.position += transform.forward * moveStep;

        // 수명 만료 검사
        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            Despawn();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isLaunched) return;

        // 발사한 본인이나 자식 콜라이더 무시
        if (owner != null && (other.gameObject == owner || other.transform.IsChildOf(owner.transform)))
        {
            return;
        }

        // 충돌 대상 레이어 필터링
        if (((1 << other.gameObject.layer) & collisionLayer.value) == 0)
        {
            return;
        }

        Vector3 hitPoint = other.ClosestPoint(transform.position);
        ProcessHit(other, hitPoint);
    }

    /// <summary>
    /// 충돌 처리 및 대미지 전달
    /// </summary>
    /// <param name="other">충돌한 콜라이더</param>
    /// <param name="hitPoint">충돌 지점</param>
    private void ProcessHit(Collider other, Vector3 hitPoint)
    {
        if (!isLaunched) return;

        // 대미지 전달
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            DamageInfo damageInfo = new DamageInfo(
                damage,
                hitPoint,
                -transform.forward,
                transform.forward,
                owner
            );

            damageable.TakeDamage(damageInfo);
        }

        Despawn();
    }

    /// <summary>
    /// 비활성화 및 소유자 전용 풀로 반환
    /// </summary>
    private void Despawn()
    {
        isLaunched = false;

        if (ObjectPoolManager.HasInstance)
        {
            ObjectPoolManager.Instance.Despawn(gameObject, owner);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}