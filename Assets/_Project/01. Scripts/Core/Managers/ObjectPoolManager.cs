using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// 유니티 빌트인 ObjectPool을 기반으로, 오브젝트별 서브 그룹을 형성해 관리하는 범용 풀 매니저
/// </summary>
public class ObjectPoolManager : LocalSingleton<ObjectPoolManager>
{
    /// <summary>
    /// 개별 소유자/프리팹 단위 서브 풀 래퍼
    /// </summary>
    private class SubPoolWrapper
    {
        private readonly IObjectPool<GameObject> pool;
        private readonly GameObject prefab;
        private readonly Transform groupParent;

        // 프로퍼티
        public Transform GroupParent => groupParent;
        public GameObject Get() => pool.Get();
        public void Release(GameObject obj) => pool.Release(obj);

        public SubPoolWrapper(GameObject sourcePrefab, Transform parent, int defaultCapacity, int maxSize)
        {
            prefab = sourcePrefab;
            groupParent = parent;

            // 유니티 빌트인 ObjectPool 인스턴스화
            pool = new ObjectPool<GameObject>(
                createFunc: OnCreatePoolItem,
                actionOnGet: OnGetFromPool,
                actionOnRelease: OnReleaseToPool,
                actionOnDestroy: OnDestroyPoolItem,
                collectionCheck: true,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        /// <summary>
        /// 풀에 새 오브젝트를 생성할 때 호출되는 콜백
        /// </summary>
        /// <returns></returns>
        private GameObject OnCreatePoolItem()
        {
            GameObject obj = Instantiate(prefab, groupParent);
            obj.SetActive(false);
            return obj;
        }

        /// <summary>
        /// 풀에서 오브젝트를 꺼낼 때 호출되는 콜백
        /// </summary>
        /// <param name="obj"></param>
        private void OnGetFromPool(GameObject obj)
        {
            // 꺼낼 때는 아직 위치/방향 셋업 전이므로 비활성화 상태 유지
        }

        /// <summary>
        /// 풀에 오브젝트를 반환할 때 호출되는 콜백
        /// </summary>
        /// <param name="obj"></param>
        private void OnReleaseToPool(GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetParent(groupParent);
        }

        /// <summary>
        /// 풀에서 오브젝트를 제거할 때 호출되는 콜백
        /// </summary>
        /// <param name="obj">제거될 오브젝트</param>
        private void OnDestroyPoolItem(GameObject obj)
        {
            Destroy(obj);
        }
    }

    // Key - Value 매핑
    // Key: 소유자 GameObject의 InstanceID, Value: 해당 소유자의 서브 풀 래퍼
    private readonly Dictionary<int, SubPoolWrapper> poolMap = new Dictionary<int, SubPoolWrapper>();

    /// <summary>
    /// 객체(에너미/보스 등)가 Start() 시점에 자신만의 풀을 등록
    /// </summary>
    /// <param name="owner">풀을 사용할 GameObject</param>
    /// <param name="prefab">풀링할 대상 프리팹</param>
    /// <param name="defaultCapacity">기본 생성 예약 개수</param>
    /// <param name="maxSize">최대 수용 한계치</param>
    public void RegisterPool(GameObject owner, GameObject prefab, int defaultCapacity = 3, int maxSize = 10)
    {
        if (owner == null || prefab == null) return;

        int ownerId = owner.GetInstanceID();
        if (poolMap.ContainsKey(ownerId)) return;

        // 하이어라키 정리용 빈 자식 오브젝트 생성
        GameObject groupObj = new GameObject($"{owner.name}_{ownerId}");
        groupObj.transform.SetParent(transform);

        SubPoolWrapper wrapper = new SubPoolWrapper(prefab, groupObj.transform, defaultCapacity, maxSize);
        poolMap.Add(ownerId, wrapper);

        // 초기 수량만큼 미리 메모리에 인출해 생성해 둔 뒤 즉시 반환
        List<GameObject> warmUpList = new List<GameObject>(defaultCapacity);
        for (int i = 0; i < defaultCapacity; i++)
        {
            warmUpList.Add(wrapper.Get());
        }
        for (int i = 0; i < warmUpList.Count; i++)
        {
            wrapper.Release(warmUpList[i]);
        }
    }

    /// <summary>
    /// 지정된 소유자의 풀에서 오브젝트 1개 인출
    /// </summary>
    public GameObject Spawn(GameObject owner)
    {
        if (owner == null) return null;

        int ownerId = owner.GetInstanceID();
        if (poolMap.TryGetValue(ownerId, out var wrapper))
        {
            return wrapper.Get();
        }

        Debug.LogWarning($"[ObjectPoolManager] {owner.name}에 등록된 풀을 찾을 수 없습니다.");
        return null;
    }

    /// <summary>
    /// 특정 컴포넌트 타입으로 반환
    /// </summary>
    public T Spawn<T>(GameObject owner) where T : Component
    {
        GameObject obj = Spawn(owner);
        if (obj == null) return null;

        return obj.GetComponent<T>();
    }

    /// <summary>
    /// 사용이 끝난 오브젝트를 풀로 반환
    /// </summary>
    public void Despawn(GameObject obj, GameObject owner)
    {
        if (obj == null) return;

        if (owner != null && poolMap.TryGetValue(owner.GetInstanceID(), out var wrapper))
        {
            wrapper.Release(obj);
            return;
        }

        // 소유주 정보가 없는 경우 파괴
        Destroy(obj);
    }
}