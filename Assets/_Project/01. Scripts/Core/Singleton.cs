using UnityEngine;

/// <summary>
/// 범용 제네릭 싱글톤 추상 클래스. <br/>
/// 파괴되지 않고 씬 전역에서 단일 인스턴스를 유지한다.
/// </summary>
/// <typeparam name="T">싱글톤으로 상속받을 컴포넌트 클래스 타입</typeparam>
public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    // 멀티스레드 환경에서 동시 접근 시 중복 인스턴스 생성을 막기 위한 동기화 락 객체
    private static readonly object _lock = new object();

    // 애플리케이션이 종료 중일 때 새 인스턴스가 씬에 생성되는 것을 방지하는 플래그
    private static bool _isQuitting = false;

    /// <summary>
    /// 전역에서 접근 가능한 정적 인스턴스 프로퍼티
    /// </summary>
    public static T Instance
    {
        get
        {
            // 게임이 종료되는 중이라면 유령 인스턴스를 만들지 않고 null 반환
            if (_isQuitting)
            {
                Debug.LogWarning($"[Singleton] 애플리케이션이 종료 중이므로 {typeof(T).Name} 인스턴스에 접근할 수 없습니다.");
                return null;
            }

            lock (_lock)
            {
                if (_instance == null)
                {
                    // 현재 씬에 이미 컴포넌트가 배치되어 있는지 탐색
                    _instance = FindFirstObjectByType<T>();

                    // 씬에 없다면 새로운 게임오브젝트를 생성하여 컴포넌트 자동 부착
                    if (_instance == null)
                    {
                        GameObject singletonObject = new GameObject(typeof(T).Name);
                        _instance = singletonObject.AddComponent<T>();

                        // 씬 전환 시 오브젝트가 파괴되지 않도록 설정
                        DontDestroyOnLoad(singletonObject);
                    }
                }
                return _instance;
            }
        }
    }

    /// <summary>
    /// 수동으로 씬에 배치되었거나 Awake 시점에 중복 검사 및 씬 보존 처리
    /// </summary>
    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;

            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (_instance != this)
        {
            // 이미 다른 인스턴스가 존재하는데 중복 생성된 경우 즉시 파괴
            Debug.LogWarning($"[Singleton] {typeof(T).Name} 인스턴스가 씬에 이미 존재하여 중복 오브젝트를 파괴합니다: {gameObject.name}");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 애플리케이션 종료 감지
    /// </summary>
    protected virtual void OnApplicationQuit()
    {
        _isQuitting = true;
    }
}