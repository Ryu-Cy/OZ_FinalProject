using UnityEngine;

/// <summary>
/// 로컬 매니저용 제네릭 싱글톤 추상 클래스. <br/>
/// 씬 전환 시 씬과 함께 파괴된다.
/// </summary>
/// <typeparam name="T">로컬 싱글톤을 상속받을 컴포넌트 타입</typeparam>
public abstract class LocalSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    /// <summary>
    /// 현재 씬 내에서 접근 가능한 정적 인스턴스 프로퍼티
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                // 현재 활성화된 씬 내부에서 해당 타입 탐색
                _instance = FindFirstObjectByType<T>();

                // 씬에 컴포넌트가 없을 경우 자동 생성
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    _instance = singletonObject.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// 현재 씬에 유효한 인스턴스가 존재하는지 안전하게 체크하는 프로퍼티
    /// </summary>
    public static bool HasInstance => _instance != null;

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else if (_instance != this)
        {
            Debug.LogWarning($"[LocalSingleton] {typeof(T).Name} 인스턴스가 씬에 이미 존재하여 중복 오브젝트를 파괴합니다: {gameObject.name}");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 씬이 언로드되어 오브젝트가 파괴될 때 인스턴스 캐시를 깔끔하게 해제
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}