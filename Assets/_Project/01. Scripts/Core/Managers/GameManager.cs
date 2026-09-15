using System;
using UnityEngine;

/// <summary>
/// 게임의 순수 진행 상태와 라이프사이클을 통괄하는 코어 매니저 클래스.
/// </summary>
public class GameManager : Singleton<GameManager>, IInitializable
{
    [Header("게임 상태")]
    [SerializeField] private GameState currentState = GameState.Title;

    // 프로퍼티
    public GameState CurrentState => currentState;      // 현재 게임 진행 상태
    public event Action<GameState> OnGameStateChanged;  // 게임 상태 변경 이벤트

    /// <summary>
    /// GameManager 초기화
    /// </summary>
    public void Initialize()
    {
        SceneLoadManager.Instance.OnSceneLoadCompleted += HandleSceneLoaded;

#if UNITY_EDITOR
        // 개발 중 테스트 씬이나 특정 씬을 켜둔 채로 바로 플레이했을 때 자동 보정
        string activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (activeSceneName != SceneType.Title.ToString())
        {
            ChangeState(GameState.InGame); 
            Debug.Log($"[GameManager] 에디터 테스트 감지: '{activeSceneName}' 씬에 맞춰 InGame 상태로 자동 시작합니다.");
        }
#endif

        Debug.Log("[GameManager] 초기화 완료 및 이벤트 바인딩 성공");
    }

    private void OnDestroy()
    {
        if (SceneLoadManager.Instance != null)
        {
            SceneLoadManager.Instance.OnSceneLoadCompleted -= HandleSceneLoaded;
        }
    }

    /// <summary>
    /// 게임의 런타임 진행 상태를 변경하고, 일시 정지 설정을 적용
    /// </summary>
    /// <param name="newState">전이할 새로운 GameState</param>
    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        Debug.Log($"[GameManager] GameState 변경 -> {newState}");

        // 상태별 엔진 시간 배속 및 물리 연산 일시정지 제어
        switch (newState)
        {
            case GameState.Paused:
                Time.timeScale = 0.0f;
                break;
            case GameState.InGame:
            case GameState.Title:
            case GameState.GameOver:
            default:
                Time.timeScale = 1.0f;
                break;
        }

        OnGameStateChanged?.Invoke(newState);
    }

    /// <summary>
    /// 인게임 플레이 중 일시정지 상태를 토글
    /// </summary>
    public void TogglePause()
    {
        if (currentState == GameState.InGame)
            ChangeState(GameState.Paused);
        else if (currentState == GameState.Paused)
            ChangeState(GameState.InGame);
    }

    /// <summary>
    /// SceneLoadManager의 씬 로딩 완료 이벤트를 수신하여 알맞은 기본 GameState로 전환
    /// </summary>
    /// <param name="loadedScene">로드가 완료된 대상 SceneType</param>
    private void HandleSceneLoaded(SceneType loadedScene)
    {
        Time.timeScale = 1.0f;

        switch (loadedScene)
        {
            case SceneType.Title:
                ChangeState(GameState.Title);
                break;
            case SceneType.MainGame:
                ChangeState(GameState.InGame);
                break;
        }
    }
}