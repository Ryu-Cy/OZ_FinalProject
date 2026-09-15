using UnityEngine;

/// <summary>
/// 씬 로드 이전 시점에 실행되어 핵심 매니저 및 코어 시스템 프리팹을 자동 생성하는 클래스.
/// </summary>
public static class Bootstrapper
{
    // 첫 번째 씬이 로드되기 전(Awake보다 앞선 시점)에 런타임 엔진에 의해 자동 실행
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Execute()
    {
        // GameManager 인스턴스가 아직 씬에 없는 경우에만 프리팹 로드 시도
        if (Object.FindFirstObjectByType<GameManager>() == null)
        {
            // Assets/Resources/CoreSystems.prefab 경로에서 프리팹 탐색
            GameObject corePrefab = Resources.Load<GameObject>("CoreSystems");

            if (corePrefab != null)
            {
                // 프리팹 인스턴스화
                GameObject coreInstance = Object.Instantiate(corePrefab);
                coreInstance.name = "[CoreSystems]";

                Debug.Log("[Bootstrapper] CoreSystems 프리팹 초기화가 성공적으로 완료되었습니다.");
            }
            else
            {
                Debug.LogWarning("[Bootstrapper] 'Resources/CoreSystems' 프리팹을 찾을 수 없습니다. 코어 매니저들이 런타임에 동적으로 개별 생성될 수 있습니다.");
            }
        }
    }
}