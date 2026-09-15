using UnityEngine;

/// <summary>
/// CoreSystems 프리팹의 루트 컴포넌트. <br/>
/// 하이어라키 자식 오브젝트 순서(0번부터 순차적)대로 IInitializable 매니저들을 동기식 초기화합니다.
/// </summary>
public class CoreSystems : MonoBehaviour
{
    private void Awake()
    {
        // 최상위 루트 오브젝트 씬 전환 보존
        DontDestroyOnLoad(gameObject);

        // 자식 오브젝트 순환하며 순서대로 Initialize 호출
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            if (child.TryGetComponent<IInitializable>(out var initializable))
            {
                initializable.Initialize();
            }
        }

        Debug.Log("[CoreSystems] 모든 하위 매니저의 순차적 초기화 완료");
    }
}