using UnityEngine;

/// <summary>
/// 발사 시 일직선으로 날아가며, 지정된 충돌 레이어와 부딪히거나 수명 만료 시 ObjectPoolManager로 반환되는 탄환
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class EnemyBullet : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("탄환 비행 속도")]
    [SerializeField] private float speed = 15.0f;

    [Tooltip("최대 생존 시간(초)")]
    [SerializeField] private float lifeTime = 5.0f;

    [Header("Collision Filter")]
    [Tooltip("충돌을 감지할 대상 레이어")]
    [SerializeField] private LayerMask collisionLayer;

    private Rigidbody rb;
    private Collider bulletCollider;

    private GameObject owner;
    private float damage;
    private float timer = 0.0f;
    private bool isLaunched = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        bulletCollider = GetComponent<Collider>();

        rb.useGravity = false;
        rb.isKinematic = true;
        bulletCollider.isTrigger = true;
    }

    /// <summary>
    /// 탄환 발사 설정 및 활성화
    /// </summary>
    public void Fire(Vector3 spawnPosition, Vector3 direction, float damageAmount, GameObject ownerObject)
    {
        transform.position = spawnPosition;
        transform.forward = direction.normalized;

        owner = ownerObject;
        damage = damageAmount;
        timer = 0.0f;
        isLaunched = true;

        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!isLaunched) return;

        // 전방을 향해 직선 이동
        transform.position += transform.forward * (speed * Time.deltaTime);

        // 수명 검사
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

        // 지정된 충돌 레이어에 포함되지 않은 대상은 무시
        if (((1 << other.gameObject.layer) & collisionLayer.value) == 0)
        {
            return;
        }

        // IDamageable 인터페이스 대상 대미지 판정
        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            DamageInfo damageInfo = new DamageInfo(
                damage,
                hitPoint,
                -transform.forward,
                transform.forward,
                owner
            );

            damageable.TakeDamage(damageInfo);
        }

        // 충돌 대상에 닿았으므로 회수
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