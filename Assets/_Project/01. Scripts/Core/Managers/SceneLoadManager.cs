using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환 및 비동기 로딩 연출을 전담하는 매니저 클래스.
/// </summary>
public class SceneLoadManager : Singleton<SceneLoadManager>, IInitializable
{
    [Header("현재 씬 모니터링")]
    [SerializeField] private SceneType currentScene = SceneType.Title;

    // 프로퍼티
    public SceneType CurrentScene => currentScene;          // 현재 로드된 씬 타입
    public event Action<SceneType> OnSceneLoadStarted;      // 씬 로딩 프로세스 시작 시점에 호출되는 이벤트
    public event Action<SceneType> OnSceneLoadCompleted;    // 씬 로딩 프로세스 완료 시점에 호출되는 이벤트

    /// <summary>
    /// SceneLoadManager 초기화
    /// </summary>
    public void Initialize()
    {
#if UNITY_EDITOR
        // 에디터에서 바로 재생 시 현재 활성화된 씬에 맞춰 CurrentScene 보정
        string activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (System.Enum.TryParse<SceneType>(activeSceneName, out var parsedScene))
        {
            // 현재 열린 씬 이름이 SceneType에 존재하면 해당 타입으로 지정
            currentScene = parsedScene;
        }
        else
        {
            // 목록에 없는 임의의 테스트 씬이라면 SceneType.Test로 지정
            currentScene = SceneType.Test;
        }

        Debug.Log($"[SceneLoadManager] 에디터 테스트 감지: CurrentScene -> {currentScene}");
#endif

        Debug.Log("[SceneLoadManager] 초기화 완료");
    }

    /// <summary>
    /// 지정한 씬으로 비동기 전환
    /// </summary>
    /// <param name="targetScene">전환하고자 하는 목표 SceneType</param>
    public void LoadScene(SceneType targetScene)
    {
        if (targetScene == SceneType.None || targetScene == SceneType.Length)
        {
            Debug.LogWarning("[SceneLoadManager] 유효하지 않은 SceneType입니다.");
            return;
        }

        StartCoroutine(LoadSceneRoutine(targetScene));
    }

    /// <summary>
    /// 비동기 씬 로드 및 화면 전환 연출 처리를 제어하는 코루틴
    /// </summary>
    /// <param name="targetScene">전환하고자 하는 목표 SceneType</param>
    private IEnumerator LoadSceneRoutine(SceneType targetScene)
    {
        OnSceneLoadStarted?.Invoke(targetScene);

        // 비동기 씬 로드
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(targetScene.ToString());
        while (!asyncOp.isDone)
        {
            yield return null;
        }

        // 씬 상태 갱신
        currentScene = targetScene;
        OnSceneLoadCompleted?.Invoke(targetScene);
    }

#if UNITY_EDITOR
    /// <summary>
    /// 씬 이름을 문자열로 직접 지정하여 로드
    /// </summary>
    /// <param name="sceneName">로드할 씬의 이름</param>
    public void LoadSceneByName(string sceneName)
    {
        StartCoroutine(LoadSceneByNameRoutine(sceneName));
    }

    private IEnumerator LoadSceneByNameRoutine(string sceneName)
    {
        OnSceneLoadStarted?.Invoke(SceneType.Test);

        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncOp.isDone)
        {
            yield return null;
        }

        OnSceneLoadCompleted?.Invoke(SceneType.Test);
    }
#endif
}